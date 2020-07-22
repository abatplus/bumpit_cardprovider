using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardExchangeService.Redis
{
    public class SubscriptionDataRepository : ISubscriptionDataRepository
    {
        private readonly IRedisClient redisClient;

        public SubscriptionDataRepository(IRedisClient redisClient)
        {
            this.redisClient = redisClient;
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
                            resList.Add(JsonConvert.SerializeObject(
                                new SubscriptionData() { DeviceId = el.Member, DisplayName = subscData }
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

        public async Task<bool> SaveSubscriber(string deviceId, double longitude, double latitude, string displayName)
        {
            return await await redisClient.SetString(deviceId, displayName).ContinueWith(
               x => redisClient.GeoAdd(longitude, latitude, deviceId));
        }

        public async Task<bool> DeleteSubscriber(string deviceId)
        {
            return await await redisClient.RemoveKey(deviceId).ContinueWith(
                x => redisClient.GeoRemove(deviceId));
        }
    }
}