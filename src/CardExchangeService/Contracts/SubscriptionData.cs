using Newtonsoft.Json;

namespace CardExchangeService
{
    public class SubscriptionData : DeviceData
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }
    }
}