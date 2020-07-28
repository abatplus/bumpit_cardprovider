using System.Threading.Tasks;
using CardExchangeService;
using CardExchangeService.Redis;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CardExchangeServiceTests
{
    public class SubscriptionDataRepositoryTest
    {
        private readonly Mock<IConfiguration> configurationMock;
        private readonly IRedisClient _redisClient;
        private readonly ISubscriptionDataRepository _repository;

        private const string deviceId1 = "d77b8214 - f7de - 4405 - abda - e87cfa05abac";
        private const string deviceId2 = "d77b8214 - f7de - 4405 - abda - e87cfa05abaa";
        private const string deviceId3 = "d77b8214 - f7de - 4405 - abda - e87cfa05abab";
        private const double latitude1 = 12.466561146;
        private const double latitudeNotIn2 = 12.496562656;
        private const double latitudeIn2 = 12.466561156;
        private const double longitude = -34.405804850;

        public SubscriptionDataRepositoryTest()
        {
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(x => x["Redis:Host"]).Returns("localhost");
            configurationMock.Setup(x => x["Redis:Port"]).Returns("6379");
            configurationMock.Setup(x => x["Redis:GeoRadius_m"]).Returns("5");
            configurationMock.Setup(x => x["Redis:KeyExpireTimeout_s"]).Returns("2");

            _redisClient = new RedisClient(configurationMock.Object);
            _repository = new SubscriptionDataRepository(_redisClient);
        }

        [Fact]
        public async void Add2Subscriptions_InRadius_GetExisting_Ok()
        {
            await _repository.DeleteSubscriber(deviceId1).ContinueWith(x => _repository.DeleteSubscriber(deviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(deviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(deviceId1, longitude, latitude1, "displayName1", null)
                .ContinueWith(x => _repository.SaveSubscriber(deviceId2, longitude, latitudeIn2, "displayName2", null));

            var list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(deviceId2);
            data.DisplayName.Should().Be("displayName2");
        }

        [Fact]
        public async void Add2Subscriptions_DeleteOne_GetExisting_Ok()
        {
            await _repository.DeleteSubscriber(deviceId1).ContinueWith(x => _repository.DeleteSubscriber(deviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(deviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(deviceId1, longitude, latitude1, "displayName1", null)
                .ContinueWith(x => _repository.SaveSubscriber(deviceId2, longitude, latitudeIn2, "displayName2", null));

            var list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(deviceId2);
            data.DisplayName.Should().Be("displayName2");

            await _repository.DeleteSubscriber(deviceId2);

            list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(0);
        }

        [Fact]
        public async void Add2Subscriptions_NotInRadius_GetExisting_NotOk()
        {
            await _repository.DeleteSubscriber(deviceId1).ContinueWith(x => _repository.DeleteSubscriber(deviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(deviceId3))
                .ContinueWith(x => _repository.SaveSubscriber(deviceId1, longitude, latitude1, "displayName1", null))
                .ContinueWith(x => _repository.SaveSubscriber(deviceId2, longitude, latitudeNotIn2, "displayName2", null));

            var list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(0);
        }

        [Fact]
        public async void Add3Subscriptions_NotInRadiusAndInRadius_GetExisting_Ok()
        {
            await _repository.DeleteSubscriber(deviceId1).ContinueWith(x => _repository.DeleteSubscriber(deviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(deviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(deviceId1, longitude, latitude1, "displayName1", null)
                .ContinueWith(x => _repository.SaveSubscriber(deviceId2, longitude, latitudeNotIn2, "displayName2", null))
                .ContinueWith(x => _repository.SaveSubscriber(deviceId3, longitude, latitudeIn2, "displayName3", null));

            var list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(deviceId3);
            data.DisplayName.Should().Be("displayName3");

            await _repository.DeleteSubscriber(deviceId2);

            list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(deviceId3);
            data.DisplayName.Should().Be("displayName3");
        }
    }
}