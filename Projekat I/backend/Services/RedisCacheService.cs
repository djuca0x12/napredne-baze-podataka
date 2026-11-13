using System.Text.Json;
using StackExchange.Redis;

namespace WebTemplate.Services;

public class RedisCacheService
{
    private readonly IDatabase _database;

    // versioned cache keys:
    // redis može da invalidira sve rezultate pretrage jednim atomičnim inkrementom,
    // umesto da se radi scan za sve searches ključeve
    private const string SearchCacheVersionKey = "cache:version:asteroid-search";

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    // kreira versioned search cache key
    // +1 na verziju invalidira sve prethodne ključeve za pretragu
    // tj. logički invalidira sve stare unose iz keša, koji se fizički obrišu kad istekne TTL
    // nema potrebe da ručno brišemo npr. asteroid:search:v1:2019
    public async Task<string> GetSearchCacheKeyAsync(string normalizedName)
    {
        var version = await _database.StringGetAsync(
            SearchCacheVersionKey
        );

        string currentVersion = version.IsNullOrEmpty
            ? "0"
            : version.ToString();

        return $"asteroid:search:v{currentVersion}:{normalizedName}";
    }

    // čuvanje objekata u Redisu sa ograničenim TTL
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        // key -> value(expiration)
        string json = JsonSerializer.Serialize(value);

        await _database.StringSetAsync(key, json, expiration);
    }

    // čitanje keširanog objekta
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    // brisanje vrednosti po ključu (automatski briše i vrednost)
    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    // Invalidacija kesiranih podataka
    // Potpuna invalidacija - sto znaci da cemo obrisati
    // i podatke koji su posle ucitavanja idalje azurni!
    public async Task InvalidateAsteroidCacheAsync(List<string> asteroidIds)
    {
        // Brisanje kesa svih asteroida i svih slicnosti.
        await _database.KeyDeleteAsync("asteroids:all");
        await _database.KeyDeleteAsync("similarities:all");

        // Brisanje kesa pojedinacnih asteroida i njihovih slicnosti.
        foreach (var id in asteroidIds)
        {
            await _database.KeyDeleteAsync($"asteroid:{id}");
        }

        // brisanje keša leaderboard-a: prebačeno u metodu koja osvežava leaderboard!
        // await _database.KeyDeleteAsync("leaderboard:largest");
        // await _database.KeyDeleteAsync("leaderboard:fastest");
        // await _database.KeyDeleteAsync("leaderboard:closest");


        // takođe, keširani rezultati pretrage su ovde invalidirani, odjednom svi!
        // stari ključečvi postanu nevalidni logički promenom verzije, a onda i fizički istekom TTL-a
        /*
            pre sinhronizacije:
            asteroid:search:v0:2019
            asteroid:search:v0:433

            posle sinhronizacije:
            asteroid:search:v1:2019
            asteroid:search:v1:433
        */
        await _database.StringIncrementAsync("cache:version:asteroid-search");

    }

}