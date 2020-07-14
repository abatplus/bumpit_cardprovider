using System;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BumpitCardExchangeService.Redis;
using Newtonsoft.Json;

namespace BumpitCardExchangeService
{
    public class BumpitCardExchangeHub : Hub<IBumpitCardExchangeHub>
    {
        private readonly ISubscriptionDataRepository _repository;

        public BumpitCardExchangeHub(ISubscriptionDataRepository repository)
        {
            _repository = repository;
        }

        async Task SubcribeToCardExchange(string deviceId, double longitude, double latitude, string subcriberDescription)
        {
            _repository.SaveSubscriber(deviceId, longitude,latitude, subcriberDescription);

            await Clients.Caller.Subscribed(_repository.GetNearestSubscribers(deviceId));
        }

        async Task UnsubcribeFromCardExchange(string deviceId)
        {
            _repository.DeleteSubscriber(deviceId);
            await Clients.Caller.UnSubscribed("Erfolgreich abgemeldet.");
        }

        async Task UpdateGeolocation(string deviceId, double longitude, double latitude)
        {
            _repository.UpdateGeolocation(deviceId, longitude, latitude);
            await Clients.Caller.GeolocationDataUpdated(_repository.GetNearestSubscribers(deviceId));
        }

        async Task UpdateSubcriberDescription(string deviceId, string subcriberDescription)
        {
            _repository.UpdateSubcriberDescription(deviceId, subcriberDescription);
            await Clients.Caller.SubscriptionDataUpdated("message text");
        }

        async Task RequestCardExchange(string deviceIdCaller, string deviceIdOfCardOnwer)
        {

            //TODO
            await Clients.Caller.ReceivedExchangeRequest(deviceIdOfCardOnwer, String.Empty);
        }


        async Task SendCardDataToConfirmedRecipient(string deviceIdCaller, string deviceIdRecipient, string visitCardOfCaller)
        {
            //TODO
            await Clients.Caller.ReceivedCardData(String.Empty, String.Empty);
        }

    }
}