using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BumpitCardExchangeService
{
  public interface ICardExchangeClient
  {
    Task Subscribed(IEnumerable<string> peers);
    Task Unsubscribed(string statusMessage);
    Task Updated(IEnumerable<string> peers);

    Task CardExchangeRequested(string deviceId, string displayName);
    Task WaitingForAcceptance(string peerDeviceId);

    Task CardExchangeRequestRevoked (string deviceId);
    Task RevokeSent(string peerDeviceId);

    Task CardExchangeAccepted(string peerDeviceId, string displayName, string cardData);
    Task AcceptanceSent(string deviceId);

    Task CardDataReceived(string deviceId, string displayName, string cardData);
    Task CardDataSent(string peerDeviceId);
  }
}