using System.Text.Json;
using StackExchange.Redis;

// Koristimo StackExchange za Redis, umesto
// ServiceStack, oba su validna za komunikaciju sa Redis-om,
// jedino mi je mozda verzija koju mi koristimo
// intiutivnija i laksa za pisanje!

namespace AsteroidTracker.Services;

public class RedisCacheService
{
    // Koristimo za pristup Redis-u.
    private readonly IDatabase _database;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    // Cuvanje objekta u redisu
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        // key -> value(expiration)
        string json = JsonSerializer.Serialize(value);

        await _database.StringSetAsync(key, json, expiration);
    }

    // Citanje kesiranog objekta
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);

        if(value.IsNullOrEmpty)
            return default;
        
        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    // Brisanje kljuca (automatski brise i vrednost iz kljuca)
    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    // Povecanje otvaranja detalja asteroida,
    // Sto automatski treba da poveca i brojac
    // za ukupno otvaranja detalja svih asteroida.
    public async Task IncrementAsteroidViewsAsync(string id)
    {
        // Pronalazimo kljuc oblika ,,asteroid:15:views" i povecavamo
        // vrednost koja se krije iza kljuca
        await _database.StringIncrementAsync($"asteroid:{id}:views");

        await _database.StringIncrementAsync($"stats:totalViews");
    }

    // Povecava broj pretraga konkretnog asteroida.
    public async Task IncrementAsteroidSearchesAsync(string id)
    {
        await _database.StringIncrementAsync($"asteroid:{id}:searches");
    }

    // Povecava ukupan broj uspesnih zahteva za pretragu.
    public async Task IncrementTotalSearchesAsync()
    {
        await _database.StringIncrementAsync("stats:totalSearches");
    }

    // Citanje broja pregleda asteroida.
    public async Task<long> GetAsteroidViewsAsync(string id)
    {
        var value = await _database.StringGetAsync($"asteroid:{id}:views");

        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    // Citanje broja pretraga asteroida.
    public async Task<long> GetAsteroidSearchesAsync(string id)
    {
        var value = await _database.StringGetAsync($"asteroid:{id}:searches");

        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    // Ukupan broj otvaranja detalja
    public async Task<long> GetTotalViewsAsync()
    {
        var value = await _database.StringGetAsync("stats:totalViews");

        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    // Ukupan broj pretraga
    public async Task<long> GetTotalSearchesAsync()
    {
        var value = await _database.StringGetAsync("stats:totalSearches");

        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    // Varijante metoda za dodavanje u SortedSet
    // u zavisnosti za koji leaderboard su.
    public async Task AddToLargestLeaderboardAsync(string asteroidId, double diameter)
    {
        // (kljuc celog leaderboarda, id asteroida, vrednost po kojoj se sortiraju)
        await _database.SortedSetAddAsync("leaderboard:largest", asteroidId, diameter);
    }

    public async Task AddToFastestLeaderboardAsync(string asteroidId, double speed)
    {
        // (kljuc celog leaderboarda, id asteroida, vrednost po kojoj se sortiraju)
        await _database.SortedSetAddAsync("leaderboard:fastest", asteroidId, speed);
    }

    public async Task AddToClosestLeaderboardAsync(string asteroidId, double missDistance)
    {
        // (kljuc celog leaderboarda, id asteroida, vrednost po kojoj se sortiraju)
        await _database.SortedSetAddAsync("leaderboard:closest", asteroidId, missDistance);
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

        // Brisanje kesa leaderboard-a.
        await _database.KeyDeleteAsync("leaderboard:largest");
        await _database.KeyDeleteAsync("leaderboard:fastest");
        await _database.KeyDeleteAsync("leaderboard:closest");

        // Da li cemo brisati vrednosti brojaca?
    }
}