using Newtonsoft.Json;

namespace BumpitCardSwapService
{
    public class SubscriptionData
    {
        [JsonProperty("device_id")]
        [JsonRequired]
        public string DeviceId
        {
            get; set;
        }

        [JsonProperty("longitude")]
        [JsonRequired]
        public double Longitude
        {
            get;
            set;
        }

        [JsonProperty("latitude")]
        [JsonRequired]
        public double Latitude
        {
            get;
            set;
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}