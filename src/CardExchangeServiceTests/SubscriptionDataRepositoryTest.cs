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

        private const string DeviceId1 = "d77b8214 - f7de - 4405 - abda - e87cfa05abac";
        private const string DeviceId2 = "d77b8214 - f7de - 4405 - abda - e87cfa05abaa";
        private const string DeviceId3 = "d77b8214 - f7de - 4405 - abda - e87cfa05abab";
        private const double Latitude1 = 12.466561146;
        private const double LatitudeNotIn2 = 12.496562656;
        private const double LatitudeIn2 = 12.466561156;
        private const double Longitude = -34.405804850;
        private string _bse64StringImage1;
        private string _bse64StringImage2;

        public SubscriptionDataRepositoryTest()
        {
            Mock<IConfiguration> configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(x => x["Redis:Host"]).Returns("localhost");
            configurationMock.Setup(x => x["Redis:Port"]).Returns("6379");
            configurationMock.Setup(x => x["Redis:GeoRadius_m"]).Returns("25");
            configurationMock.Setup(x => x["Redis:KeyExpireTimeout_s"]).Returns("5");

            configurationMock.Setup(x => x["ImageFileSettings:SizeLimitBytes"]).Returns("2097152");
            configurationMock.Setup(x => x["ImageFileSettings:ThumbWidth"]).Returns("100");
            configurationMock.Setup(x => x["ImageFileSettings:ThumbHeight"]).Returns("100");
            configurationMock.Setup(x => x["ImageFileSettings:MaxWidth"]).Returns("300");
            configurationMock.Setup(x => x["ImageFileSettings:MaxHeight"]).Returns("300");
            configurationMock.Setup(x => x["ImageFileSettings:AllowedExtensions"]).Returns(".jpg, .jpeg");
            configurationMock.Setup(x => x["ImageFileSettings:ThumbFolder"]).Returns("thumbnails");
            configurationMock.Setup(x => x["ImageFileSettings:ImagesFolder"]).Returns("images");
            configurationMock.Setup(x => x["ImageFileSettings:ThumbUrlPathPrefix"]).Returns("/thumbnails");
            configurationMock.Setup(x => x["ImageFileSettings:ImgUrlPathPrefix"]).Returns("/images");

            IRedisClient redisClient = new RedisClient(configurationMock.Object);
            IImageFileService imageFileService = new ImageFileService(configurationMock.Object);
            _repository = new SubscriptionDataRepository(redisClient, imageFileService, configurationMock.Object);

            InitTestImageString();
        }

        private void InitTestImageString()
        {
            Byte[] bytes = File.ReadAllBytes("../../../img/1.jpg");
            _bse64StringImage1 = @"data:image/jpg;base64," + Convert.ToBase64String(bytes);
            bytes = File.ReadAllBytes("../../../img/2.jpg");
            _bse64StringImage2 = @"data:image/jpg;base64," + Convert.ToBase64String(bytes);
        }

        #region Without images
        [Fact]
        public async void Add2Subscriptions_InRadius_GetExisting_Ok()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", null)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", null));

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(DeviceId2);
            data.DisplayName.Should().Be("displayName2");
            data.ThumbnailUrl.Should().BeNullOrEmpty();
        }

        [Fact]
        public async void Add2Subscriptions_DeleteOne_GetExisting_Ok()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", null)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", null));

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(DeviceId2);
            data.DisplayName.Should().Be("displayName2");
            data.ThumbnailUrl.Should().BeNullOrEmpty();

            await _repository.DeleteSubscriber(DeviceId2);

            list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(0);
        }

        [Fact]
        public async void Add2Subscriptions_NotInRadius_GetExisting_NotOk()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3))
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", null))
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeNotIn2, "displayName2", null));

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(0);
        }

        [Fact]
        public async void Add3Subscriptions_NotInRadiusAndInRadius_GetExisting_Ok()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", null)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeNotIn2, "displayName2", null))
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId3, Longitude, LatitudeIn2, "displayName3", null));

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(DeviceId3);
            data.DisplayName.Should().Be("displayName3");
            data.ThumbnailUrl.Should().BeNullOrEmpty();

            await _repository.DeleteSubscriber(DeviceId2);

            list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(DeviceId3);
            data.DisplayName.Should().Be("displayName3");
            data.ThumbnailUrl.Should().BeNullOrEmpty();
        }
        #endregion

        #region With images

        [Fact]
        public async void Add2SubscriptionsWithImage_InRadius_GetExisting_Ok()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", _bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", _bse64StringImage2));

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(DeviceId2);
            data.DisplayName.Should().Be("displayName2");
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();

            await Task.Delay(5000);

            list = await _repository.GetNearestSubscribers(DeviceId1);
            list.Should().NotBeNull();
            list.Count.Should().Be(0);
        }

        [Fact]
        public async void Add2SubscriptionsWithImage_DeleteOne_GetExisting_Ok()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", _bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", _bse64StringImage2));

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(DeviceId2);
            data.DisplayName.Should().Be("displayName2");
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();

            await _repository.DeleteSubscriber(DeviceId2);

            list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(0);
        }

        [Fact]
        public async void Add2SubscriptionsWithImage_2Update_ImageWasNotDeleted()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", _bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", _bse64StringImage2));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", null)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", null));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", null)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", null));

            await Task.Delay(2000);

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(DeviceId2);
            data.DisplayName.Should().Be("displayName2");
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();

            var resImage = await _repository.GetSubscriberImage(DeviceId1);

            resImage.Should().NotBeNullOrWhiteSpace();
            _bse64StringImage1.Should().NotBeNullOrWhiteSpace();

            var imageBytes1 = GetImageBytes(resImage);
            var imageBytes2 = GetImageBytes(_bse64StringImage1);

            imageBytes1.Should().NotBeNull();
            imageBytes2.Should().NotBeNull();

            // File.WriteAllBytes("../../../img/3.jpg", imageBytes1);

            imageBytes1.Should().BeEquivalentTo(imageBytes2);
        }
        
        [Fact]
        public async void Add2SubscriptionsWithImage_2Update_ImageWasDeleted()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", _bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", _bse64StringImage2));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", null)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", null));

            await Task.Delay(5000);

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().BeNullOrEmpty();

            var resImage = await _repository.GetSubscriberImage(DeviceId1);

            resImage.Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public async void Add2SubscriptionsWithImage_NotInRadius_GetExisting_NotOk()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3))
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", _bse64StringImage1))
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeNotIn2, "displayName2", _bse64StringImage2));

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(0);
        }

        [Fact]
        public async void Add3SubscriptionsWithImage_NotInRadiusAndInRadius_GetExisting_Ok()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", _bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeNotIn2, "displayName2", null))
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId3, Longitude, LatitudeIn2, "displayName3", _bse64StringImage2));

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(DeviceId3);
            data.DisplayName.Should().Be("displayName3");
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();

            await _repository.DeleteSubscriber(DeviceId2);

            list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(DeviceId3);
            data.DisplayName.Should().Be("displayName3");
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async void AddSubscriptionWithImage_GetSubscriberImage_Ok()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);

            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", _bse64StringImage1);

            var resImage = await _repository.GetSubscriberImage(DeviceId1);

            resImage.Should().NotBeNullOrWhiteSpace();
            _bse64StringImage1.Should().NotBeNullOrWhiteSpace();

            var imageBytes1 = GetImageBytes(resImage);
            var imageBytes2 = GetImageBytes(_bse64StringImage1);

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
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);


            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", _bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", _bse64StringImage2));

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(DeviceId2);
            data.DisplayName.Should().Be("displayName2");
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();

            var resThumbnailUrl = await _repository.GetThumbnailUrl(DeviceId2);

            data.ThumbnailUrl.Should().Be(resThumbnailUrl);

        }

        [Fact]
        public async void Add2SubscriptionsWithImage_GetImageUrl_Ok()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);


            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", _bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", _bse64StringImage2));

            var list = await _repository.GetNearestSubscribers(DeviceId1);

            list.Should().NotBeNull();
            list.Count.Should().Be(1);

            var data = JsonConvert.DeserializeObject<SubscriptionData>(list[0]);
            data.Should().NotBeNull();
            data.Latitude.Should().Be(0);
            data.Longitute.Should().Be(0);
            data.DeviceId.Should().Be(DeviceId2);
            data.DisplayName.Should().Be("displayName2");
            data.ThumbnailUrl.Should().NotBeNullOrEmpty();

            var resImageUrl = await _repository.GetImageUrl(DeviceId2);

            resImageUrl.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async void Add2SubscriptionsWithImage_Subscribe_Update_Ok()
        {
            await _repository.DeleteSubscriber(DeviceId1).ContinueWith(x => _repository.DeleteSubscriber(DeviceId2))
                .ContinueWith(x => _repository.DeleteSubscriber(DeviceId3));

            await Task.Delay(2000);

            //Subscribe
            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", _bse64StringImage1)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", _bse64StringImage2));

            await Task.Delay(2000);

            //Update
            await _repository.SaveSubscriber(DeviceId1, Longitude, Latitude1, "displayName1", null)
                .ContinueWith(x => _repository.SaveSubscriber(DeviceId2, Longitude, LatitudeIn2, "displayName2", null));

            await Task.Delay(2000);

            var list1 = await _repository.GetNearestSubscribers(DeviceId1);
            var list2 = await _repository.GetNearestSubscribers(DeviceId2);

            list1.Should().NotBeNull();
            list1.Count.Should().Be(1);
            list2.Should().NotBeNull();
            list2.Count.Should().Be(1);

            var data1 = JsonConvert.DeserializeObject<SubscriptionData>(list1[0]);
            data1.Should().NotBeNull();
            data1.Latitude.Should().Be(0);
            data1.Longitute.Should().Be(0);
            data1.DeviceId.Should().Be(DeviceId2);
            data1.DisplayName.Should().Be("displayName2");
            data1.ThumbnailUrl.Should().NotBeNullOrEmpty();

            var data2 = JsonConvert.DeserializeObject<SubscriptionData>(list2[0]);
            data2.Should().NotBeNull();
            data2.Latitude.Should().Be(0);
            data2.Longitute.Should().Be(0);
            data2.DeviceId.Should().Be(DeviceId1);
            data2.DisplayName.Should().Be("displayName1");
            data2.ThumbnailUrl.Should().NotBeNullOrEmpty();
        }
        #endregion
    }
}