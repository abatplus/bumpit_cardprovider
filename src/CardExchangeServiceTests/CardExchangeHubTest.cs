using System;
using CardExchangeService;
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
        private const double latitude1 = 12.466561146;
        private const double latitudeIn2 = 12.466561156;
        private const double longitude = -34.405804850;

        private const string connectionUrl = "https://vswap-dev.smef.io/swaphub";
        //private const string connectionUrl = "http://localhost:5000/swaphub";

        public CardExchangeHubTest()
        {
        }

        [Fact]
        public async void ConnectionTest_SubscribeUpdate2Subscribers_SubscribedCalled()
        {
            var connection1 = new HubConnectionBuilder()
                .WithUrl(connectionUrl)
                .Build();
            var connection2 = new HubConnectionBuilder()
                .WithUrl(connectionUrl)
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
                .WithUrl(connectionUrl)
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
                .WithUrl(connectionUrl)
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
                .WithUrl(connectionUrl)
                .Build();
            var connection2 = new HubConnectionBuilder()
                .WithUrl(connectionUrl)
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

        [Fact]
        public async void ConnectionTest_Swap2Subscribers_SubscribedCalled()
        {
            //Connection
            var connection1 = new HubConnectionBuilder()
                .WithUrl(connectionUrl)
                .Build();
            var connection2 = new HubConnectionBuilder()
                .WithUrl(connectionUrl)
                .Build();

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


            // Events to handle
            //Connection 1
            string deviceIdReqCon1 = string.Empty;
            string displayNameCon1 = string.Empty;
            connection1.On(nameof(ICardExchangeClient.CardExchangeRequested), (string deviceId, string displayName) =>
            {
                deviceIdReqCon1 = deviceId;
                displayNameCon1 = displayName;

                connection1.SendAsync("AcceptCardExchange", deviceId, deviceId1, "displayName1", "cardData1");
            });
            string waitingForAcceptanceFromDeviceCon1 = string.Empty;
            connection1.On(nameof(ICardExchangeClient.WaitingForAcceptance), (string peerDeviceId) =>
            {
                waitingForAcceptanceFromDeviceCon1 = peerDeviceId;
            });


            string cardExchangeAcceptedPeerDeviceId1 = string.Empty;
            string cardExchangeAcceptedPeerDisplayName1 = string.Empty;
            string cardExchangeAcceptedPeerCardData1 = string.Empty;
            connection1.On(nameof(ICardExchangeClient.CardExchangeAccepted), (string peerDeviceId, string peerDisplayName, string peerCardData) =>
            {
                cardExchangeAcceptedPeerDeviceId1 = peerDeviceId;
                cardExchangeAcceptedPeerDisplayName1 = peerDisplayName;
                cardExchangeAcceptedPeerCardData1 = peerCardData;

                connection1.SendAsync("SendCardData", deviceId1, peerDeviceId, "displayName1", "cardData1");
            });
            string acceptanceSentDeviceCon1 = string.Empty;
            connection1.On(nameof(ICardExchangeClient.AcceptanceSent), (string deviceId) =>
            {
                acceptanceSentDeviceCon1 = deviceId;
            });

            string cardDataReceivedDeviceId1 = string.Empty;
            string cardDataReceivedDisplayName1 = string.Empty;
            string cardDataReceivedCardData1 = string.Empty;
            connection1.On(nameof(ICardExchangeClient.CardDataReceived), (string deviceId, string displayName, string cardData) =>
            {
                cardDataReceivedDeviceId1 = deviceId;
                cardDataReceivedDisplayName1 = displayName;
                cardDataReceivedCardData1 = cardData;
            });
            string cardDataSentDeviceCon1 = string.Empty;
            connection1.On(nameof(ICardExchangeClient.CardDataSent), (string peerDeviceId) =>
            {
                cardDataSentDeviceCon1 = peerDeviceId;
            });

            //Connection 2
            string deviceIdReqCon2 = string.Empty;
            string displayNameCon2 = string.Empty;
            connection2.On(nameof(ICardExchangeClient.CardExchangeRequested), (string deviceId, string displayName) =>
            {
                deviceIdReqCon2 = deviceId;
                displayNameCon2 = displayName;

                connection2.SendAsync("AcceptCardExchange", deviceId, deviceId2, "displayName2", "cardData2");
            });
            string waitingForAcceptanceFromDeviceCon2 = string.Empty;
            connection2.On(nameof(ICardExchangeClient.WaitingForAcceptance), (string peerDeviceId) =>
            {
                waitingForAcceptanceFromDeviceCon2 = peerDeviceId;
            });

            string cardExchangeAcceptedPeerDeviceId2 = string.Empty;
            string cardExchangeAcceptedPeerDisplayName2 = string.Empty;
            string cardExchangeAcceptedPeerCardData2 = string.Empty;
            connection2.On(nameof(ICardExchangeClient.CardExchangeAccepted), (string peerDeviceId, string peerDisplayName, string peerCardData) =>
            {
                cardExchangeAcceptedPeerDeviceId2 = peerDeviceId;
                cardExchangeAcceptedPeerDisplayName2 = peerDisplayName;
                cardExchangeAcceptedPeerCardData2 = peerCardData;

                connection2.SendAsync("SendCardData", deviceId2, peerDeviceId, "displayName2", "cardData2");
            });
            string acceptanceSentDeviceCon2 = string.Empty;
            connection2.On(nameof(ICardExchangeClient.AcceptanceSent), (string deviceId) =>
            {
                acceptanceSentDeviceCon2 = deviceId;
            });

            string cardDataReceivedDeviceId2 = string.Empty;
            string cardDataReceivedDisplayName2 = string.Empty;
            string cardDataReceivedCardData2 = string.Empty;
            connection2.On(nameof(ICardExchangeClient.CardDataReceived), (string deviceId, string displayName, string cardData) =>
            {
                cardDataReceivedDeviceId2 = deviceId;
                cardDataReceivedDisplayName2 = displayName;
                cardDataReceivedCardData2 = cardData;
            });
            string cardDataSentDeviceCon2 = string.Empty;
            connection2.On(nameof(ICardExchangeClient.CardDataSent), (string peerDeviceId) =>
            {
                cardDataSentDeviceCon2 = peerDeviceId;
            });

            // Actions
            await connection1.SendAsync("Update", deviceId1, longitude, latitude1, "displayName1");
            await connection2.SendAsync("Update", deviceId2, longitude, latitudeIn2, "displayName2");

            await connection1.SendAsync("RequestCardExchange", deviceId1, deviceId2, "displayName1");
            await connection2.SendAsync("RequestCardExchange", deviceId2, deviceId1, "displayName2");

            await Task.Delay(2000);

            //Asserts
            deviceIdReqCon1.Should().Be(deviceId2);
            displayNameCon1.Should().Be("displayName2");
            deviceIdReqCon2.Should().Be(deviceId1);
            displayNameCon2.Should().Be("displayName1");

            waitingForAcceptanceFromDeviceCon1.Should().Be(deviceId2);
            waitingForAcceptanceFromDeviceCon2.Should().Be(deviceId1);

            cardExchangeAcceptedPeerDeviceId1.Should().Be(deviceId2);
            cardExchangeAcceptedPeerDisplayName1.Should().Be("displayName2");
            cardExchangeAcceptedPeerCardData1.Should().Be("cardData2");
            acceptanceSentDeviceCon1.Should().Be(deviceId2);
            cardExchangeAcceptedPeerDeviceId2.Should().Be(deviceId1);
            cardExchangeAcceptedPeerDisplayName2.Should().Be("displayName1");
            cardExchangeAcceptedPeerCardData2.Should().Be("cardData1");
            acceptanceSentDeviceCon2.Should().Be(deviceId1);

            cardDataReceivedDeviceId1.Should().Be(deviceId2);
            cardDataReceivedDisplayName1.Should().Be("displayName2");
            cardDataReceivedCardData1.Should().Be("cardData2");
            cardDataSentDeviceCon1.Should().Be(deviceId2);
            cardDataReceivedDeviceId2.Should().Be(deviceId1);
            cardDataReceivedDisplayName2.Should().Be("displayName1");
            cardDataReceivedCardData2.Should().Be("cardData1");
            cardDataSentDeviceCon2.Should().Be(deviceId1);
        }

        [Fact]
        public async void ConnectionTest_Swap2Subscribers_RevokeCardExchangeRequest_SubscribedCalled()
        {
            //Connection
            var connection1 = new HubConnectionBuilder()
                .WithUrl(connectionUrl)
                .Build();
            var connection2 = new HubConnectionBuilder()
                .WithUrl(connectionUrl)
                .Build();

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


            // Events to handle
            //Connection 1
            string deviceIdReqCon1 = string.Empty;
            string displayNameCon1 = string.Empty;
            connection1.On(nameof(ICardExchangeClient.CardExchangeRequested), (string deviceId, string displayName) =>
            {
                deviceIdReqCon1 = deviceId;
                displayNameCon1 = displayName;

                connection1.SendAsync("AcceptCardExchange", deviceId, deviceId1, "displayName1", "cardData1");
            });

            string cardExchangeAcceptedPeerDeviceId1 = string.Empty;
            string cardExchangeAcceptedPeerDisplayName1 = string.Empty;
            string cardExchangeAcceptedPeerCardData1 = string.Empty;
            connection1.On(nameof(ICardExchangeClient.CardExchangeAccepted), (string peerDeviceId, string peerDisplayName, string peerCardData) =>
            {
                cardExchangeAcceptedPeerDeviceId1 = peerDeviceId;
                cardExchangeAcceptedPeerDisplayName1 = peerDisplayName;
                cardExchangeAcceptedPeerCardData1 = peerCardData;
            });
            string cardExchangeRequestRevokedDeviceId1 = string.Empty;
            connection1.On(nameof(ICardExchangeClient.CardExchangeRequestRevoked), (string deviceId) =>
            {
                cardExchangeRequestRevokedDeviceId1 = deviceId;
            });


            //Connection 2
            string deviceIdReqCon2 = string.Empty;
            string displayNameCon2 = string.Empty;
            connection2.On(nameof(ICardExchangeClient.CardExchangeRequested), (string deviceId, string displayName) =>
            {
                deviceIdReqCon2 = deviceId;
                displayNameCon2 = displayName;

                connection2.SendAsync("RevokeCardExchangeRequest", deviceId2, deviceId);
            });
            string revokeSentDeviceCon2 = string.Empty;
            connection2.On(nameof(ICardExchangeClient.RevokeSent), (string deviceId) =>
            {
                revokeSentDeviceCon2 = deviceId;
            });

            // Actions
            await connection1.SendAsync("Update", deviceId1, longitude, latitude1, "displayName1");
            await connection2.SendAsync("Update", deviceId2, longitude, latitudeIn2, "displayName2");

            await connection1.SendAsync("RequestCardExchange", deviceId1, deviceId2, "displayName1");
            await connection2.SendAsync("RequestCardExchange", deviceId2, deviceId1, "displayName2");

            await Task.Delay(2000);

            //Asserts
            deviceIdReqCon1.Should().Be(deviceId2);
            displayNameCon1.Should().Be("displayName2");
            deviceIdReqCon2.Should().Be(deviceId1);
            displayNameCon2.Should().Be("displayName1");

            //should not be called
            cardExchangeAcceptedPeerDeviceId1.Should().Be(String.Empty);
            cardExchangeAcceptedPeerDisplayName1.Should().Be(String.Empty);
            cardExchangeAcceptedPeerCardData1.Should().Be(String.Empty);

            cardExchangeRequestRevokedDeviceId1.Should().Be(deviceId2);
            revokeSentDeviceCon2.Should().Be(deviceId1);
        }
    }
}
