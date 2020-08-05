using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
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

            //TODO : this solution dont work. the key that stores a path is at that time already deleted
            //redisClient.KeyDeletedEvent += DeleteSubscriberImages;
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
                                string thumbnailUrl = string.Empty;
                                try
                                {
                                    thumbnailUrl = !string.IsNullOrWhiteSpace(imageData?.ThumbnailFilePath)
                                        ? imageFileService.GetUrlFromPath(imageData?.ThumbnailFilePath)
                                        : string.Empty;
                                }
                                catch (Exception e)
                                {
                                    thumbnailUrl = string.Empty;
                                    //TODO Log error 
                                }

                                resList.Add(JsonConvert.SerializeObject(
                                    new SubscriptionData()
                                    {
                                        DeviceId = el.Member,
                                        DisplayName = imageData?.DisplayName,
                                        ThumbnailUrl = thumbnailUrl
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
            ImageData imageData = new ImageData()
            {
                DeviceId = deviceId,
                DisplayName = displayName
            };
            if (!string.IsNullOrEmpty(image))
            {
                imageFileService.SaveImageToFile(image, out var imageFilePath, out var thumbFilePath);
                imageData.ImageFilePath = imageFilePath ?? string.Empty;
                imageData.ThumbnailFilePath = thumbFilePath ?? string.Empty;
            }
            else
            {
                var serverImageData = await GetImageData(deviceId);
                imageData.ImageFilePath = serverImageData?.ImageFilePath ?? string.Empty;
                imageData.ThumbnailFilePath = serverImageData?.ThumbnailFilePath ?? string.Empty;
            }

            return await await redisClient.SetString(deviceId, JsonConvert.SerializeObject(imageData)).ContinueWith(
               x => redisClient.GeoAdd(longitude, latitude, deviceId));
        }

        public async Task<bool> DeleteSubscriber(string deviceId)
        {
            DeleteSubscriberImages(deviceId);

            return await await redisClient.RemoveKey(deviceId).ContinueWith(
                x => redisClient.GeoRemove(deviceId));
        }

        private async void DeleteSubscriberImages(string deviceId)
        {
            var imageData = await GetImageData(deviceId);

            imageFileService.DeleteImageFile(imageData?.ImageFilePath);
            imageFileService.DeleteImageFile(imageData?.ThumbnailFilePath);
        }

        public async Task<string> GetThumbnailUrl(string deviceId)
        {
            return imageFileService.GetUrlFromPath(await GetThumbnailPath(deviceId));
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