using BumpitCardProvider.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IEnumerable<string> GetBumpitCard()
        {
            //TODO
            return new List<string>();
        }

        /// <summary>
        /// Creates a document in the elasticsearch database in the specified index.
        /// </summary>
        /// <remarks>
        ///  
        ///     POST /api/redis/device/timestamp
        ///     {
        ///        "kubernetes":
        ///        {
        ///           "container":
        ///           {
        ///             "name":"con-name-1"
        ///           },
        ///        "message":"Error message",
        ///        "log":
        ///        {
        ///           "level":"error"
        ///        }
        ///     }
        /// 
        /// </remarks>
        /// <param name="index">The index to insert a document</param>
        /// <param name="message">The message data to send in request`s body</param>
        /// <returns>Result of the action</returns>
        /// <response code="200">A document was created</response>
        /// <response code="404">Error at inserting data</response>
        /// <response code="400">If the item is null</response>
        [HttpPost("{device/timestamp}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Exception), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult SaveBumpitCard(string deviceId, string timestamp, [FromBody] string message)
        {
            if (message == null || string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(timestamp))
            {
                return BadRequest();
            }

            if (!redisClient.SetStringAsync(deviceId, message).Result)
                return NotFound();

            return Ok();
        }

        /// <summary>
        /// Gets data to specified index in the elasticsearch database.
        /// </summary>
        /// <remarks>
        ///  
        ///     GET /api/redis/device/timestamp
        /// 
        /// </remarks>
        /// <param name="index">The index to find</param>
        /// <returns>Index data</returns>
        /// <response code="200">En index was found</response>
        [HttpGet("{device/timestamp}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public IEnumerable<string> GetBumpitCard(string deviceId, string timestamp)
        {
            if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(timestamp))
            {
                return null;
            }

            var res = redisClient.GetStringAsync(deviceId).Result;
            //TODO

            return new List<string>();
        }

        #endregion

        #region Helper Methods


        #endregion
    }
}