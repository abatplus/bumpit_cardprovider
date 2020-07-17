using Newtonsoft.Json;

namespace BumpitCardExchangeService
{
  public class DeviceData
  {
    [JsonProperty("device_id")]
    [JsonRequired]
    public string DeviceId { get; set; }

    public string DisplayName { get; set; }
  }
}