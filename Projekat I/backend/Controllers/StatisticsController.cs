namespace AsteroidTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly RedisStatisticsService _statistics;

    public StatisticsController(
        RedisStatisticsService statistics)
    {
        _statistics = statistics;
    }

    // GET: /api/statistics
    // Global application statistics
    [HttpGet]
    public async Task<ActionResult<GlobalStatisticsModel>> GetGlobalStatistics()
    {
        var statistics =
            await _statistics.GetGlobalStatisticsAsync();

        return Ok(statistics);
    }

    // GET: /api/statistics/asteroids/3841634
    [HttpGet("asteroids/{id}")]
    public async Task<ActionResult<AsteroidStatisticsModel>> GetAsteroidStatistics(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("Invalid asteroid ID.");

        var statistics = await _statistics.GetAsteroidStatisticsAsync(id);

        return Ok(statistics);
    }

    // GET: /api/statistics/asteroids/3841634/views
    [HttpGet("asteroids/{id}/views")]
    public async Task<ActionResult<long>> GetAsteroidViews(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("Invalid asteroid ID.");

        var views = await _statistics.GetAsteroidViewsAsync(id);

        return Ok(views);
    }

    // GET: /api/statistics/asteroids/3841634/searches
    [HttpGet("asteroids/{id}/searches")]
    public async Task<ActionResult<long>> GetAsteroidSearches(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("Invalid asteroid ID.");

        var searches = await _statistics.GetAsteroidSearchesAsync(id);

        return Ok(searches);
    }
}
