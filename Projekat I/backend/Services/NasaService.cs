using System.Globalization;
using System.Text.Json;
using AsteroidTracker.Models;

namespace AsteroidTracker.Services;

// Preuzimanje i obrada podataka sa API-a.
// Nema dodira sa Neo4J bazom.
public class SimpleNasaService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public SimpleNasaService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(config["NasaApi:BaseUrl"] ?? "https://api.nasa.gov/neo/rest/v1/");
        _apiKey = config["NasaApi:ApiKey"] ?? "DEMO_KEY";
    }

    // Preuzima podatke o asteroidima za narednih 7 dana.
    public async Task<List<AsteroidModel>> GetBasicTodayAsteroidsAsync(
        CancellationToken cancellationToken = default)
    {
        string startDate = DateTime.UtcNow.ToString(
            "yyyy-MM-dd", CultureInfo.InvariantCulture);

        string endDate = DateTime.UtcNow.Date
            .AddDays(6)
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        string url =
            $"feed?start_date={startDate}" +
            $"&end_date={endDate}&api_key={_apiKey}";

        // CancellationToken se koristi da bi preuzmimanje 
        // moglo da se prekine (tip kada se aplikacija gasi)
        using var response = await _httpClient.GetAsync(
            url, cancellationToken);

        // Proverava da li odogovor uspesan i baca
        // izuzetak ako nije!
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(
                cancellationToken));

        var list = new List<AsteroidModel>();

        var neoObj = doc.RootElement.GetProperty(
            "near_earth_objects");

        // Prolazimo kroz sve datume koje je nasa vratila
        foreach (var day in neoObj.EnumerateObject())
        {
            foreach (var item in day.Value.EnumerateArray())
            {
                // Kreiramo novi asteroid sa podacima
                var asteroid = new AsteroidModel
                {
                    Id = item.GetProperty("id").GetString() ?? "",

                    Name = item.GetProperty("name").GetString() ?? "",

                    IsHazardous = item
                        .GetProperty("is_potentially_hazardous_asteroid")
                        .GetBoolean(),

                    DiameterMinKm = item
                        .GetProperty("estimated_diameter")
                        .GetProperty("kilometers")
                        .GetProperty("estimated_diameter_min")
                        .GetDouble(),

                    DiameterMaxKm = item
                        .GetProperty("estimated_diameter")
                        .GetProperty("kilometers")
                        .GetProperty("estimated_diameter_max")
                        .GetDouble(),
                    
                    AbsoluteMagnitude = item
                        .GetProperty("absolute_magnitude_h")
                        .GetDouble()
                };

                if (item.TryGetProperty(
                        "close_approach_data", out var approaches))
                {
                    foreach(var approach in approaches.EnumerateArray())
                    {
                        string epochString = approach
                            .GetProperty("epoch_date_close_approach")
                            .GetRawText();
                        
                        long.TryParse(
                            epochString,
                            out long epoch);

                        string speedString = approach
                            .GetProperty("relative_velocity")
                            .GetProperty("kilometers_per_second")
                            .GetString() ?? "0";

                        string distanceString = approach
                            .GetProperty("miss_distance")
                            .GetProperty("kilometers")
                            .GetString() ?? "0";

                        double.TryParse(
                            speedString,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out double velocity);
                        
                        double.TryParse(
                            distanceString,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out double distance);

                        asteroid.Approaches.Add(new ApproachModel
                        {
                            // Jedinstvena oznaka svakog prilaska.
                            ApproachId = $"{asteroid.Id}_{epoch}",

                            Date = approach
                                .GetProperty("close_approach_date")
                                .GetString() ?? "",

                            EpochDateCloseApproach = epoch,

                            RelativeVelocityKps = velocity,

                            MissDistance = distance,

                            OrbitingBody = approach
                                .GetProperty("orbiting_body")
                                .GetString() ?? ""
                        });
                    }
                }

                list.Add(asteroid);
            }
        }

        // Isti asteroid moze biti vracen za vise datuma
        // Spajamo ga u jedan objekat i objedinjujemo prilaske
        return list
            .GroupBy(a => a.Id) // grupisemo asteroide po id-u
            .Select(group => // za svaku grupu
            {
                var asteroid = group.First(); // uzimamo prvi asteroid iz grupe

                asteroid.Approaches = group
                    .SelectMany(a => a.Approaches) // uzimamo sve prilaske asteroida
                    .GroupBy(a => a.ApproachId) // grupisemo ih po id-u
                    .Select(g => g.First()) // uzimamo samo prvi iz duplikata grupisanih po id-u
                    .ToList();

                return asteroid; // sredjen asteroid sa prilazima
            })
            .ToList();

        /*
        Asteroid
        ├── Id
        ├── Name
        ├── DiameterMinKm
        ├── DiameterMaxKm
        ├── AbsoluteMagnitude
        ├── IsHazardous
        └── Approaches
            ├── Approach 1
            ├── Approach 2
            └── Approach 3
        */
    }

}