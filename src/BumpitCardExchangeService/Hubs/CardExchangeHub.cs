using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using BumpitCardExchangeService.Redis;

namespace BumpitCardExchangeService
{
    public class CardExchangeHub : Hub<ICardExchangeClient>, ICardExchangeHub
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

        //TODO: Subscribe and Update do equal things=> to make one method?
        public async Task Update(string deviceId, double longitude, double latitude, string displayName)
        {
            _repository.SaveSubscriber(deviceId, longitude, latitude, displayName);

            await Clients.Caller.Updated(_repository.GetNearestSubscribers(deviceId));
        }

        public async Task RequestCardExchange(string deviceId, string peerDeviceId, string displayName)
        {
            //TODO: send to deviceIdOfCardOnwer
            await Clients.Client(peerDeviceId).CardExchangeRequested(deviceId, displayName);

      //TODO: send to deviceIdCaller request confirmation
      await Clients.Caller.WaitingForAcceptance(peerDeviceId);
    }

    public async Task RevokeCardExchangeRequest(string deviceId, string peerDeviceId)
    {
      await Clients.Client(peerDeviceId).CardExchangeRequestRevoked(deviceId);

      await Clients.Caller.RevokeSent(peerDeviceId);
    }

    public async Task AcceptCardExchange(string deviceId, string peerDeviceId, string displayName, string cardData)
    {
      await Clients.Client(deviceId).CardExchangeAccepted(peerDeviceId, displayName, cardData);

      await Clients.Caller.AcceptanceSent(deviceId);
    }

        public async Task SendCardData(string deviceId, string peerDeviceId, string displayName, string cardData)
        {
            //TODO: check that deviceIdRecipient was published data to deviceIdCaller
            await Clients.Client(peerDeviceId).CardDataReceived(deviceId, displayName, cardData);

      await Clients.Caller.CardDataSent(peerDeviceId);
    }
  }
}