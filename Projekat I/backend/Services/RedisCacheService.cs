using System.Text.Json;
using StackExchange.Redis;

namespace WebTemplate.Services;

public class RedisCacheService
{
    private readonly IDatabase _database;

    // versioned cache keys:
    // redis može da invalidira sve rezultate pretrage jednim atomičnim inkrementom,
    // umesto da se radi scan za sve searches ključeve
    // private const string SearchCacheVersionKey = "cache:version:asteroid-search";

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
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

        // ključevi za leaderboard i statistiku su van keš invalidacije?
        // takođe, rezultati pretrage nisu ovde invalidirani
    }

}