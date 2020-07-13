using System;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BumpitCardSwapService
{
    public class BumpitCardSwapHub : Hub<IBumpitCardSwapHub>
    {
        async Task SubcribeToSwap(SubscriptionData subsData)
        {
            //await Groups.AddToGroupAsync(Context.ConnectionId, boardId.ToString());

            List<JObject> list = new List<JObject>();

            await Clients.Caller.Subscribed(list);
        }

        async Task UnSubcribeFromSwap(SubscriptionData subsData)
        {
            List<JObject> list = new List<JObject>();

            await Clients.Caller.UnSubscribed(list);
        }

        async Task UpdateGeolocationData(string deviceId, double longitude, double latitude)
        {
            List<JObject> list = new List<JObject>();

            await Clients.Caller.GeolocationDataUpdated(list);
        }

        async Task UpdateSubscriptionData(string deviceId, string firstName, string lastName)
        {
            List<JObject> list = new List<JObject>();

            await Clients.Caller.SubscriptionDataUpdated(list);
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