using NameGenerator;

public class FishGenerator(GeneratorBase nameGenerator) : IFishGenerator
{
    private static readonly string[] _fishSpecies =
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

    public string GetNewSpecies() => _fishSpecies[this._random.Next(_fishSpecies.Length)];
}