using Newtonsoft.Json;

namespace BumpitCardExchangeService
{
    public class SubscriptionData
    {
        [JsonProperty("device_id")]
        [JsonRequired]
        public string DeviceId
        {
            get; set;
        }

        //FirstName, LastName ...
        public string AdditionalData { get; set; }
    }
}