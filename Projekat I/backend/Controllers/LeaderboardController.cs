using AsteroidTracker.Services;

namespace AsteroidTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly RedisLeaderboardService _leaderboards;
    private readonly RedisCacheService _cache;
    private readonly Neo4jAsteroidRepository _repository;
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(
        RedisLeaderboardService leaderboards,
        RedisCacheService cache,
        Neo4jAsteroidRepository repository,
        ILogger<LeaderboardController> logger)
    {
        _leaderboards = leaderboards;
        _cache = cache;
        _repository = repository;
        _logger = logger;
    }

    [HttpGet("largest")]
    public async Task<ActionResult<List<LeaderboardAsteroidModel>>> GetLargest(int count = 10)
    {
        if (count < 1 || count > 100)
            return BadRequest("Count must be between 1 and 100.");

        var entries = await _leaderboards.GetLargestLeaderboardAsync(count);

        return Ok(await MapEntriesAsync(entries, "diameter"));
    }

    [HttpGet("fastest")]
    public async Task<ActionResult<List<LeaderboardAsteroidModel>>> GetFastest(int count = 10)
    {
        if (count < 1 || count > 100)
            return BadRequest("Count must be between 1 and 100.");

        var entries = await _leaderboards.GetFastestLeaderboardAsync(count);

        return Ok(await MapEntriesAsync(entries, "speed"));
    }

    [HttpGet("closest")]
    public async Task<ActionResult<List<LeaderboardAsteroidModel>>> GetClosest(int count = 10)
    {
        if (count < 1 || count > 100)
            return BadRequest("Count must be between 1 and 100.");

        var entries = await _leaderboards.GetClosestLeaderboardAsync(count);

        return Ok(await MapEntriesAsync(entries, "distance"));
    }

    private async Task<List<LeaderboardAsteroidModel>> MapEntriesAsync(StackExchange.Redis.SortedSetEntry[] entries, string rankingType)
    {
        var result = new List<LeaderboardAsteroidModel>();

        // Redis je već sortirao asteroide
        foreach (var entry in entries)
        {
            string asteroidId = entry.Element.ToString();

            // prvo proveravamo keš
            string cacheKey = $"asteroid:{asteroidId}";

            var asteroid = await _cache.GetAsync<AsteroidModel>(cacheKey);

            if (asteroid == null)
            {
                // promašaj keša: čitamo iz Neo4j baze podataka
                asteroid = await _repository.GetAsteroidByIdAsync(asteroidId);

                if (asteroid == null)
                {
                    _logger.LogWarning(
                        "Asteroid {AsteroidId} from leaderboard was not found.",
                        asteroidId);

                    continue;
                }

                await _cache.SetAsync(
                    cacheKey,
                    asteroid,
                    TimeSpan.FromMinutes(30));

                _logger.LogInformation(
                    "Asteroid {AsteroidId} loaded from Neo4j and cached.",
                    asteroidId);
            }
            else
            {
                _logger.LogInformation(
                    "Asteroid {AsteroidId} loaded from Redis cache.",
                    asteroidId);
            }

            var approaches = asteroid.Approaches ?? new List<ApproachModel>();

            var fastest = approaches
                .Where(a => double.IsFinite(a.RelativeVelocityKps))
                .MaxBy(a => a.RelativeVelocityKps);

            var closest = approaches
                .Where(a => double.IsFinite(a.MissDistance))
                .MinBy(a => a.MissDistance);

            double diameter = asteroid.DiameterMaxKm;
            double speed = fastest?.RelativeVelocityKps ?? 0;
            double distance = closest?.MissDistance ?? 0;

            switch (rankingType)
            {
                case "diameter":
                    diameter = entry.Score;
                    break;

                case "speed":
                    speed = entry.Score;
                    break;

                case "distance":
                    distance = entry.Score;
                    break;
            }

            result.Add(new LeaderboardAsteroidModel
            {
                Id = asteroid.Id,
                Name = asteroid.Name,
                SpeedKmS = speed,
                DiameterKm = diameter,
                MissDistanceKm = distance,
                Hazardous = asteroid.IsHazardous,

                // OrbitClass trenutno nemamo, izbrisati ili zameniti?
                OrbitClass = "Unknown"
            });
        }

        return result;
    }
}
