using StackExchange.Redis;
using System.Threading.Tasks;

namespace CardExchangeService.Redis
{
    public interface IRedisClient
    {
        Task<bool> GeoAdd(double longitude, double latitude, string cardData);
        Task<GeoRadiusResult[]> GeoRadiusByMember(string member);
        Task<bool> SetString(string key, string value);
        Task<RedisValue> GetString(string key);
        Task<bool> GeoRemove(string device);
        Task<bool> RemoveKey(string device);
    }
}