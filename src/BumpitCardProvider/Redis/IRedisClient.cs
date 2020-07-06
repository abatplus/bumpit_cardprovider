using StackExchange.Redis;
using System.Threading.Tasks;

namespace BumpitCardProvider.Redis
{
    public interface IRedisClient
    {
        void Connect();
        Task<bool> SetStringAsync(string key, string value);
        Task<RedisValue> GetStringAsync(string key);
    }
}
