using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace BumpitCardExchangeService.Redis
{
    public interface ISubscriptionDataRepository
    {
        void SaveSubscriber(string deviceId, double longitude, double latitude, string subcriberDescription);
        void DeleteSubscriber(string deviceId);
        IEnumerable<string> GetNearestSubscribers(string device);
        void UpdateGeolocation(string deviceId, double longitude, double latitude);
        void UpdateSubcriberDescription(string deviceId, string subcriberDescription);
    }
}