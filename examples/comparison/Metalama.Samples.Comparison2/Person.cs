namespace Metalama.Samples.Comparison2;

[GenerateEqualityComparison]
public partial class Entity
{
    public string Name { get; init; }
}

public partial class Person : Entity
{
    public int Age { get; init; }
}