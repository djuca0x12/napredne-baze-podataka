//using System.Text.Json.Serialization;

namespace AsteroidTracker.Models;

public class AsteroidModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double DiameterMinKm { get; set; }
    public double DiameterMaxKm { get; set; }

    // Koliko svetlosti reflektuje.
    public double AbsoluteMagnitude { get; set; }
    public bool IsHazardous { get; set; }

    // Lista zabelezenih prilazaka.
    public List<ApproachModel> Approaches { get; set; } = new();

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