using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CardExchangeService.Redis
{
    public class RedisClient : IRedisClient
    {
        private const string chanelPrefix = "__keyspace@0__:";
        private readonly string _redisHost;
        private readonly int _redisPort;
        private readonly int _redisKeyExpireTimeout;
        private readonly int _redisGeoRadius;
        private ConnectionMultiplexer _redis;

        private string GetGeoEntryKey()
        {
            return "GeoEntryKey";
        }

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
            _redisGeoRadius = Convert.ToInt32(config["REDIS_GEORADIUS"] ?? config["Redis:GeoRadius_m"]);
            _redisKeyExpireTimeout = Convert.ToInt32(config["REDIS_KEY_EXPIRE_TIMEOUT"] ?? config["Redis:KeyExpireTimeout_s"]);
        }

        private void Connect()
        {
            try
            {
                var configString = $"{_redisHost}:{_redisPort},allowAdmin=true";
                _redis = ConnectionMultiplexer.Connect(configString);
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                /*
                 *K     Keyspace events, published with __keyspace@<db>__ prefix.
                 g     Generic commands (non-type specific) like DEL, EXPIRE, RENAME, ...
                 s     Set commands
                 z     Sorted set commands
                 x     Expired events (events generated every time a key expires)
                 */
                server.ConfigSet("notify-keyspace-events", "Kszx");
                var conf = _redis.GetServer(_redisHost, _redisPort).ConfigGet();
                var subscriber = _redis.GetSubscriber();
                subscriber.Subscribe("__keyspace@0__:*", OnExpireDeviceId);
            }
            catch (RedisConnectionException err)
            {
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy")+": " + err);
                throw err;
            }
        }

        //https://stackoverflow.com/questions/33196237/how-to-set-expire-when-using-redis-geoadd
        //https://redis.io/topics/notifications
        private async void OnExpireDeviceId(RedisChannel channel, RedisValue value)
        {
            string channelStr = channel.ToString();
            if (channelStr.StartsWith(chanelPrefix) && (string)value == "expired")
            {
                string key = channelStr.Substring(chanelPrefix.Length);
                if (!key.StartsWith(GetGeoEntryKey()))
                {
                    await GeoRemove(key);
                }
            }
        }

        #region Implementation of IRedisClient

        public async Task<bool> SetString(string key, string value)
        {
            var db = Redis.GetDatabase();
            return await await db.StringSetAsync(key, value).
                ContinueWith(x => db.KeyExpireAsync(key, TimeSpan.FromSeconds(_redisKeyExpireTimeout)));
        }

        public async Task<RedisValue> GetString(string key)
        {
            var db = Redis.GetDatabase();
            return await db.StringGetAsync(key);
        }

        public async Task<bool> GeoAdd(double longitude, double latitude, string cardData)
        {
            var db = Redis.GetDatabase();

            RedisKey redisKey = new RedisKey(GetGeoEntryKey());
            RedisValue value = new RedisValue(cardData);

            return await db.GeoAddAsync(redisKey, longitude, latitude, value);
        }

        public async Task<GeoRadiusResult[]> GeoRadiusByMember(string member)
        {
            var db = Redis.GetDatabase();
            return await db.GeoRadiusAsync(GetGeoEntryKey(), member, _redisGeoRadius);
        }

        public async Task<bool> GeoRemove(string device)
        {
            var db = Redis.GetDatabase();
            return await db.GeoRemoveAsync(GetGeoEntryKey(), device);
        }

        public async Task<bool> RemoveKey(string device)
        {
            var db = Redis.GetDatabase();
            return await db.KeyDeleteAsync(device);
        }

        #endregion
    }
}
