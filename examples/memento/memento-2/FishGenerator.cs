using NameGenerator;

public class FishGenerator(GeneratorBase nameGenerator) : IFishGenerator
{
    private static readonly string[] FishSpecies =
    [
        "Clownfish",
        "Damselfish",
        "Dottyback",
        "Fairy Basslet",
        "Goby",
        "Hawkfish",
        "Jawfish",
        "Lionfish",
        "Mandarin Dragonet",
        "Neon Goby",
        "Pseudochromis",
        "Royal Gramma",
        "Tang",
        "Wrasse",
        "Scuba Diver"
    ];

    private readonly Random _random = new();

    public string GetNewName() => nameGenerator.Generate();

    public string GetNewSpecies() => FishSpecies[this._random.Next(FishSpecies.Length)];
}