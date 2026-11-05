using AsteroidTracker.Models;
using AsteroidTracker.Repositories;
using AsteroidTracker.Services;

namespace AsteroidTracker.Controllers;

[ApiController]
[Route("[controller]")]
public class CounterController : ControllerBase
{
    private readonly Neo4jAsteroidRepository _repository;
    private readonly RedisCacheService _cache;

    // Dodao sam loger da bi smo u svakoj metodi mogli da proverimo
    // da li se podaci citaju iz kesa ili baze.
    private readonly ILogger<AsteroidController> _logger;

    public CounterController(Neo4jAsteroidRepository rep, RedisCacheService red, ILogger<AsteroidController> log)
    {
        _repository = rep;
        _cache = red;
        _logger = log;
    }

    
}
