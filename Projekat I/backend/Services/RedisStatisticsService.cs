
using StackExchange.Redis;

namespace WebTemplate.Services;

public class RedisStatisticsService
{
    private readonly IDatabase _database;

    public RedisStatisticsService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    // Povecanje otvaranja detalja asteroida,
    // Sto automatski treba da poveca i brojac za ukupno otvaranja detalja svih asteroida.
    public async Task IncrementAsteroidViewsAsync(string id)
    {
        await _database.StringIncrementAsync(
            $"asteroid:{id}:views"
        );

        await _database.StringIncrementAsync(
            "stats:totalViews"
        );
    }

    // Povecava broj pretraga konkretnog asteroida.
    public async Task IncrementAsteroidSearchesAsync(string id)
    {
        await _database.StringIncrementAsync(
            $"asteroid:{id}:searches"
        );
    }

    // Povecava ukupan broj uspesnih zahteva za pretragu.
    public async Task IncrementTotalSearchesAsync()
    {
        await _database.StringIncrementAsync(
            "stats:totalSearches"
        );
    }

    // Citanje broja pregleda asteroida.
    public async Task<long> GetAsteroidViewsAsync(string id)
    {
        var value = await _database.StringGetAsync(
            $"asteroid:{id}:views"
        );

        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    // Citanje broja pretraga asteroida.
    public async Task<long> GetAsteroidSearchesAsync(string id)
    {
        var value = await _database.StringGetAsync(
            $"asteroid:{id}:searches"
        );

        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    // Ukupan broj otvaranja detalja
    public async Task<long> GetTotalViewsAsync()
    {
        var value = await _database.StringGetAsync(
            "stats:totalViews"
        );

        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    // Ukupan broj pretraga
    public async Task<long> GetTotalSearchesAsync()
    {
        var value = await _database.StringGetAsync(
            "stats:totalSearches"
        );

        return value.IsNullOrEmpty ? 0 : (long)value;
    }
}
