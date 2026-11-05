using AsteroidTracker.Models;

namespace AsteroidTracker.Services;

public class AsteroidSimilarityService
{
    // Ovo je metoda koju cemo koristiti tokom upisa podataka u Neo4J,
    // za pravljenje strukture 2 cvora (Asteroid i Prilaz), kao i grane
    // (Slinost), prilikom pisanja upita koristicemo tu strukturu 
    // (prednosti graf baze), a ne ovu metodu ponovo! 
    public AsteroidSimilarityModel CalculateSimilarity(AsteroidModel first, AsteroidModel second)
    {
        // Pronalazimo najblizi prilazak za oba asteroida
        var firstClosest = GetClosestApproach(first);
        var secondClosest = GetClosestApproach(second);

        // 1. Slicnost po PRECNIKU:
        double firstDiameter = (first.DiameterMaxKm + first.DiameterMinKm) / 2.0;
        double secondDiameter = (second.DiameterMaxKm + second.DiameterMinKm) / 2.0;

        double? diameterSimilarity = CalculateNumericSimilarity(firstDiameter, secondDiameter);

        // 2. Slicnost po MAGNITUDI:
        double? magnitudeSimilarity = CalculateMagnitudeSimilarity(first.AbsoluteMagnitude, second.AbsoluteMagnitude);

        // 3. Slicnost po OPASNOSTI:
        double? hazardSimilarity = first.IsHazardous == second.IsHazardous ? 1.0 : 0.0;

        // 4. Slicnost po BRZINI:
        double? velocitySimilarity = null;

        // 5. Slicnost po UDALJENOSTI:
        double? missDistanceSimilarity = null;

        // 4. i 5. jedino ima smisla racunati kada
        // oba asteroida imaju prilaske Zemlji (ili cemu vec) 
        if(firstClosest != null && secondClosest != null)
        {
            velocitySimilarity = CalculateNumericSimilarity(firstClosest.RelativeVelocityKps, secondClosest.RelativeVelocityKps);

            missDistanceSimilarity = CalculateNumericSimilarity(firstClosest.MissDistance, secondClosest.MissDistance);
        }

        // Racunanje ukupne slicnost.
        // Koristimo tezinu/weight da bi smo odredili
        // koliko ce koji parametar imati uticaja.

        double totalScore = 0;
        double totalWeight = 0;

        AddToTotal(diameterSimilarity, 0.30,
                ref totalScore, ref totalWeight);

        AddToTotal(magnitudeSimilarity, 0.20,
            ref totalScore, ref totalWeight);

        AddToTotal(hazardSimilarity, 0.10,
            ref totalScore, ref totalWeight);

        AddToTotal(velocitySimilarity, 0.20,
            ref totalScore, ref totalWeight);

        AddToTotal(missDistanceSimilarity, 0.20,
            ref totalScore, ref totalWeight);

        double overallSimilarity = totalWeight > 0 ? totalScore / totalWeight : 0;

        // Vracamo objekat koji sadrzi pojedinacne procene slicnosti
        // kao i ukupnu slicnost.
        return new AsteroidSimilarityModel
        {
            FirstAsteroidId = first.Id,
            SecondAsteroidId = second.Id,

            OverallSimilarity = Math.Round(
                overallSimilarity, 4),

            DiameterSimilarity = RoundScore(diameterSimilarity),

            AbsoluteMagnitudeSimilarity =
                RoundScore(magnitudeSimilarity),

            HazardStatusSimilarity =
                RoundScore(hazardSimilarity),

            VelocitySimilarity = RoundScore(velocitySimilarity),

            MissDistanceSimilarity =
                RoundScore(missDistanceSimilarity)
        };
    }

    // Formula preko koje izracunavamo koliko su 2 parametra slicna
    // u opsegu od 0%-100% / 0-1.
    private double CalculateNumericSimilarity(double first, double second)
    {
        double maxValue = Math.Max(Math.Abs(first), Math.Abs(second));

        if(maxValue == 0)
            return 1;
        
        return Math.Max(0, 1 - Math.Abs(first-second) / maxValue);
    }

    // Posebna formula za apsolutnu magnitudu
    // jer ona koristi logaritamsku skalu
    private double CalculateMagnitudeSimilarity(double first, double second)
    {
        return Math.Max(
            0,
            1 - Math.Abs(first - second) / 10.0
        );
    }

    // Pronalazenje najblizeg prilaska
    private ApproachModel? GetClosestApproach(AsteroidModel asteroid)
    {
        return asteroid.Approaches
                .OrderBy(a => a.MissDistance)
                .FirstOrDefault();
    }

    // Dodavanje rezultata u ukupnu slicnost
    private void AddToTotal(double? similarity, double weight,
                            ref double totalScore, ref double totalWeight)
    {
        if(similarity == null)
            return;
        
        totalScore += similarity.Value * weight;
        totalWeight += weight;
    }

    // Zaokruzivanje rezultata
    private double? RoundScore(double? score)
    {
        return score.HasValue ? Math.Round(score.Value, 4) : null;
    }

    // Racuna slicnost izmedju svih mogucih asteroida
    public List<AsteroidSimilarityModel> CalculateAllSimilarities(List<AsteroidModel> asteroids)
    {
        var similarities = new List<AsteroidSimilarityModel>();

        // Prolazimo kroz sve moguce kombinacije asteroida
        for(int i=0; i<asteroids.Count; i++)
        {
            for(int j=i+1; j<asteroids.Count; j++)
            {
                var similarity = CalculateSimilarity(asteroids[i], asteroids[j]);
                similarities.Add(similarity);
            }
        }

        return similarities;
    }
}