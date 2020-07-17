using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CardExchangeService.Redis
{
  public interface ISubscriptionDataRepository
  {
    void SaveSubscriber(string deviceId, double longitude, double latitude, string displayName);
    void DeleteSubscriber(string deviceId);
    IList<string> GetNearestSubscribers(string deviceId);
  }
}