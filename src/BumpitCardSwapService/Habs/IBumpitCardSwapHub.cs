using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BumpitCardSwapService
{
    public interface IBumpitCardSwapHub
    {
        Task Subscribed(IEnumerable<JObject> subscribedDevices);

        Task UnSubscribed(IEnumerable<JObject> subscribedDevices);

        Task GeolocationDataUpdated(IEnumerable<JObject> subscribedDevices);

        Task SubscriptionDataUpdated(IEnumerable<JObject> subscribedDevices);
        
        Task ReceivedExchangeRequest(SubscriptionData senderData);

        Task ReceivedCardData(string idSender, string cardDataSender);
    }
}