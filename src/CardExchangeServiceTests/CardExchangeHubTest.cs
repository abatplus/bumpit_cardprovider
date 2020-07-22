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

namespace CardExchangeServiceTests
{
    public class CardExchangeHubTest
    {
        //private readonly Mock<IElasticClient> elasticClientMock;
        private readonly Mock<IConfiguration> configurationMock;
        private ICardExchangeHub cardExchangeHub;
        private readonly Mock<ISubscriptionDataRepository> repository;

        private Mock<IHubCallerClients<ICardExchangeClient>> mockClients;
        private Mock<HubCallerContext> mockContext;
        private Mock<IGroupManager> mockGroups;


        public CardExchangeHubTest()
        {
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(x => x["Redis:Host"]).Returns("localhost");
            configurationMock.Setup(x => x["Redis:Port"]).Returns("6379");
            configurationMock.Setup(x => x["Redis:GeoRadius_m"]).Returns("5");
            configurationMock.Setup(x => x["Redis:KeyExpireTimeout_s"]).Returns("2");
            configurationMock.Setup(x => x["ConnectionInfo:AllowedCoreOrigins0"]).Returns("http://localhost:3000");
            
            repository = new Mock<ISubscriptionDataRepository>();
            mockClients = new Mock<IHubCallerClients<ICardExchangeClient>>();
            mockGroups = new Mock<IGroupManager>();
            mockContext = new Mock<HubCallerContext>();

            cardExchangeHub = new CardExchangeHub(repository.Object);
            ((Hub<ICardExchangeClient>)cardExchangeHub).Clients = mockClients.Object;
            ((Hub<ICardExchangeClient>) cardExchangeHub).Groups = mockGroups.Object;
            ((Hub<ICardExchangeClient>)cardExchangeHub).Context = mockContext.Object;
        }

        //[Fact]
        //public async void CardExchangeHubTest_Subscribe()
        //{
        //    var connection = new HubConnectionBuilder()
        //        .WithUrl("http://localhost:5000/swaphub")
        //        .Build();

        //    repository.Setup(x => x.GetNearestSubscribers("device1")).Returns(new List<string>()
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
