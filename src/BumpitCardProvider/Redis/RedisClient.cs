using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace BumpitCardProvider.Redis
{
    public class RedisClient : IRedisClient
    {
        #region Member fields
        private readonly string _redisHost;
        private readonly int _redisPort;
        private ConnectionMultiplexer _redis;
        #endregion

        #region Constructor

        public RedisClient(IConfiguration config)
        {
            _redisHost = config["Redis:Host"];
            _redisPort = Convert.ToInt32(config["Redis:Port"]);
        }
        #endregion

        #region Implementation of IRedisClient
        public void Connect()
        {
            try
            {
                var configString = $"{_redisHost}:{_redisPort},connectRetry=5";
                _redis = ConnectionMultiplexer.Connect(configString);
            }
            catch (RedisConnectionException err)
            {
                throw err;
            }
        }

        public async Task<bool> GeoAddAsync(string key, double longitude, double latitude, string cardData)
        {
            var db = _redis.GetDatabase();

            RedisKey redisKey = new RedisKey(key);
            RedisValue value = new RedisValue(cardData);

            await db.GeoAddAsync(redisKey, longitude, latitude, value);

            return await db.KeyExpireAsync(redisKey, TimeSpan.FromSeconds(10));
        }

        public Task<GeoRadiusResult[]> GeoRadiusAsync(string key, double longitude, double latitude)
        {
            var db = _redis.GetDatabase();
            return db.GeoRadiusAsync(key, longitude, latitude, 5, GeoUnit.Meters);
        }
        #endregion
    }
}
