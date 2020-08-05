using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardExchangeService.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace CardExchangeService.Redis
{
    public class SubscriptionDataRepository : ISubscriptionDataRepository
    {
        private readonly IRedisClient _redisClient;
        private readonly IImageFileService _imageFileService;

        private readonly int _redisKeyExpireTimeout;

        private readonly ConcurrentDictionary<string, DelayTimer> _deleteTimers = new ConcurrentDictionary<string, DelayTimer>();

        public SubscriptionDataRepository(IRedisClient redisClient, IImageFileService imageFileService, IConfiguration config)
        {
            _redisClient = redisClient;
            _imageFileService = imageFileService;

            _redisKeyExpireTimeout = Convert.ToInt32(config["REDIS_KEY_EXPIRE_TIMEOUT"] ?? config["Redis:KeyExpireTimeout_s"]);
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
                var res = await _redisClient.GeoRadiusByMember(deviceId);
                if (res != null)
                {
                    foreach (var el in res)
                    {
                        if (el.Member != deviceId)
                        {
                            string subscData = await _redisClient.GetString(el.Member);
                            if (!string.IsNullOrWhiteSpace(subscData))
                            {
                                var imageData = JsonConvert.DeserializeObject<ImageData>(subscData);
                                resList.Add(JsonConvert.SerializeObject(
                                    new SubscriptionData()
                                    {
                                        DeviceId = el.Member,
                                        DisplayName = imageData?.DisplayName,
                                        ThumbnailUrl = _imageFileService.GetUrlFromPath(imageData?.ThumbnailFilePath)
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
            ImageData imageData = new ImageData
            {
                DeviceId = deviceId,
                DisplayName = displayName
            };

            if (string.IsNullOrEmpty(image))
            {
                var serverImageData = await GetImageData(deviceId);
                imageData.ImageFilePath = serverImageData?.ImageFilePath;
                imageData.ThumbnailFilePath = serverImageData?.ThumbnailFilePath;
            }
            else
            {                
                _imageFileService.SaveImageToFile(image, out var imageFilePath, out var thumbFilePath);
                imageData.ImageFilePath = imageFilePath;
                imageData.ThumbnailFilePath = thumbFilePath;
            }

            if(!_deleteTimers.ContainsKey(deviceId))
            {
                _deleteTimers.TryAdd(deviceId, new DelayTimer(_=> DeleteImages(deviceId), null, _redisKeyExpireTimeout));
            }
            
            _deleteTimers[deviceId].Invoke();
            
            return await await _redisClient.SetString(deviceId, JsonConvert.SerializeObject(imageData)).ContinueWith(
               x => _redisClient.GeoAdd(longitude, latitude, deviceId));
        }

        private void DeleteImages(string deviceId)
        {
            DeleteSubscriberImages(deviceId);

            _deleteTimers[deviceId].Dispose();
            _deleteTimers[deviceId] = null;
        }

        public async Task<bool> DeleteSubscriber(string deviceId)
        {
            DeleteSubscriberImages(deviceId);

            return await await _redisClient.RemoveKey(deviceId).ContinueWith(
                x => _redisClient.GeoRemove(deviceId));
        }

        private async void DeleteSubscriberImages(string deviceId)
        {
            var imageData = await GetImageData(deviceId);
            
            if(imageData == null)
                return;

            _imageFileService.DeleteImageFile(imageData.ImageFilePath);
            _imageFileService.DeleteImageFile(imageData.ThumbnailFilePath);
        }

        public async Task<string> GetThumbnailUrl(string deviceId)
        {
            return _imageFileService.GetUrlFromPath(await GetThumbnailPath(deviceId));
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
                res = JsonConvert.DeserializeObject<ImageData>(await _redisClient.GetString(deviceId));
            }
            catch (Exception e)
            {
                //TODO Log error 
            }

            return res;
        }

        public async Task<string> GetSubscriberImage(string deviceId)
        {
            return _imageFileService.GetImage(await GetImagePath(deviceId));
        }
    }
}