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

        private const string ConnectionId1 = "81sQRSCbfEidBxsIO9bnyQ";
        private const string ConnectionId2 = "81sQRSCbfEidBxsIO9bny1";
        private const string ConnectionId3 = "81sQRSCbfEidBxsIO9bny2";


        //private readonly Mock<IElasticClient> elasticClientMock;
        private readonly Mock<IConfiguration> configurationMock;
        private ICardExchangeHub cardExchangeHub;
        private readonly Mock<ISubscriptionDataRepository> repository;

        private Mock<IHubCallerClients<ICardExchangeClient>> mockClients;
        private Mock<HubCallerContext> mockContext;
        private Mock<IGroupManager> mockGroups;

        private readonly IRedisClient _redisClient;
        private readonly ISubscriptionDataRepository _repository;

        public CardExchangeHubTest()
        {
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(x => x["Redis:Host"]).Returns("localhost");
            configurationMock.Setup(x => x["Redis:Port"]).Returns("6379");
            configurationMock.Setup(x => x["Redis:GeoRadius_m"]).Returns("5");
            configurationMock.Setup(x => x["Redis:KeyExpireTimeout_s"]).Returns("2");

            repository = new Mock<ISubscriptionDataRepository>();
            mockClients = new Mock<IHubCallerClients<ICardExchangeClient>>();
            mockGroups = new Mock<IGroupManager>();
            mockContext = new Mock<HubCallerContext>();


            _redisClient = new RedisClient(configurationMock.Object);
            _repository = new SubscriptionDataRepository(_redisClient);

            //            cardExchangeHub = new CardExchangeHub(repository.Object);
            cardExchangeHub = new CardExchangeHub(_repository);
            ((Hub<ICardExchangeClient>)cardExchangeHub).Clients = mockClients.Object;
            ((Hub<ICardExchangeClient>)cardExchangeHub).Groups = mockGroups.Object;
            ((Hub<ICardExchangeClient>)cardExchangeHub).Context = mockContext.Object;
        }

        [Fact]
        public async void CardExchangeHubTest_Subscribe_SunscribedCalled()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/swaphub")
                .Build();

            mockContext.Setup(x => x.ConnectionId).Returns(ConnectionId1);


            var caller = new MockCardExchangeClient();
            mockClients.Setup(x => x.Caller).Returns(caller);

            await connection.StartAsync().ContinueWith(x =>
           {
               if (connection.State == HubConnectionState.Connected)
               {
                   cardExchangeHub.Subscribe(deviceId1, longitude, latitude1, "displayName1");
               }
           });

            await Task.Delay(2000);

            caller.Peers.Should().NotBeNull();
        }


        [Fact]
        public async void CardExchangeHubTest_TwoSubscribers_SunscribedCalled()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/swaphub")
                .Build();

            mockContext.Setup(x => x.ConnectionId).Returns(ConnectionId1);

            var caller = new MockCardExchangeClient();
            mockClients.Setup(x => x.Caller).Returns(caller);

            await connection.StartAsync().ContinueWith(x =>
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    cardExchangeHub.Subscribe(deviceId1, longitude, latitude1, "displayName1");
                }
            });

            await Task.Delay(2000);

            caller.Peers.Should().NotBeNull();
        }

        //[Fact]
        //public async void CardExchangeHubTest_Subscribe()
        //{
        //    var connection = new HubConnectionBuilder()
        //        .WithUrl("http://localhost:5000/swaphub")
        //        .Build();

        //    repository.Setup(x => x.GetNearestSubscribers("device1")).ReturnsAsync(new List<string>()
        //    {
        //        JsonConvert.SerializeObject(
        //            new SubscriptionData() {DeviceId = "Device2", DisplayName = "DisplayName2"}),
        //            JsonConvert.SerializeObject(
        //                new SubscriptionData() {DeviceId = "Device3", DisplayName = "DisplayName3"})
        //    });
        //    // "81sQRSCbfEidBxsIO9bnyQ";
        //    mockContext.Setup(x => x.ConnectionId).Returns("81sQRSCbfEidBxsIO9bnyQ");

        //    //TODO: mock caller
        //    dynamic caller = new ExpandoObject();
        //    mockClients.Setup(x => x.Caller).Returns(caller);

        //    bool isSubscribedCalled = false;
        //    IEnumerable<string> resPeers = new List<string>();
        //    connection.On(nameof(ICardExchangeClient.Subscribed), (IEnumerable<string> peers) =>
        //    {
        //        isSubscribedCalled = true;
        //        resPeers = peers;
        //    });

        //    await connection.StartAsync();
        //    await cardExchangeHub.Subscribe("device1", 1, 2, "displayName1");

        //    isSubscribedCalled.Should().BeTrue();


        //}
    }
}
