using Newtonsoft.Json;

namespace CardExchangeService
{
    public class SubscriptionData : DeviceData
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        [JsonProperty("longitute")]
        public double Longitute { get; set; }
        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }
    }
}