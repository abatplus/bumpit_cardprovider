using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace BumpitCardExchangeService.Redis
{
    public interface ISubscriptionDataRepository
    {
        void SaveSubscriber(string deviceId, double longitude, double latitude, string displayName);
        void DeleteSubscriber(string deviceId);
        IEnumerable<string> GetNearestSubscribers(string device);
    }
}