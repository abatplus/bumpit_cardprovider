using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BumpitCardExchangeService
{
    public interface IBumpitCardExchangeHub
    {
        Task Subscribed(IEnumerable<string> nearestSubscribers);

        Task Unsubscribed(string statusMessage);

        Task GeolocationChanged(IEnumerable<string> nearestSubscribers);

        Task SubscriptionPublicInfoChanged(string statusMessage);
        
        Task CardExchangeRequesting(string deviceIdSender, string senderDescription);

        Task WaitingOfCardData(string statusMessage);

        Task CardDataReceived(string deviceIdSender, string cardDataSender);
    }
}