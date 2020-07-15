using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using BumpitCardExchangeService.Redis;

namespace BumpitCardExchangeService
{
  public class CardExchangeHub : Hub<ICardExchangeClient>
  {
    private readonly ISubscriptionDataRepository _repository;

    public CardExchangeHub(ISubscriptionDataRepository repository)
    {
      _repository = repository;
    }

    public async Task Subscribe(string deviceId, double longitude, double latitude, string displayName)
    {
      _repository.SaveSubscriber(deviceId, longitude, latitude, displayName);

      await Clients.Caller.Subscribed(_repository.GetNearestSubscribers(deviceId));
    }

    public async Task Unsubcribe(string deviceId)
    {
      _repository.DeleteSubscriber(deviceId);

      await Clients.Caller.Unsubscribed("Erfolgreich abgemeldet.");
    }

    public async Task UpdateGeolocation(string deviceId, double longitude, double latitude)
    {
      _repository.UpdateGeolocation(deviceId, longitude, latitude);

      await Clients.Caller.GeolocationChanged(_repository.GetNearestSubscribers(deviceId));
    }

    public async Task UpdateDisplayName(string deviceId, string displayName)
    {
      _repository.UpdateSubcriberDescription(deviceId, displayName);

      await Clients.Caller.DisplayNameChanged("Ihre Anzeigedaten wurden erfolgreich geändert.");
    }

    // Workflow:
    // Client A (device)   Client B (peerDevice)      
    // -----------------------------------
    // Request (B)
    //   ------------------->Requested (A)
    //   ->Waiting (B)
    //
    //                         Accept (A)
    // Accepted (B)<--------------------
    //                  AcceptanceSent<-
    //
    // Send (B)
    //   ------------------->Received (A)
    //   -> Sent()

    public async Task RequestCardExchange(string deviceId, string peerDeviceId, string displayName)
    {
      //TODO: send to deviceIdOfCardOnwer
      await Clients.Client(peerDeviceId).CardExchangeRequested(deviceId, displayName);

      //TODO: send to deviceIdCaller request confirmation
      await Clients.Caller.WaitingForAcceptance(peerDeviceId, displayName);
    }

    public async Task AcceptCardExchange(string deviceId, string peerDeviceId, string displayName)
    {
      await Clients.Client(deviceId).CardExchangeAccepted(peerDeviceId, displayName);

      await Clients.Caller.AcceptanceSent(deviceId, displayName);
    }

    public async Task SendCardData(string deviceId, string peerDeviceId, string displayName, string cardData)
    {
      //TODO: check that deviceIdRecipient was published data to deviceIdCaller
      await Clients.Client(peerDeviceId).CardDataReceived(deviceId, displayName, cardData);

      await Clients.Caller.CardDataSent(peerDeviceId, displayName);
    }
  }
}