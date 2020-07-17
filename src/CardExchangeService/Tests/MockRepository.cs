using System;
using System.Collections.Generic;
using System.Linq;
using BumpitCardExchangeService.Redis;
using Newtonsoft.Json;

namespace CardExchangeService.Tests
{
  public class MockRepositoryTest
  {
    public MockRepositoryTest()
    {
      var repo = new MockRepository();
      repo.SaveSubscriber("123", 0, 0, "Optimus");
      repo.SaveSubscriber("1001", 0, 0, "Bumblebee");
      PrintList(repo.GetNearestSubscribers("1001"));
      repo.SaveSubscriber("9000", 0, 0, "Wheeljack");
      PrintList(repo.GetNearestSubscribers("1001"));
      repo.DeleteSubscriber("1001");
      PrintList(repo.GetNearestSubscribers("123"));
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
    private List<SubscriptionData> subscribers = new List<SubscriptionData>();

    public IList<string> GetNearestSubscribers(string deviceId)
    {
      return subscribers.Where(s => s.DeviceId != deviceId)
          .Select(s => JsonConvert.SerializeObject(new DeviceData { DeviceId = s.DeviceId, DisplayName = s.DisplayName }))
          .ToList();
    }

    public void SaveSubscriber(string deviceId, double longitude, double latitude, string displayName)
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
    }

    public void DeleteSubscriber(string deviceId)
    {
      var subscriber = subscribers.FirstOrDefault(s => s.DeviceId == deviceId);
      if (subscriber != null)
        subscribers.Remove(subscriber);
    }
  }
}