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

        public async Task Subscribe(string deviceId, double longitude, double latitude, string displayName, string image)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, deviceId)
            .ContinueWith(async _ => await _repository.SaveSubscriber(deviceId, longitude, latitude, displayName, image))
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
            await _repository.SaveSubscriber(deviceId, longitude, latitude, displayName, null)
            .ContinueWith(async x => Clients.Caller.Updated(await _repository.GetNearestSubscribers(deviceId)));
        }

        public async Task RequestCardExchange(string deviceId, string peerDeviceId, string displayName)
        {
            await Clients.Group(peerDeviceId).CardExchangeRequested(deviceId, displayName, await _repository.GetThumbnailUrl(deviceId))
            .ContinueWith(_ => Clients.Caller.WaitingForAcceptance(peerDeviceId));
        }
        public async Task RevokeCardExchangeRequest(string deviceId, string peerDeviceId)
        {
            await Clients.Group(peerDeviceId).CardExchangeRequestRevoked(deviceId)
            .ContinueWith(_ => Clients.Caller.RevokeSent(peerDeviceId));
        }

        public async Task AcceptCardExchange(string deviceId, string peerDeviceId, string peerDisplayName, string peerCardData)
        {
            await Clients.Group(deviceId).CardExchangeAccepted(peerDeviceId, peerDisplayName, peerCardData, await _repository.GetSubscriberImage(peerDeviceId))
            .ContinueWith(_ => Clients.Caller.AcceptanceSent(deviceId));
        }

        public async Task SendCardData(string deviceId, string peerDeviceId, string displayName, string cardData)
        {
            await Clients.Group(peerDeviceId).CardDataReceived(deviceId, displayName, cardData, await _repository.GetSubscriberImage(deviceId))
            .ContinueWith(_ => Clients.Caller.CardDataSent(peerDeviceId));
        }
    }
}