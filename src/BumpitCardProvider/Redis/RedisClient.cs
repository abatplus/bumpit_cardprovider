using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        public Task<bool> SetStringAsync(string key, string value)
        {
            var db = _redis.GetDatabase();
            return db.StringSetAsync(key, value);
        }

        public Task<RedisValue> GetStringAsync(string key)
        {
            var db = _redis.GetDatabase();
            return db.StringGetAsync(key);
        }
    }
}
