using AsteroidTracker.Models;
using AsteroidTracker.Services;

namespace AsteroidTracker.Controllers;

[ApiController]
[Route("[controller]")]
public class AsteroidController : ControllerBase
{
    private readonly Neo4jAsteroidRepository _repository;
    private readonly RedisCacheService _cache;
    private readonly RedisLeaderboardService _leaderboards;
    private readonly RedisStatisticsService _statistics;

    // Dodao sam loger da bi smo u svakoj metodi mogli da proverimo
    // da li se podaci citaju iz kesa ili baze.
    private readonly ILogger<AsteroidController> _logger;

    public AsteroidController(Neo4jAsteroidRepository rep, RedisCacheService cache, RedisLeaderboardService leaderboards,
    RedisStatisticsService statistics, ILogger<AsteroidController> log)
    {
        _repository = rep;
        _cache = cache;
        _leaderboards = leaderboards;
        _statistics = statistics;
        _logger = log;
    }

    // Vracanje svih asteroida
    // Trenutno se vracaju podaci o asteroidu i njegovim prilascima
    // Razmisliti da li je to potrebno podeliti na 2 metode ovde,
    // taman da mozemo da inkrementiramo brojac pregleda detalja lakse
    // i logicnije (u metodi koja vraca detalje?)
    [HttpGet]
    public async Task<ActionResult> GetAllAsteroids()
    {
        // Prvo proveravamo kes:
        var cachedAsteroids = await _cache.GetAsync<List<AsteroidModel>>("asteroids:all");

        if (cachedAsteroids != null)
        {
            _logger.LogInformation("Podaci su procitani iz kesa");
            return Ok(cachedAsteroids);
        }

        // Nemamo podataka u kesu, citamo iz baze, upisujemo u kes
        // i vracamo asteroide nazad
        var asteroids = await _repository.GetAllAsteroidsAsync();

        await _cache.SetAsync(
            "asteroids:all",
            asteroids,
            TimeSpan.FromMinutes(30)
        );

        _logger.LogInformation("Podaci su procitani iz baze, i upisani u kes");

        return Ok(asteroids);
    }


    // Vracanje asteroida po id-u
    [HttpGet("id")]
    public async Task<ActionResult> GetAsteroidById([FromQuery] string id)
    {
        string key = $"asteroid:{id}"; // Redis podatak

        var asteroid = await _cache.GetAsync<AsteroidModel>(key);

        if (asteroid != null)
            _logger.LogInformation("Asteroid {id} učitan iz keša", id);

        else
        {
            _logger.LogInformation("Asteroid {id} nije pronađen u kešu, traži se u Neo4j", id);
            asteroid = await _repository.GetAsteroidByIdAsync(id);

            if (asteroid == null)
                return NotFound("Asteroida nije pronađen");

            await _cache.SetAsync(
                key,
                asteroid,
                TimeSpan.FromMinutes(30)
            );

            _logger.LogInformation("Asteroid {Id} pročitan iz Neo4j i upisan u Redis.", id);
        }

        // Inkrementiranje broja pregleda detalja
        await _statistics.IncrementAsteroidViewsAsync(id);

        return Ok(asteroid);
    }

    [HttpGet("name")]
    public async Task<ActionResult> SearchAsteroids([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Morate uneti naziv asteroida");

        // Sredjujemo naziv:
        string normalizedName = name.Trim().ToLowerInvariant();

        // Redis kljuc: 
        // todo: da li je validan format ključa za više potencijalnih rezultata? dodati hash/brojač/uuid?
        //string key = $"asteroid:search:{normalizedName}";
        string key = await _cache.GetSearchCacheKeyAsync(normalizedName);

        var result = await _cache.GetAsync<List<AsteroidModel>>(key);

        if (result != null)
        {
            _logger.LogInformation("Podaci su procitani iz kesa");
        }
        else
        {
            result = await _repository.SearchAsteroidsByNameAsync(normalizedName);

            if (result == null || result.Count == 0)
            {
                // vraća se prazna lista, kako se brojači ne bi inkrementirali naknadno
                return Ok(new List<AsteroidModel>());
            }

            await _cache.SetAsync(
                key,
                result,
                TimeSpan.FromMinutes(30)
            );

            _logger.LogInformation("Podaci su procitani iz baze, i upisani u kes");
        }

        // Povecava se broj ukupnih pretraga:
        await _statistics.IncrementTotalSearchesAsync();

        // Kao i broj pretraga pojedinacnih asteroida
        foreach (var asteroid in result)
        {
            await _statistics.IncrementAsteroidSearchesAsync(asteroid.Id);
        }

        return Ok(result);
    }
}
