using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace BumpitCardExchangeService.Redis
{
    public class SubscriptionDataRepository : ISubscriptionDataRepository
    {
        private readonly IRedisClient redisClient;

        public SubscriptionDataRepository(IRedisClient redisClient)
        {
            this.redisClient = redisClient;
        }

        public IEnumerable<string> GetNearestSubscribers(string device)
        {
            List<string> resList = new List<string>();

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
                        resList.Add(JsonConvert.SerializeObject(
                            new SubscriptionData() { DeviceId = el.Member, AdditionalData = subscData }
                        ));
                    }
                }
            }

            return resList;
        }

        public async void SaveSubscriber(string deviceId, double longitude, double latitude, string subcriberDescription)
        {
            await redisClient.SetString(deviceId, subcriberDescription);
            await redisClient.GeoAdd(GetGeoEntryKey(), longitude, latitude, deviceId);
        }

        public async void DeleteSubscriber(string deviceId)
        {
            await redisClient.RemoveKey(deviceId);
            redisClient.GeoRemove(GetGeoEntryKey(), deviceId);
        }

        private string GetGeoEntryKey()
        {
            return "BumpitGeoEntryKey";
        }

        public async void UpdateGeolocation(string deviceId, double longitude, double latitude)
        {
            redisClient.GeoRemove(GetGeoEntryKey(), deviceId);
            await redisClient.GeoAdd(GetGeoEntryKey(), longitude, latitude, deviceId);
        }

        public async void UpdateSubcriberDescription(string deviceId, string subcriberDescription)
        {
            await redisClient.SetString(deviceId, subcriberDescription);
        }


    }
}