
using AsteroidTracker.Models;
using Neo4j.Driver;

namespace AsteroidTracker.Repositories;

// Komunicira sa Neo4J radi cuvanja podataka.
public class Neo4jAsteroidRepository
{
    // Objekat preko kog komuniciramo sa bazom!
    private readonly IDriver _driver;

    public Neo4jAsteroidRepository(IDriver driver)
    {
        _driver = driver;
    }

    // Cuva asteroide i njihove prilaske.
    public async Task SaveAsteroidsAsync(
        List<AsteroidModel> asteroids)
    {
        if (asteroids.Count == 0)
            return;

        await using var session = _driver.AsyncSession();

        await session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(
                """
                UNWIND $asteroids AS a

                MERGE (n:Asteroid {id: a.id})

                SET n.name = a.name,
                    n.isHazardous = a.isHazardous,
                    n.diameterMaxKm = a.diameterMaxKm,
                    n.diameterMinKm = a.diameterMinKm,
                    n.absoluteMagnitude = a.absoluteMagnitude,
                    n.updatedAt = datetime()

                WITH n, a
                UNWIND a.approaches AS p

                MERGE (ap:Approach {approachId: p.approachId})

                SET ap.date = p.date,
                    ap.epochDateCloseApproach = p.epochDateCloseApproach,
                    ap.missDistance = p.missDistance,
                    ap.relativeVelocityKps = p.relativeVelocityKps,
                    ap.orbitingBody = p.orbitingBody

                MERGE (n)-[:HAS_APPROACH]->(ap)
                """,
                new
                {
                    asteroids = asteroids.Select(a => new
                    {
                        id = a.Id,
                        name = a.Name,
                        isHazardous = a.IsHazardous,
                        diameterMaxKm = a.DiameterMaxKm,
                        diameterMinKm = a.DiameterMinKm,
                        absoluteMagnitude = a.AbsoluteMagnitude,

                        approaches = a.Approaches.Select(p => new
                        {
                            approachId = p.ApproachId,
                            date = p.Date,
                            epochDateCloseApproach = p.EpochDateCloseApproach,
                            missDistance = p.MissDistance,
                            relativeVelocityKps = p.RelativeVelocityKps,
                            orbitingBody = p.OrbitingBody
                        }).ToList()
                    }).ToList()
                });

            await result.ConsumeAsync();
        });
    }

    // Cuva vec izracunate slicnosti izmedju parova asteroida.
    public async Task SaveSimilaritiesAsync(
        List<AsteroidSimilarityModel> similarities)
    {
        if (similarities.Count == 0)
            return;

        await using var session = _driver.AsyncSession();

        await session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(
                """
                UNWIND $similarities AS s

                MATCH (a:Asteroid {id: s.firstId})
                MATCH (b:Asteroid {id: s.secondId})

                MERGE (a)-[r:SIMILAR_TO]->(b)

                SET r.overallSimilarity = s.overallSimilarity,
                    r.diameterSimilarity = s.diameterSimilarity,
                    r.absoluteMagnitudeSimilarity = s.absoluteMagnitudeSimilarity,
                    r.hazardStatusSimilarity = s.hazardStatusSimilarity,
                    r.velocitySimilarity = s.velocitySimilarity,
                    r.missDistanceSimilarity = s.missDistanceSimilarity,
                    r.updatedAt = datetime()
                """,
                new
                {
                    similarities = similarities.Select(s =>
                    {
                        // Cuvamo samo jedan smer relacije.
                        // Tako ne pravimo A -> B i B -> A.
                        // CompareOrdinal - vrsi pordjenje po abecedi
                        // tako da cemo uvek da radimo sa jednom stranom
                        // i kreiramo jednu granu.
                        bool firstComesFirst =
                            string.CompareOrdinal(
                                s.FirstAsteroidId,
                                s.SecondAsteroidId) < 0;

                        return new
                        {
                            firstId = firstComesFirst
                                ? s.FirstAsteroidId
                                : s.SecondAsteroidId,

                            secondId = firstComesFirst
                                ? s.SecondAsteroidId
                                : s.FirstAsteroidId,

                            overallSimilarity = s.OverallSimilarity,
                            diameterSimilarity = s.DiameterSimilarity,
                            absoluteMagnitudeSimilarity = s.AbsoluteMagnitudeSimilarity,
                            hazardStatusSimilarity = s.HazardStatusSimilarity,
                            velocitySimilarity = s.VelocitySimilarity,
                            missDistanceSimilarity = s.MissDistanceSimilarity
                        };
                    }).ToList()
                });

            await result.ConsumeAsync();
        });
    }

    // Vraca sve asteroide iz baze, sa podacima
    // o njihovim prilazima.
    public async Task<List<AsteroidModel>> GetAllAsteroidsAsync()
    {
        await using var session = _driver.AsyncSession();

        // ExecuteReadAsync - koristimo ovo jer vrsimo samo citanje,
        // a ne upis
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var queryResult = await tx.RunAsync(
                """
                MATCH (a:Asteroid)
                OPTIONAL MATCH (a)-[:HAS_APPROACH]->(p:Approach)

                RETURN a, collect(p) AS approaches
                """);

            var records = await queryResult.ToListAsync();

            var asteroids = new List<AsteroidModel>();

            foreach (var record in records)
            {
                var asteroidNode = record["a"].As<INode>();

                var asteroid = new AsteroidModel
                {
                    Id = asteroidNode.Properties["id"].As<string>(),
                    Name = asteroidNode.Properties["name"].As<string>(),
                    DiameterMinKm = Convert.ToDouble(
                        asteroidNode.Properties["diameterMinKm"]),
                    DiameterMaxKm = Convert.ToDouble(
                        asteroidNode.Properties["diameterMaxKm"]),
                    AbsoluteMagnitude = Convert.ToDouble(
                        asteroidNode.Properties["absoluteMagnitude"]),
                    IsHazardous = Convert.ToBoolean(
                        asteroidNode.Properties["isHazardous"])
                };

                var approachNodes = record["approaches"]
                    .As<List<INode>>();

                foreach (var approachNode in approachNodes)
                {
                    // OPTIONAL MATCH moze vratiti null element
                    // ako asteroid nema nijedan prilazak.
                    if (approachNode == null)
                        continue;

                    asteroid.Approaches.Add(new ApproachModel
                    {
                        ApproachId = approachNode.Properties["approachId"]
                            .As<string>(),

                        Date = approachNode.Properties["date"]
                            .As<string>(),

                        EpochDateCloseApproach = Convert.ToInt64(
                            approachNode.Properties["epochDateCloseApproach"]),

                        MissDistance = Convert.ToDouble(
                            approachNode.Properties["missDistance"]),

                        RelativeVelocityKps = Convert.ToDouble(
                            approachNode.Properties["relativeVelocityKps"]),

                        OrbitingBody = approachNode.Properties["orbitingBody"]
                            .As<string>()
                    });
                }

                asteroids.Add(asteroid);
            }

            return asteroids;
        });

        return result;
    }

    // Vracanje jednog asteroida po id-u.
    public async Task<AsteroidModel?> GetAsteroidByIdAsync(string id)
    {
        await using var session = _driver.AsyncSession();

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var queryResult = await tx.RunAsync(
                """
                MATCH (a:Asteroid {id: $id})
                OPTIONAL MATCH (a)-[:HAS_APPROACH]->(p:Approach)

                RETURN a, collect(p) AS approaches
                """,
                new { id });

            var records = await queryResult.ToListAsync();

            // Ako asteroid ne postoji, vracamo null!
            if(records.Count == 0)
                return null;

            var record = records[0];

            // Preuzimamo podatke o asteroidu
            var asteroidNode = record["a"].As<INode>();

            // Pruzimamo sve njegove prolaske
            var approachNodes = record["approaches"].As<List<INode>>();

            var asteroid = new AsteroidModel
            {
                Id = asteroidNode.Properties["id"].As<string>(),
                Name = asteroidNode.Properties["name"].As<string>(),

                DiameterMinKm = Convert.ToDouble(
                    asteroidNode.Properties["diameterMinKm"]),

                DiameterMaxKm = Convert.ToDouble(
                    asteroidNode.Properties["diameterMaxKm"]),

                AbsoluteMagnitude = Convert.ToDouble(
                    asteroidNode.Properties["absoluteMagnitude"]),

                IsHazardous = Convert.ToBoolean(
                    asteroidNode.Properties["isHazardous"])
            };

            // Prolazimo kroz sve prilaske i dodajemo ih u listu.
            foreach (var approachNode in approachNodes)
            {
                if (approachNode == null)
                    continue;

                asteroid.Approaches.Add(new ApproachModel
                {
                    ApproachId = approachNode.Properties["approachId"]
                        .As<string>(),

                    Date = approachNode.Properties["date"]
                        .As<string>(),

                    EpochDateCloseApproach = Convert.ToInt64(
                        approachNode.Properties["epochDateCloseApproach"]),

                    MissDistance = Convert.ToDouble(
                        approachNode.Properties["missDistance"]),

                    RelativeVelocityKps = Convert.ToDouble(
                        approachNode.Properties["relativeVelocityKps"]),

                    OrbitingBody = approachNode.Properties["orbitingBody"]
                        .As<string>()
                });
            }

            return asteroid;
        });

        return result;
    }

    // Vraca numericku vrednost ili null ako ne postoji.
    private static double? GetNullableDouble(
        IReadOnlyDictionary<string, object> properties,
        string key)
    {
        if (!properties.TryGetValue(key, out var value) || value == null)
            return null;

        return Convert.ToDouble(value);
    }

    // Vraca sve asteroide koji imaju izracunatu slicnost
    // sa asteroidom, kao i same vrednosti slicnosti
    public async Task<List<AsteroidSimilarityModel>> GetSimilaritiesAsteroidAsync(string id)
    {
        await using var session = _driver.AsyncSession();

        var result = await session.ExecuteReadAsync(async tx =>
        {
           var queryResult = await tx.RunAsync(
            """
            MATCH (a:Asteroid {id: $id})-[r:SIMILAR_TO]-(b:Asteroid)

            RETURN a.id AS firstId,
                   b.id AS secondId,
                   r AS similarity
            """,
            new { id });

            var records = await queryResult.ToListAsync();

            var similarities = new List<AsteroidSimilarityModel>();

            foreach (var record in records)
            {
                var similarity = record["similarity"].As<IRelationship>();

                similarities.Add(new AsteroidSimilarityModel
                {
                    FirstAsteroidId = record["firstId"].As<string>(),
                    SecondAsteroidId = record["secondId"].As<string>(),

                    OverallSimilarity = Convert.ToDouble(
                        similarity.Properties["overallSimilarity"]),

                    DiameterSimilarity = GetNullableDouble(
                        similarity.Properties, "diameterSimilarity"),

                    AbsoluteMagnitudeSimilarity = GetNullableDouble(
                        similarity.Properties, "absoluteMagnitudeSimilarity"),

                    HazardStatusSimilarity = GetNullableDouble(
                        similarity.Properties, "hazardStatusSimilarity"),

                    VelocitySimilarity = GetNullableDouble(
                        similarity.Properties, "velocitySimilarity"),

                    MissDistanceSimilarity = GetNullableDouble(
                        similarity.Properties, "missDistanceSimilarity")
                });
            }

            return similarities;
        });

        return result;
    }

    // Vracamo sve podatke o tome koji asteroid je slican drugom
    // Odnosno sve relacije slicnosti iz baze!
    public async Task<List<AsteroidSimilarityModel>> GetAllSimilaritiesAsync()
    {
        await using var session = _driver.AsyncSession();

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var queryResult = await tx.RunAsync
            (
                """
                MATCH (a:Asteroid)-[r:SIMILAR_TO]->(b:Asteroid)

                RETURN a.id AS firstId,
                       b.id AS secondId,
                       r AS similarity
                """
            );

            var records = await queryResult.ToListAsync();

            var similarities = new List<AsteroidSimilarityModel>();

            foreach(var record in records)
            {
                // Pristupamo svojstvima
                var similarity = record["similarity"].As<IRelationship>();

                similarities.Add(new AsteroidSimilarityModel
                {
                    FirstAsteroidId = record["firstId"].As<string>(),
                    SecondAsteroidId = record["secondId"].As<string>(),

                    OverallSimilarity = Convert.ToDouble(
                        similarity.Properties["overallSimilarity"]),

                    DiameterSimilarity = GetNullableDouble(
                        similarity.Properties, "diameterSimilarity"),

                    AbsoluteMagnitudeSimilarity = GetNullableDouble(
                        similarity.Properties, "absoluteMagnitudeSimilarity"),

                    HazardStatusSimilarity = GetNullableDouble(
                        similarity.Properties, "hazardStatusSimilarity"),

                    VelocitySimilarity = GetNullableDouble(
                        similarity.Properties, "velocitySimilarity"),

                    MissDistanceSimilarity = GetNullableDouble(
                        similarity.Properties, "missDistanceSimilarity")
                });
            }

            return similarities;
        });

        return result;
    }


    // Pretraga asteroida po imenu:
    public async Task<List<AsteroidModel>> SearchAsteroidsByNameAsync(string name)
    {
        await using var session = _driver.AsyncSession();

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var queryResult = await tx.RunAsync(
                """
                    MATCH (a:Asteroid)
                    WHERE toLower(a.name) CONTAINS toLower($name)

                    OPTIONAL MATCH (a)-[HAS_APPROACH]->(p:Approach)

                    RETURN a, collect(p) as approaches
                """,
                new { name });

            var records = await queryResult.ToListAsync();

            var asteroids = new List<AsteroidModel>();

            foreach (var record in records)
            {
                var asteroidNode = record["a"].As<INode>();
                var approachNodes = record["approaches"].As<List<INode>>();

                var asteroid = new AsteroidModel
                {
                    Id = asteroidNode.Properties["id"].As<string>(),
                    Name = asteroidNode.Properties["name"].As<string>(),

                    DiameterMinKm = Convert.ToDouble(
                        asteroidNode.Properties["diameterMinKm"]),

                    DiameterMaxKm = Convert.ToDouble(
                        asteroidNode.Properties["diameterMaxKm"]),

                    AbsoluteMagnitude = Convert.ToDouble(
                        asteroidNode.Properties["absoluteMagnitude"]),

                    IsHazardous = Convert.ToBoolean(
                        asteroidNode.Properties["isHazardous"])
                };

                foreach (var approachNode in approachNodes)
                {
                    if (approachNode == null)
                        continue;

                    asteroid.Approaches.Add(new ApproachModel
                    {
                        ApproachId = approachNode.Properties["approachId"].As<string>(),
                        Date = approachNode.Properties["date"].As<string>(),

                        EpochDateCloseApproach = Convert.ToInt64(
                            approachNode.Properties["epochDateCloseApproach"]),

                        MissDistance = Convert.ToDouble(
                            approachNode.Properties["missDistance"]),

                        RelativeVelocityKps = Convert.ToDouble(
                            approachNode.Properties["relativeVelocityKps"]),

                        OrbitingBody = approachNode.Properties["orbitingBody"].As<string>()
                    });
                }

                asteroids.Add(asteroid);
            }

            return asteroids;
        });

        return result;
    }

    // Vracanje slicnih asteroida po nekom kriterijumu
    // (AsteroidSimilarityModel), za sva asteroida kazemo da su
    // slicna po nekom kriterijumu ako je slicnost >= 50 % (ili da
    // stavimo 75%)
    public async Task<List<AsteroidSimilarityModel>> GetSimilarAsteroidsAsync(string id, string criterion, double minSimilarity)
    {
        await using var session = _driver.AsyncSession();

        string property = criterion.ToLower() switch
        {
            "diameter" => "diameterSimilarity",
            "magnitude" => "absoluteMagnitudeSimilarity",
            "hazard" => "hazardStatusSimilarity",
            "velocity" => "velocitySimilarity",
            "missdistance" => "missDistanceSimilarity",
            _ => throw new ArgumentException("Nepoznat kriterijum slicnosti")
        };

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var queryResult = await tx.RunAsync(
                $$"""
                MATCH (a:Asteroid {id: $id})-[r:SIMILAR_TO]-(b:Asteroid)
                WHERE r.{{property}} IS NOT NULL
                    AND r.{{property}} >= $minSimilarity
                RETURN a.id AS firstId,
                       b.id as secondId,
                       r AS similarity
                ORDER BY r.{{property}} DESC
                """,
                new { id, minSimilarity });

            var records = await queryResult.ToListAsync();
            var similarities = new List<AsteroidSimilarityModel>();

            foreach (var record in records)
            {
                var relationship = record["similarity"].As<IRelationship>();

                similarities.Add(new AsteroidSimilarityModel
                {
                    FirstAsteroidId = record["firstId"].As<string>(),
                    SecondAsteroidId = record["secondId"].As<string>(),
                    OverallSimilarity = Convert.ToDouble(
                        relationship.Properties["overallSimilarity"]),
                    DiameterSimilarity = relationship.Properties.ContainsKey("diameterSimilarity")
                        ? Convert.ToDouble(relationship.Properties["diameterSimilarity"]) : null,
                    AbsoluteMagnitudeSimilarity = relationship.Properties.ContainsKey("absoluteMagnitudeSimilarity")
                        ? Convert.ToDouble(relationship.Properties["absoluteMagnitudeSimilarity"]) : null,
                    HazardStatusSimilarity = relationship.Properties.ContainsKey("hazardStatusSimilarity")
                        ? Convert.ToDouble(relationship.Properties["hazardStatusSimilarity"]) : null,
                    VelocitySimilarity = relationship.Properties.ContainsKey("velocitySimilarity")
                        ? Convert.ToDouble(relationship.Properties["velocitySimilarity"]) : null,
                    MissDistanceSimilarity = relationship.Properties.ContainsKey("missDistanceSimilarity")
                        ? Convert.ToDouble(relationship.Properties["missDistanceSimilarity"]) : null
                });
            }

            return similarities;
        });

        return result;
    }
}
