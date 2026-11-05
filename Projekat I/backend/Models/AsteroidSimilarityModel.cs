namespace AsteroidTracker.Models;

public class AsteroidSimilarityModel
{
    public string FirstAsteroidId { get; set; } = string.Empty;
    public string SecondAsteroidId { get; set; } = string.Empty;
    public double OverallSimilarity { get; set; }

    // Pojedinacne slicnosti razlicitih parametara
    public double? DiameterSimilarity { get; set; }
    public double? AbsoluteMagnitudeSimilarity { get; set; }
    public double? HazardStatusSimilarity { get; set; }
    public double? VelocitySimilarity { get; set; }
    public double? MissDistanceSimilarity { get; set; }
}