using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace BumpitCardExchangeService.Redis
{
    public class RedisClient : IRedisClient
    {
        private readonly string _redisHost;
        private readonly int _redisPort;
        private ConnectionMultiplexer _redis;

        public ConnectionMultiplexer Redis
        {
            get
            {
                if (_redis == null)
                    Connect();
                return _redis;
            }
        }

        public RedisClient(IConfiguration config)
        {
            _redisHost = config["REDIS_HOST"] ?? config["Redis:Host"];
            _redisPort = Convert.ToInt32(config["REDIS_PORT"] ?? config["Redis:Port"]);
        }

        private void Connect()
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

        #region Implementation of IRedisClient

        public Task<bool> SetString(string key, string value)
        {
            var db = Redis.GetDatabase();
            return db.StringSetAsync(key, value);
        }

        public Task<RedisValue> GetString(string key)
        {
            var db = Redis.GetDatabase();
            return db.StringGetAsync(key);
        }

        public async Task<bool> GeoAdd(string key, double longitude, double latitude, string cardData)
        {
            var db = Redis.GetDatabase();

            RedisKey redisKey = new RedisKey(key);
            RedisValue value = new RedisValue(cardData);

            return await db.GeoAddAsync(redisKey, longitude, latitude, value);
        }

        public Task<GeoRadiusResult[]> GeoRadius(string key, double longitude, double latitude)
        {
            var db = Redis.GetDatabase();
            return db.GeoRadiusAsync(key, longitude, latitude, 5, GeoUnit.Meters);
        }

        public Task<GeoRadiusResult[]> GeoRadiusByMember(string key, string member)
        {
            var db = Redis.GetDatabase();
            return db.GeoRadiusAsync(key, member, 5, GeoUnit.Meters);
        }

        public bool GeoRemove(string key, string device)
        {
            var db = Redis.GetDatabase();
            return db.GeoRemove(key, device);
        }

        public async Task<bool> RemoveKey(string device)
        {
            var db = Redis.GetDatabase();
            return await db.KeyDeleteAsync(device);
        }
        #endregion
    }
}
