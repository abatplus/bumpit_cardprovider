using StackExchange.Redis;
using System.Threading.Tasks;

namespace BumpitCardProvider.Redis
{
    public interface IRedisClient
    {
        void Connect();
        Task<bool> GeoAddAsync(string key, double longitude, double latitude, string cardData);
        Task<GeoRadiusResult[]> GeoRadiusAsync(string key, double longitude, double latitude);
    }
}
