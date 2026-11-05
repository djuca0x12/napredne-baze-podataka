using AsteroidTracker.Models;
using AsteroidTracker.Repositories;
using AsteroidTracker.Services;

namespace AsteroidTracker.Controllers;

[ApiController]
[Route("[controller]")]
public class SimilarityController : ControllerBase
{
    private readonly Neo4jAsteroidRepository _repository;
    private readonly RedisCacheService _cache;

    // Dodao sam loger da bi smo u svakoj metodi mogli da proverimo
    // da li se podaci citaju iz kesa ili baze.
    private readonly ILogger<AsteroidController> _logger;

    public SimilarityController(Neo4jAsteroidRepository rep, RedisCacheService red, ILogger<AsteroidController> log)
    {
        _repository = rep;
        _cache = red;
        _logger = log;
    }

    [HttpGet("id/criterion")]
    public async Task<ActionResult> GetSimilarAsteroids(string id, string criterion)
    {
        string key = $"asteroid:{id}:similar:{criterion.ToLower()}";

        var similarities = await _cache.GetAsync<List<AsteroidSimilarityModel>>(key);

        if(similarities != null)
        {
            _logger.LogInformation("Podaci su procitani iz kesa");

            return Ok(similarities);
        }
        else
        {
            try
            {
                similarities = await _repository.GetSimilarAsteroidsAsync(id, criterion, 0.5);
            }
            catch(ArgumentException)
            {
                return BadRequest("Nepoznat kriterijum slicnosti");
            }

            _logger.LogInformation("Podaci su procitani iz baze, i upisani u kes");            
        }


        return Ok(similarities);
    }
}
