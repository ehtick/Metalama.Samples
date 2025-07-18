namespace Metalama.Samples.Comparison2;

[GenerateEqualityComparison]
public partial class Entity
{
    public string Name { get; init; }
}

public sealed partial class Person : Entity
{
    public int Age { get; init; }
}