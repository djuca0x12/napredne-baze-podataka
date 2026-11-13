
using AsteroidTracker.Repositories;
using Microsoft.OpenApi.Any;

namespace AsteroidTracker.Services;

// Ceo proces, od poziva API do upisa u bazu, pokrece automatski
// BackgroundService - klasa za dugotrajne pozadinske operacije

// Preuzmemo asteroide sa NASA API-ja.
// Sacuvamo ih u Neo4j.
// Iz Neo4j baze preuzmemo sve asteroide i njihove prilaske.
// Izracunamo slicnosti između svih parova.
// Sacuvamo ili azuriramo SIMILAR_TO relacije.
// Nakon poziva API-ja, baza se čisti od zastarelih unosa
public class AsteroidBackgroundService : BackgroundService
{
    private readonly SimpleNasaService _nasaService;
    private readonly Neo4jAsteroidRepository _repository;
    private readonly ILogger<AsteroidBackgroundService> _logger;
    private readonly AsteroidSimilarityService _similarityService;
    private readonly RedisCacheService _cache;
    private readonly RedisLeaderboardService _leaderboards;

    public AsteroidBackgroundService(
        SimpleNasaService nasaService,
        Neo4jAsteroidRepository repository,
        AsteroidSimilarityService similarityService,
        RedisCacheService cache,
        RedisLeaderboardService leaderboards,
        ILogger<AsteroidBackgroundService> logger)
    {
        _nasaService = nasaService;
        _repository = repository;
        _logger = logger;
        _similarityService = similarityService;
        _cache = cache;
        _leaderboards = leaderboards;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation(
                    "Pocinje preuzimanje podataka sa NASA API-ja.");

                // Preuzimamo asteroide za narednih 7 dana
                var newAsteroids = await _nasaService.GetBasicTodayAsteroidsAsync(stoppingToken);

                // Upisujemo nove asteroide i njihove prilaske
                await _repository.SaveAsteroidsAsync(newAsteroids);

                // Brisanje zasterelih podataka iz Neo4J-a
                var cleanup = await _repository.DeleteExpiredAsteroidsAsync();

                _logger.LogInformation(
                    "Ciscenje baze zavrseno. " +
                    "Obrisano asteroida: {DeletedAsteroids}. " +
                    "Obrisano prilazaka: {DeletedApproaches}.",
                    cleanup.DeletedAsteroids,
                    cleanup.DeletedApproaches);

                // Ucitavamo SVE asteroide iz Neo4J baze
                // RAZMISLITI: Da li je ovo dobro/efikasno?
                var allAsteroids = await _repository.GetAllAsteroidsAsync();

                // Racunamo slicnosti izmedju svih mogucih parova asteroida
                var similarities = _similarityService.CalculateAllSimilarities(allAsteroids);

                // Cuvamo ili azuriramo relacije slicnosti
                await _repository.SaveSimilaritiesAsync(similarities);

                // Brisemo zastarele podatke iz kesa
                // todo: da li treba prazniti i redis od zastarelih podataka ili je to automatski?
                await _cache.InvalidateAsteroidCacheAsync(allAsteroids.Select(a => a.Id).ToList());

                // popunjavamo i leaderboard nakon učitavanja novih asteroida i brisanja zastarelih
                await _leaderboards.RebuildLeaderboardsAsync(allAsteroids);

                _logger.LogInformation(
                    "Uspesno obradjeno {NewCount} novih/azuriranih asteroida. " +
                    "Ukupno asteroida u bazi: {TotalCount}. " +
                    "Izracunato slicnosti: {SimilarityCount}.",
                    newAsteroids.Count,
                    allAsteroids.Count,
                    similarities.Count);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Greska prilikom osvezavanja asteroida.");
            }

            try
            {
                // Sledece osvezavanje za 24 sata.
                await Task.Delay(
                    TimeSpan.FromHours(24),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
