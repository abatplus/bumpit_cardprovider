using Newtonsoft.Json;

namespace CardExchangeService
{
  public class DeviceData
  {
    [JsonProperty("deviceId")]
    [JsonRequired]
    public string DeviceId { get; set; }

    [JsonProperty("displayName")]
    [JsonRequired]
    public string DisplayName { get; set; }
  }
}