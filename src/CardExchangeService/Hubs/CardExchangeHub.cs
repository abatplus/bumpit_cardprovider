using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using CardExchangeService.Redis;

namespace CardExchangeService
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
            await Groups.AddToGroupAsync(Context.ConnectionId, deviceId)
            .ContinueWith(async _ => await _repository.SaveSubscriber(deviceId, longitude, latitude, displayName))
            .ContinueWith(async _ => Clients.Caller.Subscribed(await _repository.GetNearestSubscribers(deviceId)));
        }

        public async Task Unsubscribe(string deviceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, deviceId)
            .ContinueWith(async _ => await _repository.DeleteSubscriber(deviceId))
            .ContinueWith(_ => Clients.Caller.Unsubscribed("Erfolgreich abgemeldet."));
        }

        public async Task Update(string deviceId, double longitude, double latitude, string displayName)
        {
            await _repository.SaveSubscriber(deviceId, longitude, latitude, displayName).ContinueWith(async x =>
            Clients.Caller.Updated(await _repository.GetNearestSubscribers(deviceId)));
        }

        public async Task RequestCardExchange(string deviceId, string peerDeviceId, string displayName)
        {
            await Clients.Group(peerDeviceId).CardExchangeRequested(deviceId, displayName)
            .ContinueWith(_ => Clients.Caller.WaitingForAcceptance(peerDeviceId));
        }
        public async Task RevokeCardExchangeRequest(string deviceId, string peerDeviceId)
        {
            await Clients.Group(peerDeviceId).CardExchangeRequestRevoked(deviceId)
            .ContinueWith(_ => Clients.Caller.RevokeSent(peerDeviceId));
        }

        public async Task AcceptCardExchange(string deviceId, string peerDeviceId, string peerDisplayName, string peerCardData)
        {
            await Clients.Group(deviceId).CardExchangeAccepted(peerDeviceId, peerDisplayName, peerCardData)
            .ContinueWith(_ => Clients.Caller.AcceptanceSent(deviceId));
        }

        public async Task SendCardData(string deviceId, string peerDeviceId, string displayName, string cardData)
        {
            await Clients.Group(peerDeviceId).CardDataReceived(deviceId, displayName, cardData)
            .ContinueWith(_ => Clients.Caller.CardDataSent(peerDeviceId));
        }
    }
}