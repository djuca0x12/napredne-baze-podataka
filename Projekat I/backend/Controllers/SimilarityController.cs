using AsteroidTracker.Models;
using AsteroidTracker.Services;

namespace AsteroidTracker.Controllers;

[ApiController]
[Route("[controller]")]
public class SimilarityController : ControllerBase
{
    private readonly Neo4jAsteroidRepository _repository;
    private readonly RedisCacheService _cache;
    private readonly ILogger<SimilarityController> _logger;

    public SimilarityController(Neo4jAsteroidRepository rep, RedisCacheService red, ILogger<SimilarityController> log)
    {
        _repository = rep;
        _cache = red;
        _logger = log;
    }

    [HttpGet("{id}/{criterion}")]
    public async Task<ActionResult> GetSimilarAsteroids([FromRoute] string id, [FromRoute] string criterion)
    {
        _logger.LogInformation("Similarity endpoint reached. ID: {Id}, Criterion: {Criterion}", id, criterion);

        string key = $"asteroid:{id}:similar:{criterion.ToLowerInvariant()}";

        var similarities =
            await _cache.GetAsync<List<AsteroidSimilarityModel>>(key);

        if (similarities != null)
        {
            _logger.LogInformation("Sličnosti pročitane iz keša");
        }

        if (similarities == null)
        {
            try
            {
                similarities = await _repository.GetSimilarAsteroidsAsync(
                    id,
                    criterion,
                    0.85
                );

                _logger.LogInformation("Sličnosti pročitane iz Neo4j baze");
                // todo: upis u keš?
            }
            catch (ArgumentException)
            {
                return BadRequest("Nepoznat kriterijum slicnosti");
            }
        }

        if (similarities == null || similarities.Count == 0)
            return Ok(new List<SimilarAsteroidResultModel>());

        var results = new List<SimilarAsteroidResultModel>();

        foreach (var similarity in similarities)
        {
            // repository lookup, ne /Asteroid/id
            // /Asteroid/id endpoint inkrementira broj pregleda samo kad se pregleda asteroid nakon pretrage
            // todo: da li da ostane tako?
            var asteroid = await _repository.GetAsteroidByIdAsync(similarity.SecondAsteroidId);

            if (asteroid == null)
                continue;

            double? criterionSimilarity = criterion.ToLowerInvariant() switch
            {
                "velocity" => similarity.VelocitySimilarity,
                "diameter" => similarity.DiameterSimilarity,
                "missdistance" => similarity.MissDistanceSimilarity,
                _ => null
            };

            results.Add(new SimilarAsteroidResultModel
            {
                Asteroid = asteroid,
                OverallSimilarity = similarity.OverallSimilarity,
                CriterionSimilarity = criterionSimilarity,
                Criterion = criterion
            });
        }

        return Ok(results);
    }
}
