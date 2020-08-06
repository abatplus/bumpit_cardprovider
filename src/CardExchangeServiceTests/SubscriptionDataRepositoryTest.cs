using System;
using System.IO;
using System.Threading.Tasks;
using CardExchangeService;
using CardExchangeService.Redis;
using CardExchangeService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CardExchangeServiceTests
{
    public class SubscriptionDataRepositoryTest
    {
        private readonly ISubscriptionDataRepository _repository;

        private const string deviceId1 = "d77b8214 - f7de - 4405 - abda - e87cfa05abac";
        private const string deviceId2 = "d77b8214 - f7de - 4405 - abda - e87cfa05abaa";
        private const string deviceId3 = "d77b8214 - f7de - 4405 - abda - e87cfa05abab";
        private const double latitude1 = 12.466561146;
        private const double latitudeNotIn2 = 12.496562656;
        private const double latitudeIn2 = 12.466561156;
        private const double longitude = -34.405804850;
        private string bse64StringImage1;
        private string bse64StringImage2;

        public SubscriptionDataRepositoryTest()
        {
            Mock<IConfiguration> configurationMock = new Mock<IConfiguration>();
            Mock<IWebHostEnvironment> webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

            configurationMock.Setup(x => x["Redis:Host"]).Returns("localhost");
            configurationMock.Setup(x => x["Redis:Port"]).Returns("6379");
            configurationMock.Setup(x => x["Redis:GeoRadius_m"]).Returns("5");
            configurationMock.Setup(x => x["Redis:KeyExpireTimeout_s"]).Returns("5");

            configurationMock.Setup(x => x["ImageFileSettings:SizeLimitBytes"]).Returns("2097152");
            configurationMock.Setup(x => x["ImageFileSettings:ThumbWidth"]).Returns("100");
            configurationMock.Setup(x => x["ImageFileSettings:ThumbHeight"]).Returns("100");
            configurationMock.Setup(x => x["ImageFileSettings:MaxWidth"]).Returns("300");
            configurationMock.Setup(x => x["ImageFileSettings:MaxHeight"]).Returns("300");
            configurationMock.Setup(x => x["ImageFileSettings:AllowedExtensions"]).Returns(".jpg, .jpeg, .png, .gif");
            configurationMock.Setup(x => x["ImageFileSettings:ThumbFolder"]).Returns("thumbnails");
            configurationMock.Setup(x => x["ImageFileSettings:ImagesFolder"]).Returns("images");

            webHostEnvironmentMock.Setup(x => x.WebRootPath).Returns("wwwroot");
            IRedisClient redisClient = new RedisClient(configurationMock.Object);
            IImageFileService imageFileService = new ImageFileService(configurationMock.Object, webHostEnvironmentMock.Object);
            _repository = new SubscriptionDataRepository(redisClient, imageFileService, configurationMock.Object);

            InitTestImageString();
        }

        private void InitTestImageString()
        {
            Byte[] bytes = File.ReadAllBytes("../../../img/1.jpg");
            bse64StringImage1 = @"data:image/jpg;base64," + Convert.ToBase64String(bytes);
            bytes = File.ReadAllBytes("../../../img/2.jpg");
            bse64StringImage2 = @"data:image/jpg;base64," + Convert.ToBase64String(bytes);
        }

        #region Without images
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
        #endregion

        #region With images

        [Fact]
        public async void Add2SubscriptionsWithImage_InRadius_GetExisting_Ok()
        {

            await _repository.DeleteSubscriber(deviceId1).ContinueWith(x => _repository.DeleteSubscriber(deviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(deviceId3));

            await Task.Delay(2000);


            await _repository.SaveSubscriber(deviceId1, longitude, latitude1, "displayName1", bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(deviceId2, longitude, latitudeIn2, "displayName2", bse64StringImage2));

            var list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(deviceId2);
            data.DisplayName.Should().Be("displayName2");
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async void Add2SubscriptionsWithImage_DeleteOne_GetExisting_Ok()
        {
            await _repository.DeleteSubscriber(deviceId1).ContinueWith(x => _repository.DeleteSubscriber(deviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(deviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(deviceId1, longitude, latitude1, "displayName1", bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(deviceId2, longitude, latitudeIn2, "displayName2", bse64StringImage2));

            var list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(deviceId2);
            data.DisplayName.Should().Be("displayName2");
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();

            await _repository.DeleteSubscriber(deviceId2);

            list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(0);
        }

        [Fact]
        public async void Add2SubscriptionsWithImage_NotInRadius_GetExisting_NotOk()
        {
            await _repository.DeleteSubscriber(deviceId1).ContinueWith(x => _repository.DeleteSubscriber(deviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(deviceId3))
                .ContinueWith(x => _repository.SaveSubscriber(deviceId1, longitude, latitude1, "displayName1", bse64StringImage1))
                .ContinueWith(x => _repository.SaveSubscriber(deviceId2, longitude, latitudeNotIn2, "displayName2", bse64StringImage2));

            var list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(0);
        }

        [Fact]
        public async void Add3SubscriptionsWithImage_NotInRadiusAndInRadius_GetExisting_Ok()
        {
            await _repository.DeleteSubscriber(deviceId1).ContinueWith(x => _repository.DeleteSubscriber(deviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(deviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(deviceId1, longitude, latitude1, "displayName1", bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(deviceId2, longitude, latitudeNotIn2, "displayName2", null))
                .ContinueWith(x => _repository.SaveSubscriber(deviceId3, longitude, latitudeIn2, "displayName3", bse64StringImage2));

            var list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(deviceId3);
            data.DisplayName.Should().Be("displayName3");
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();

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
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async void AddSubscriptionWithImage_GetSubscriberImage_Ok()
        {
            await _repository.DeleteSubscriber(deviceId1).ContinueWith(x => _repository.DeleteSubscriber(deviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(deviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(deviceId1, longitude, latitude1, "displayName1", bse64StringImage1);

            await Task.Delay(2000);

            var resImage = await _repository.GetSubscriberImage(deviceId1);

            resImage.Should().NotBeNullOrWhiteSpace();
            bse64StringImage1.Should().NotBeNullOrWhiteSpace();

            var imageBytes1 = GetImageBytes(resImage);
            var imageBytes2 = GetImageBytes(bse64StringImage1);

            imageBytes1.Should().NotBeNull();
            imageBytes2.Should().NotBeNull();

           // File.WriteAllBytes("../../../img/3.jpg", imageBytes1);

            imageBytes1.Should().BeEquivalentTo(imageBytes2);
        }

        private byte[] GetImageBytes(string bse64StringImage)
        {
            var imageInfo = bse64StringImage.Split(',');

            if (imageInfo.Length < 2)
                return null;

            return Convert.FromBase64String(imageInfo[1]);
        }

        [Fact]
        public async void Add2SubscriptionsWithImage_GetThumbnailUrl_Ok()
        {
            await _repository.DeleteSubscriber(deviceId1).ContinueWith(x => _repository.DeleteSubscriber(deviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(deviceId3));

            await Task.Delay(2000);


            await _repository.SaveSubscriber(deviceId1, longitude, latitude1, "displayName1", bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(deviceId2, longitude, latitudeIn2, "displayName2", bse64StringImage2));

            var list = await _repository.GetNearestSubscribers(deviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(deviceId2);
            data.DisplayName.Should().Be("displayName2");
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();

            var resThumbnailUrl = await _repository.GetThumbnailUrl(deviceId2);

            data.ThumbnailUrl.Should().Be(resThumbnailUrl);

        }
        #endregion
    }
}