
using StackExchange.Redis;

namespace WebTemplate.Services;

public class RedisLeaderboardService
{
    private readonly IDatabase _database;

    public RedisLeaderboardService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    // dodavanje pojedinačnih vrednosti u leaderboard (sorted set)
    public async Task AddToLargestLeaderboardAsync(
        string asteroidId,
        double diameter)
    {
        await _database.SortedSetAddAsync(
            "leaderboard:largest",  // ključ
            asteroidId,             // id asteroida
            diameter                // vrednost po kojoj se sortiraju
        );
    }

    public async Task AddToFastestLeaderboardAsync(
        string asteroidId,
        double speed)
    {
        await _database.SortedSetAddAsync(
            "leaderboard:fastest",
            asteroidId,
            speed
        );
    }

    public async Task AddToClosestLeaderboardAsync(
        string asteroidId,
        double missDistance)
    {
        await _database.SortedSetAddAsync(
            "leaderboard:closest",
            asteroidId,
            missDistance
        );
    }

    // poziva se iz AsteroidBackgroundService, da ažurira svaki leaderboard
    public async Task RebuildLeaderboardsAsync(
        List<AsteroidModel> asteroids)
    {
        await _database.KeyDeleteAsync("leaderboard:largest");
        await _database.KeyDeleteAsync("leaderboard:fastest");
        await _database.KeyDeleteAsync("leaderboard:closest");

        // prolazak kroz sve asteroide u listi i dodavanje u odgovarajući leaderboard
        foreach (var asteroid in asteroids)
        {
            // asteroidi najvećeg prečnika
            if (double.IsFinite(asteroid.DiameterMaxKm) &&
                asteroid.DiameterMaxKm > 0)
            {
                await AddToLargestLeaderboardAsync(
                    asteroid.Id,
                    asteroid.DiameterMaxKm
                );
            }
            if (asteroid.Approaches == null ||
                asteroid.Approaches.Count == 0)
            {
                continue;
            }

            // asteroidi najveće brzine kretanja
            var fastest = asteroid.Approaches
                .Where(a => double.IsFinite(a.RelativeVelocityKps))
                .MaxBy(a => a.RelativeVelocityKps);

            if (fastest != null)
            {
                await AddToFastestLeaderboardAsync(
                    asteroid.Id,
                    fastest.RelativeVelocityKps
                );
            }

            // asteroidi najveće blizine udara
            var closest = asteroid.Approaches
                .Where(a => double.IsFinite(a.MissDistance))
                .MinBy(a => a.MissDistance);

            if (closest != null)
            {
                await AddToClosestLeaderboardAsync(
                    asteroid.Id,
                    closest.MissDistance
                );
            }
        }
    }

    // prikaz prvih N podataka iz Redis leaderboard-a
    public async Task<SortedSetEntry[]> GetLargestLeaderboardAsync(
        int count = 10)
    {
        // vraća opseg elemenata [0 - count-1] po ključu, iz sorted set strukture
        // default poredak je od manjih ka većim vrednostima
        return await _database.SortedSetRangeByRankWithScoresAsync(
            "leaderboard:largest",
            0,
            count > 0 ? count - 1 : 9,  // default 10 vrednosti, čak i ako negativna bude preneta
            Order.Descending
        );
    }

    public async Task<SortedSetEntry[]> GetFastestLeaderboardAsync(
        int count = 10)
    {
        return await _database.SortedSetRangeByRankWithScoresAsync(
            "leaderboard:fastest",
            0,
            count > 0 ? count - 1 : 9,
            Order.Descending
        );
    }

    public async Task<SortedSetEntry[]> GetClosestLeaderboardAsync(
        int count = 10)
    {
        return await _database.SortedSetRangeByRankWithScoresAsync(
            "leaderboard:closest",
            0,
            count > 0 ? count - 1 : 9,
            Order.Ascending
        );
    }
}
