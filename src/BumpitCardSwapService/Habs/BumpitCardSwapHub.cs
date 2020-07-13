using System;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BumpitCardSwapService.Redis;
using Newtonsoft.Json;

namespace BumpitCardSwapService
{
    public class BumpitCardSwapHub : Hub<IBumpitCardSwapHub>
    {
        private readonly ISubscriptionDataRepository _repository;

        public BumpitCardSwapHub(ISubscriptionDataRepository repository)
        {
            _repository = repository;
        }

        async Task SubcribeToSwap(SubscriptionData subsData)
        {
            _repository.SaveSubscriber(subsData);

            await Clients.Caller.Subscribed(_repository.GetAllSubscribers(subsData.DeviceId));
        }

        async Task UnSubcribeFromSwap(string deviceId)
        {
            _repository.DeleteSubscriber(deviceId);
            await Clients.Caller.UnSubscribed(_repository.GetAllSubscribers(deviceId));
        }

        async Task UpdateGeolocationData(string deviceId, double longitude, double latitude)
        {
            _repository.UpdateGeolocationData(deviceId, longitude, latitude);
            await Clients.Caller.GeolocationDataUpdated(_repository.GetAllSubscribers(deviceId));
        }

        async Task UpdateSubscriptionData(string deviceId, string firstName, string lastName)
        {
            _repository.UpdateSubscriptionData(deviceId, firstName, lastName);
            await Clients.Caller.SubscriptionDataUpdated(_repository.GetAllSubscribers(deviceId));
        }

        async Task StartExchangeRequest(string deviceIdSender, string deviceIdRecipient)
        {

            //TODO
            await Clients.Caller.ReceivedExchangeRequest(new SubscriptionData());
        }

        async Task AcceptExchangeRequest(string deviceIdSender, string deviceIdRecipient, string cardDataSender)
        {
            //TODO
            await Clients.Caller.ReceivedCardData(String.Empty, String.Empty);
        }

    }
}