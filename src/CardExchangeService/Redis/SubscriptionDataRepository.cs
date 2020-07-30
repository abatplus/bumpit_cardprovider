using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardExchangeService.Services;

namespace CardExchangeService.Redis
{
    public class SubscriptionDataRepository : ISubscriptionDataRepository
    {
        private readonly IRedisClient redisClient;
        private readonly IImageFileService imageFileService;

        public SubscriptionDataRepository(IRedisClient redisClient, IImageFileService imageFileService)
        {
            this.redisClient = redisClient;
            this.imageFileService = imageFileService;
        }

        public async Task<IList<string>> GetNearestSubscribers(string deviceId)
        {
            List<string> resList = new List<string>();

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return resList;
            }

            try
            {
                var res = await redisClient.GeoRadiusByMember(deviceId);
                if (res != null)
                {
                    foreach (var el in res)
                    {
                        if (el.Member != deviceId)
                        {
                            string subscData = await redisClient.GetString(el.Member);
                            var imageData = JsonConvert.DeserializeObject<ImageData>(subscData);
                            resList.Add(JsonConvert.SerializeObject(
                                new SubscriptionData() { DeviceId = el.Member, DisplayName = imageData.DisplayName }
                            ));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //TODO Log error 
            }

            return resList;
        }

        public async Task<bool> SaveSubscriber(string deviceId, double longitude, double latitude, string displayName, string image)
        {
            string imageFilePath = String.Empty;
            if (!string.IsNullOrEmpty(image))
            {
                imageFilePath = imageFileService.SaveImageToFile(image);
            }

            return await await redisClient.SetString(deviceId, JsonConvert.SerializeObject(new ImageData()
            {
                DisplayName = displayName,
                FilePath = imageFilePath
            })).ContinueWith(
               x => redisClient.GeoAdd(longitude, latitude, deviceId));
        }

        public async Task<bool> DeleteSubscriber(string deviceId)
        {
            return await await redisClient.RemoveKey(deviceId).ContinueWith(
                x => redisClient.GeoRemove(deviceId));
        }

        public async Task<string> GetThumbnailUrl(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return string.Empty;
            }

            string resUrl = string.Empty;

            try
            {
                string imageDataStr = await redisClient.GetString(deviceId);

                var imageData = JsonConvert.DeserializeObject<ImageData>(imageDataStr);

                resUrl = imageData.FilePath?.Replace("\\", "/");
            }
            catch (Exception e)
            {
                //TODO Log error 
            }

            return resUrl;
        }
    }
}