namespace Metalama.Samples.Comparison1;

[GenerateEqualityComparison]
internal partial class Person
{
    public string Name { get; init; }

    public int Age { get; init; }
}

[GenerateEqualityComparison]
internal partial struct EntityKey
{
    public string Type { get; }

    public int Id { get; }
}