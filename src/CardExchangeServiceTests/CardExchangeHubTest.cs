using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Dynamic;
using CardExchangeService;
using CardExchangeService.Redis;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using FluentAssertions;
using System.Threading.Tasks;

namespace CardExchangeServiceTests
{
    public class CardExchangeHubTest
    {
        private const string deviceId1 = "d77b8214 - f7de - 4405 - abda - e87cfa05abac";
        private const string deviceId2 = "d77b8214 - f7de - 4405 - abda - e87cfa05abaa";
        private const string deviceId3 = "d77b8214 - f7de - 4405 - abda - e87cfa05abab";
        private const double latitude1 = 12.466561146;
        private const double latitudeNotIn2 = 12.496562656;
        private const double latitudeIn2 = 12.466561156;
        private const double longitude = -34.405804850;

        public CardExchangeHubTest()
        {
        }

        [Fact]
        public async void ConnectionTest_SubscribeUpdate2Subscribers_SubscribedCalled()
        {
            var connection1 = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/swaphub")
                .Build();
            var connection2 = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/swaphub")
                .Build();

            bool isSubscribedCalled1 = false;
            IEnumerable<string> resPeers1 = new List<string>();
            connection1.On(nameof(ICardExchangeClient.Subscribed), (IEnumerable<string> peers) =>
            {
                isSubscribedCalled1 = true;
                resPeers1 = peers;
            });
            bool isUpdatedCalled1 = false;
            connection1.On(nameof(ICardExchangeClient.Updated), (IEnumerable<string> peers) =>
            {
                isUpdatedCalled1 = true;
                resPeers1 = peers;
            });

            bool isSubscribedCalled2 = false;
            IEnumerable<string> resPeers2 = new List<string>();
            connection2.On(nameof(ICardExchangeClient.Subscribed), (IEnumerable<string> peers) =>
            {
                isSubscribedCalled2 = true;
                resPeers2 = peers;
            });


            bool isUpdatedCalled2 = false;
            connection2.On(nameof(ICardExchangeClient.Updated), (IEnumerable<string> peers) =>
            {
                isUpdatedCalled2 = true;
                resPeers2 = peers;
            });

            await connection1.StartAsync().ContinueWith(x =>
            {
                if (connection1.State == HubConnectionState.Connected)
                {
                    connection1.SendAsync("Subscribe", deviceId1, longitude, latitude1, "displayName1");
                }
            });

            await connection2.StartAsync().ContinueWith(x =>
            {
                if (connection2.State == HubConnectionState.Connected)
                {
                    connection2.SendAsync("Subscribe", deviceId2, longitude, latitudeIn2, "displayName2");

                }
            });

            await connection1.SendAsync("Update", deviceId1, longitude, latitude1, "displayName1");
            await connection2.SendAsync("Update", deviceId2, longitude, latitudeIn2, "displayName2");

            await Task.Delay(2000);

            isSubscribedCalled1.Should().BeTrue();
            isUpdatedCalled1.Should().BeTrue();
            resPeers1.Should().NotBeNull();
            resPeers1.Count().Should().Be(1);
            var data1 = JsonConvert.DeserializeObject<SubscriptionData>(((List<string>)resPeers1)[0]);
            data1.Should().NotBeNull();
            data1.Latitude.Should().Be(0);
            data1.Longitute.Should().Be(0);
            data1.DeviceId.Should().Be(deviceId2);
            data1.DisplayName.Should().Be("displayName2");


            isSubscribedCalled2.Should().BeTrue();
            isUpdatedCalled2.Should().BeTrue();
            resPeers2.Should().NotBeNull();
            var data2 = JsonConvert.DeserializeObject<SubscriptionData>(((List<string>)resPeers2)[0]);
            data2.Should().NotBeNull();
            data2.Latitude.Should().Be(0);
            data2.Longitute.Should().Be(0);
            data2.DeviceId.Should().Be(deviceId1);
            data2.DisplayName.Should().Be("displayName1");
        }

        [Fact]
        public async void ConnectionTest_Subscribe_SubscribedCalled()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/swaphub")
                .Build();

            bool isSubscribedCalled = false;
            IEnumerable<string> resPeers = new List<string>();
            connection.On(nameof(ICardExchangeClient.Subscribed), (IEnumerable<string> peers) =>
            {
                isSubscribedCalled = true;
                resPeers = peers;
            });

            await connection.StartAsync().ContinueWith(x =>
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    connection.SendAsync("Subscribe", deviceId1, longitude, latitude1, "displayName1");
                }
            });

            await Task.Delay(2000);

            isSubscribedCalled.Should().BeTrue();
            resPeers.Should().NotBeNull();
            resPeers.Count().Should().Be(0);
        }
        
        [Fact]
        public async void ConnectionTest_UnSubscribe_UnSubscribedCalled()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/swaphub")
                .Build();

            bool isSubscribedCalled = false;
            IEnumerable<string> resPeers = new List<string>();
            connection.On(nameof(ICardExchangeClient.Subscribed), (IEnumerable<string> peers) =>
            {
                isSubscribedCalled = true;
                resPeers = peers;
            });

            bool isUnSubscribedCalled = false;
            string statusMessageUnSubscribed = String.Empty;
            connection.On(nameof(ICardExchangeClient.Unsubscribed), (string statusMessage) =>
            {
                isUnSubscribedCalled = true;
                statusMessageUnSubscribed = statusMessage;
            });

            await connection.StartAsync().ContinueWith(async x =>
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    await connection.SendAsync("Subscribe", deviceId1, longitude, latitude1, "displayName1");
                }
            });

            await Task.Delay(2000);

            isSubscribedCalled.Should().BeTrue();
            resPeers.Should().NotBeNull();
            resPeers.Count().Should().Be(0);

            await connection.SendAsync("Unsubscribe", deviceId1);

            await Task.Delay(2000);

            isUnSubscribedCalled.Should().BeTrue();
            statusMessageUnSubscribed.Should().NotBeNullOrWhiteSpace();
        }
        
        [Fact]
        public async void ConnectionTest_Subscribe2Subscribers_ManyUpdates_NoFallOuts()
        {
            var connection1 = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/swaphub")
                .Build();
            var connection2 = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/swaphub")
                .Build();

            bool isSubscribedCalled1 = false;
            IEnumerable<string> resPeers1 = new List<string>();
            connection1.On(nameof(ICardExchangeClient.Subscribed), (IEnumerable<string> peers) =>
            {
                   isSubscribedCalled1 = true;
                resPeers1 = peers;
            });
            bool isUpdatedCalled1 = false;
            int iCallUpdate1 = 0;
            connection1.On(nameof(ICardExchangeClient.Updated), (IEnumerable<string> peers) =>
            {
                iCallUpdate1++;
                isUpdatedCalled1 = true;
                resPeers1 = peers;
            });

            bool isSubscribedCalled2 = false;
            IEnumerable<string> resPeers2 = new List<string>();
            connection2.On(nameof(ICardExchangeClient.Subscribed), (IEnumerable<string> peers) =>
            {
                isSubscribedCalled2 = true;
                resPeers2 = peers;
            });


            bool isUpdatedCalled2 = false;
            int iCallUpdate2 = 0;
            connection2.On(nameof(ICardExchangeClient.Updated), (IEnumerable<string> peers) =>
            {
                iCallUpdate2++;
                isUpdatedCalled2 = true;
                resPeers2 = peers;
            });

            await connection1.StartAsync().ContinueWith(x =>
            {
                if (connection1.State == HubConnectionState.Connected)
                {
                    connection1.SendAsync("Subscribe", deviceId1, longitude, latitude1, "displayName1");
                }
            });

            await connection2.StartAsync().ContinueWith(x =>
            {
                if (connection2.State == HubConnectionState.Connected)
                {
                    connection2.SendAsync("Subscribe", deviceId2, longitude, latitudeIn2, "displayName2");

                }
            });

            for (int i = 0; i < 50; i++)
            {
                connection1.SendAsync("Update", deviceId1, longitude, latitude1, "displayName1");
            }

            for (int i = 0; i < 50; i++)
            {
                connection2.SendAsync("Update", deviceId2, longitude, latitudeIn2, "displayName2");
            }

            await Task.Delay(2000);

            iCallUpdate1.Should().Be(50);
            isSubscribedCalled1.Should().BeTrue();
            isUpdatedCalled1.Should().BeTrue();
            resPeers1.Should().NotBeNull();
            resPeers1.Count().Should().Be(1);
            var data1 = JsonConvert.DeserializeObject<SubscriptionData>(((List<string>)resPeers1)[0]);
            data1.Should().NotBeNull();
            data1.Latitude.Should().Be(0);
            data1.Longitute.Should().Be(0);
            data1.DeviceId.Should().Be(deviceId2);
            data1.DisplayName.Should().Be("displayName2");

            iCallUpdate2.Should().Be(50);
            isSubscribedCalled2.Should().BeTrue();
            isUpdatedCalled2.Should().BeTrue();
            resPeers2.Should().NotBeNull();
            var data2 = JsonConvert.DeserializeObject<SubscriptionData>(((List<string>)resPeers2)[0]);
            data2.Should().NotBeNull();
            data2.Latitude.Should().Be(0);
            data2.Longitute.Should().Be(0);
            data2.DeviceId.Should().Be(deviceId1);
            data2.DisplayName.Should().Be("displayName1");
        }

    }
}
