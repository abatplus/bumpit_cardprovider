using BumpitCardProvider.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
#region Usings
using System.Collections.Generic;
#endregion

namespace BumpitCardProvider.Controllers
{
    /// <summary>
    /// Class handling the incomming request GET, POST
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/redis")]
    public class BumpitCardController : ControllerBase
    {
        #region Member fields

        private readonly IRedisClient redisClient;
        #endregion

        #region Constructor
        public BumpitCardController(IRedisClient redisClient)
        {
            this.redisClient = redisClient;
        }
        #endregion

        #region Web API

        /// <summary>
        /// Creates an entry in the redis database to the specified geolocation.
        /// </summary>
        /// <remarks>
        ///  
        ///     POST /api/redis/
        ///     {
        ///       "device_id": "d77b8214-f7de-4405-abda-e87cfa05abac",  
        ///       "latitude": "12.466562656",
        ///       "longitude": "-34.405804850",
        ///       "card_data": 
        ///       {
        ///           "name": "Max", 
        ///           "tel":"344363563", 
        ///           "country": "Germany"
        ///       } 
        ///     }
        /// 
        /// </remarks>
        /// <param name="cardData">The card data to send in request`s body</param>
        /// <returns>Result of the action</returns>
        /// <response code="201">An entry was created</response>
        /// <response code="400">If the item is null</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Exception), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult SaveBumpitCard([FromBody] BumpitCardData cardData)
        {
            if (cardData == null || string.IsNullOrWhiteSpace(cardData.DeviceId))
            {
                return BadRequest();
            }

            cardData.Timestamp = DateTime.Now;
            string entryKey = GetGeoEntryKey(cardData.Longitude, cardData.Latitude);

            if (!redisClient.GeoAddAsync(entryKey, cardData.Longitude, cardData.Latitude, JsonConvert.SerializeObject(cardData)).Result)
                return BadRequest();

            return Created(string.Empty, cardData);
        }

        /// <summary>
        /// Gets card data of devices located in the radius of 5m from specified device.
        /// </summary>
        /// <remarks>
        ///  
        ///     GET /api/redis/{device}?longitude=x&latitude=y
        /// 
        /// </remarks>
        /// <param name="device">The device to find</param>
        /// /// <param name="longitude">The longitude of the geolocation</param>
        /// /// <param name="latitude">The of the geolocation</param>
        /// <returns>Card data</returns>
        /// <response code="200">Data was found</response>
        [HttpGet("{device}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IEnumerable<BumpitCardData> GetBumpitCard(string device, [FromQuery] double longitude, [FromQuery] double latitude)
        {
            List<BumpitCardData> resList = new List<BumpitCardData>();

            if (string.IsNullOrWhiteSpace(device))
            {
                return resList;
            }

            TimeSpan interval = new TimeSpan(0, 0, 10);
            string entryKey = GetGeoEntryKey(longitude, latitude);

            var res = redisClient.GeoRadiusAsync(entryKey, longitude, latitude).Result;
            if (res != null)
            {
                foreach (var el in res)
                {
                    BumpitCardData cardData = JsonConvert.DeserializeObject<BumpitCardData>(el.Member);
                    if (DateTime.Now - cardData.Timestamp <= interval)
                    {
                        if (cardData.DeviceId != device)
                        {
                            resList.Add(cardData);
                        }
                    }
                }
            }

            return resList;
        }

        #endregion

        #region Helper Methods

        private string GetGeoEntryKey(double longitude, double latitude)
        {
            return nameof(BumpitCardData) + ":" + Math.Round(latitude, 5) + ":" + Math.Round(longitude, 5);
        }

        #endregion
    }


    #region Mapping classes

    public class BumpitCardData
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

        [JsonProperty("card_data")]
        public JObject CardData
        {
            get;
            set;
        }

        [JsonProperty("timestamp")]
        public DateTime Timestamp
        {
            get;
            set;
        }
    }
    #endregion
}