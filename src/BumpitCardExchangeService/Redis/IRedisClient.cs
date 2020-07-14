using StackExchange.Redis;
using System.Threading.Tasks;

namespace BumpitCardExchangeService.Redis
{
    public interface IRedisClient

    {
        ConnectionMultiplexer Redis { get; }
        Task<bool> GeoAdd(string key, double longitude, double latitude, string cardData);
        Task<GeoRadiusResult[]> GeoRadius(string key, double longitude, double latitude);

        Task<GeoRadiusResult[]> GeoRadiusByMember(string key, string member);
        Task<bool> SetString(string key, string value);
        Task<RedisValue> GetString(string key);
        bool GeoRemove(string key, string device);
        Task<bool> RemoveKey(string device);
    }
}