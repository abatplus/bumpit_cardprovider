using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace BumpitCardSwapService.Redis
{
    public interface ISubscriptionDataRepository
    {
        void SaveSubscriber(SubscriptionData subsData);
        void DeleteSubscriber(string deviceId);
        IEnumerable<JObject> GetAllSubscribers(string device);
        void UpdateGeolocationData(string deviceId, double longitude, double latitude);
        void UpdateSubscriptionData(string deviceId, string firstName, string lastName);
    }
}