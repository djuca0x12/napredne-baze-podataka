namespace AsteroidTracker.Models;

// zgodno za slanje podataka na front
public class SimilarAsteroidResultModel
{
    public AsteroidModel Asteroid { get; set; } = new();

    public double OverallSimilarity { get; set; }

    public double? CriterionSimilarity { get; set; }

    public string Criterion { get; set; } = string.Empty;
}