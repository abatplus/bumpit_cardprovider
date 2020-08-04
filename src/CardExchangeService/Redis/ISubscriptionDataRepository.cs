using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CardExchangeService.Redis
{
    public interface ISubscriptionDataRepository
    {
        Task<bool> SaveSubscriber(string deviceId, double longitude, double latitude, string displayName, string image);
        Task<bool> DeleteSubscriber(string deviceId);
        Task<IList<string>> GetNearestSubscribers(string deviceId);
        Task<string> GetThumbnailUrl(string deviceId);
        Task<string> GetSubscriberImage(string deviceId);
    }
}