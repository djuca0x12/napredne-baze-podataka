
namespace AsteroidTracker.Models;

public class LeaderboardAsteroidModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public double SpeedKmS { get; set; }
    public double DiameterKm { get; set; }
    public double MissDistanceKm { get; set; }

    public bool Hazardous { get; set; }

    public string OrbitClass { get; set; } = "Unknown";
}
