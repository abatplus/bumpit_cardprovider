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

            redisClient.KeyDeletedEvent += async id =>
            {
                //TODO: the key that stores a path is already deleted
                imageFileService.DeleteImageFile(await GetThumbnailPath(id));
            };
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
                            if (!string.IsNullOrWhiteSpace(subscData))
                            {
                                var imageData = JsonConvert.DeserializeObject<ImageData>(subscData);
                                resList.Add(JsonConvert.SerializeObject(
                                    new SubscriptionData()
                                    {
                                        DeviceId = el.Member,
                                        DisplayName = imageData?.DisplayName,
                                        ThumbnailUrl = GetUrlFromPath(imageData?.ThumbnailFilePath)
                                    }
                                ));
                            }
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
            string thumnbnailFilePath = String.Empty;
            if (!string.IsNullOrEmpty(image))
            {
                imageFileService.SaveImageToFile(image, out imageFilePath, out thumnbnailFilePath);
            }

            return await await redisClient.SetString(deviceId, JsonConvert.SerializeObject(new ImageData()
            {
                DeviceId = deviceId,
                DisplayName = displayName,
                ImageFilePath = imageFilePath,
                ThumbnailFilePath = thumnbnailFilePath
            })).ContinueWith(
               x => redisClient.GeoAdd(longitude, latitude, deviceId));
        }

        public async Task<bool> DeleteSubscriber(string deviceId)
        {
            var imageData = await GetImageData(deviceId);

            imageFileService.DeleteImageFile(imageData?.ImageFilePath);
            imageFileService.DeleteImageFile(imageData?.ThumbnailFilePath);

            return await await redisClient.RemoveKey(deviceId).ContinueWith(
                x => redisClient.GeoRemove(deviceId));
        }

        public async Task<string> GetThumbnailUrl(string deviceId)
        {
            return GetUrlFromPath(await GetThumbnailPath(deviceId));
        }

        private string GetUrlFromPath(string filePath)
        {
            return filePath?.Replace("\\", "/");
        }

        private async Task<string> GetThumbnailPath(string deviceId)
        {
            return (await GetImageData(deviceId))?.ThumbnailFilePath;
        }

        private async Task<string> GetImagePath(string deviceId)
        {
            return (await GetImageData(deviceId))?.ImageFilePath;
        }

        private async Task<ImageData> GetImageData(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return null;
            }

            ImageData res = null;

            try
            {
                res = JsonConvert.DeserializeObject<ImageData>(await redisClient.GetString(deviceId));
            }
            catch (Exception e)
            {
                //TODO Log error 
            }

            return res;
        }

        public async Task<string> GetSubscriberImage(string deviceId)
        {
            return imageFileService.GetImage(await GetImagePath(deviceId));
        }
    }
}