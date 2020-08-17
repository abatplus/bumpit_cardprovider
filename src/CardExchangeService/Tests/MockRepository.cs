using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardExchangeService.Redis;
using Newtonsoft.Json;

namespace CardExchangeService.Tests
{
    public class MockRepositoryTest
    {
        public MockRepositoryTest()
        {
            var repo = new MockRepository();
            repo.SaveSubscriber("123", 0, 0, "Optimus", null);
            repo.SaveSubscriber("1001", 0, 0, "Bumblebee", null);
            PrintList(repo.GetNearestSubscribers("1001").Result);
            repo.SaveSubscriber("9000", 0, 0, "Wheeljack", null);
            PrintList(repo.GetNearestSubscribers("1001").Result);
            repo.DeleteSubscriber("1001");
            PrintList(repo.GetNearestSubscribers("123").Result);
        }

        private void PrintList(IList<string> list)
        {
            Console.WriteLine("List | {0} Items", list.Count());
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }
    }
    public class MockRepository : ISubscriptionDataRepository
    {
        private static List<SubscriptionData> subscribers = new List<SubscriptionData>();

        private void PrintList(IList<string> list)
        {
            Console.WriteLine("List | {0} Items", list.Count());
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        public async Task<IList<string>> GetNearestSubscribers(string deviceId)
        {
            return subscribers.Where(s => s.DeviceId != deviceId)
                .Select(s => JsonConvert.SerializeObject(new DeviceData { DeviceId = s.DeviceId, DisplayName = s.DisplayName }))
                .ToList();
        }

        public async Task<bool> SaveSubscriber(string deviceId, double longitude, double latitude, string displayName, string image)
        {
            var subscriber = subscribers.FirstOrDefault(s => s.DeviceId == deviceId);
            if (subscriber == null)
            {
                subscribers.Add(new SubscriptionData { DeviceId = deviceId, Longitute = longitude, Latitude = latitude, DisplayName = displayName });
            }
            else
            {
                subscriber.Longitute = longitude;
                subscriber.Latitude = latitude;
                subscriber.DisplayName = displayName;
            }

            return true;
        }

        public async Task<bool> DeleteSubscriber(string deviceId)
        {
            var subscriber = subscribers.FirstOrDefault(s => s.DeviceId == deviceId);
            if (subscriber != null)
                subscribers.Remove(subscriber);

            return true;
        }

        public async Task<string> GetThumbnailUrl(string deviceId)
        {
            return string.Empty;
        }

        public async Task<string> GetImageUrl(string deviceId)
        {
            return string.Empty;
        }

        public async Task<string> GetSubscriberImage(string deviceId)
        {
            return string.Empty;
        }
    }
}