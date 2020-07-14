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
            await Clients.Caller.Unsubscribed("Erfolgreich abgemeldet.");
        }

        async Task UpdateGeolocation(string deviceId, double longitude, double latitude)
        {
            _repository.UpdateGeolocation(deviceId, longitude, latitude);
            await Clients.Caller.GeolocationChanged(_repository.GetNearestSubscribers(deviceId));
        }

        async Task UpdateSubcriberDescription(string deviceId, string subcriberDescription)
        {
            _repository.UpdateSubcriberDescription(deviceId, subcriberDescription);
            await Clients.Caller.SubscriptionPublicInfoChanged("Ihre Anzeigedaten wurden erfolgreich geändert.");
        }

        async Task RequestCardExchange(string deviceIdCaller, string deviceIdOfCardOnwer)
        {
            //TODO: send to deviceIdOfCardOnwer
            await Clients.Caller.CardExchangeRequesting(deviceIdOfCardOnwer, "TODO: send owner data");

            //TODO: send to deviceIdCaller request confirmation
            await Clients.Caller.WaitingOfCardData("Warte auf Bestätigung");
        }

        async Task SendCardDataToConfirmedRecipient(string deviceIdCaller, string deviceIdRecipient, string visitCardOfCaller)
        {
            //TODO: check that deviceIdRecipient was published data to deviceIdCaller
            await Clients.Caller.CardDataReceived(deviceIdCaller, visitCardOfCaller);
        }

    }
}