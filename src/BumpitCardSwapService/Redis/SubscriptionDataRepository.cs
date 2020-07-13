using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace BumpitCardSwapService.Redis
{
    public class SubscriptionDataRepository : ISubscriptionDataRepository
    {
        private readonly IRedisClient redisClient;

        public SubscriptionDataRepository(IRedisClient redisClient)
        {
            this.redisClient = redisClient;
        }

        public IEnumerable<JObject> GetAllSubscribers(string device)
        {
            List<JObject> resList = new List<JObject>();

            if (string.IsNullOrWhiteSpace(device))
            {
                return resList;
            }

            var res = redisClient.GeoRadiusByMember(GetGeoEntryKey(), device).Result;
            if (res != null)
            {
                foreach (var el in res)
                {
                    if (el.Member != device)
                    {
                        string subscData = redisClient.GetString(el.Member).Result;
                        JObject subscDataJson = JsonConvert.DeserializeObject<JObject>(subscData);
                        resList.Add(subscDataJson);
                    }
                }
            }

            return resList;
        }

        public async void SaveSubscriber(SubscriptionData subsData)
        {
            await redisClient.SetString(subsData.DeviceId, JsonConvert.SerializeObject(subsData));
            await redisClient.GeoAdd(GetGeoEntryKey(), subsData.Longitude, subsData.Latitude,
              subsData.DeviceId);
        }

        public async void DeleteSubscriber(string deviceId)
        {
            await redisClient.RemoveKey(deviceId);
            redisClient.GeoRemove(GetGeoEntryKey(), deviceId);
        }

        private string GetGeoEntryKey()
        {
            return nameof(SubscriptionData);
        }

        public async void UpdateGeolocationData(string deviceId, double longitude, double latitude)
        {
            redisClient.GeoRemove(GetGeoEntryKey(), deviceId);
            await redisClient.GeoAdd(GetGeoEntryKey(), longitude, latitude,deviceId);
        }

        public async void UpdateSubscriptionData(string deviceId, string firstName, string lastName)
        {
            await redisClient.SetString(deviceId, JsonConvert.SerializeObject(new SubscriptionData()
            {
                DeviceId = deviceId,
                FirstName = firstName,
                LastName = lastName
            }));
        }

       
    }
}