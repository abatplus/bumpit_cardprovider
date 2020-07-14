using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BumpitCardExchangeService
{
    public interface IBumpitCardExchangeHub
    {
        Task Subscribed(IEnumerable<string> subscribedDevices);

        Task UnSubscribed(string message);

        Task GeolocationDataUpdated(IEnumerable<string> subscribedDevices);

        Task SubscriptionDataUpdated(string message);
        
        Task ReceivedExchangeRequest(string deviceIdSender, string senderData);

        Task ExchangeRequestStarted(string deviceIdRecip, string recipData);

        Task ReceivedCardData(string idSender, string cardDataSender);
    }
}