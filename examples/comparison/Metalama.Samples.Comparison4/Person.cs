namespace Metalama.Samples.Comparison4;

public sealed partial class Person
{
    [StringEqualityMember( StringComparison.InvariantCultureIgnoreCase, true )]
    public string Name { get; init; }

    [EqualityMember]
    public DateTime DateOfBirth { get; init; }
}