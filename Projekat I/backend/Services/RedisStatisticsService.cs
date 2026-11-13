
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
        // šta ako jedan inkrement prođe, a drugi ne?
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

    // pomoćna metoda za čitanje broja pregleda asteroida
    public async Task<long> GetAsteroidViewsAsync(string id)
    {
        var value = await _database.StringGetAsync(
            $"asteroid:{id}:views"
        );

        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    // pomoćna metoda za čitanje broja pretraga asteroida
    public async Task<long> GetAsteroidSearchesAsync(string id)
    {
        var value = await _database.StringGetAsync(
            $"asteroid:{id}:searches"
        );

        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    // za izabrani asteroid vraća broj pregleda i pretraga
    public async Task<AsteroidStatisticsModel> GetAsteroidStatisticsAsync(string id)
    {
        var viewsTask = GetAsteroidViewsAsync(id);
        var searchesTask = GetAsteroidSearchesAsync(id);

        await Task.WhenAll(viewsTask, searchesTask);

        return new AsteroidStatisticsModel
        {
            AsteroidId = id,
            Views = await viewsTask,
            Searches = await searchesTask
        };
    }

    // pomoćna metoda za čitanje ukupnog broja otvaranja detalja
    public async Task<long> GetTotalViewsAsync()
    {
        var value = await _database.StringGetAsync(
            "stats:totalViews"
        );

        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    // pomoćna metoda za čitanje ukupnog broja pretraga    
    public async Task<long> GetTotalSearchesAsync()
    {
        var value = await _database.StringGetAsync(
            "stats:totalSearches"
        );

        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    // ukupna statistika
    public async Task<GlobalStatisticsModel>
    GetGlobalStatisticsAsync()
    {
        var viewsTask = GetTotalViewsAsync();
        var searchesTask = GetTotalSearchesAsync();

        await Task.WhenAll(viewsTask, searchesTask);

        return new GlobalStatisticsModel
        {
            TotalViews = await viewsTask,
            TotalSearches = await searchesTask
        };
    }
}
