using Microsoft.Identity.Client;

namespace AsteroidTracker.Models;

public class ApproachModel
{
    // Formiramo kao AsteroidId + vremenska oznaka prilaska.
    // U formatu AsteroidId _ close_approach_date (Bez razmaka)
    public string ApproachId { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public long EpochDateCloseApproach { get; set; }
    public double MissDistance { get; set; }
    public double RelativeVelocityKps { get; set; }
    public string OrbitingBody { get; set; } = string.Empty;
}