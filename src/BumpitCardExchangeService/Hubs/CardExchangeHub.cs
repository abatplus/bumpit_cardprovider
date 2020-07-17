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
            await Groups.AddToGroupAsync(Context.ConnectionId, deviceId);

            _repository.SaveSubscriber(deviceId, longitude, latitude, displayName);

            await Clients.Caller.Subscribed(_repository.GetNearestSubscribers(deviceId));
        }

        public async Task Unsubcribe(string deviceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, deviceId);

            _repository.DeleteSubscriber(deviceId);

            await Clients.Caller.Unsubscribed("Erfolgreich abgemeldet.");
        }

        public async Task Update(string deviceId, double longitude, double latitude, string displayName)
        {
            _repository.SaveSubscriber(deviceId, longitude, latitude, displayName);

            await Clients.Caller.Updated(_repository.GetNearestSubscribers(deviceId));
        }

        public async Task RequestCardExchange(string deviceId, string peerDeviceId, string displayName)
        {
            await Clients.Group(peerDeviceId).CardExchangeRequested(deviceId, displayName);

            await Clients.Caller.WaitingForAcceptance(peerDeviceId);
        }

        public async Task AcceptCardExchange(string deviceId, string peerDeviceId, string displayName, string cardData)
        {
            await Clients.Group(peerDeviceId).CardExchangeAccepted(peerDeviceId, displayName, cardData);

            await Clients.Caller.AcceptanceSent(deviceId);
        }

        public async Task SendCardData(string deviceId, string peerDeviceId, string displayName, string cardData)
        {
            await Clients.Group(peerDeviceId).CardDataReceived(deviceId, displayName, cardData);

            await Clients.Caller.CardDataSent(peerDeviceId);
        }
    }
}