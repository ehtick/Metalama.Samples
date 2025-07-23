namespace Metalama.Samples.Comparison3;

public partial class Entity
{
    [EqualityMember]
    public int Id { get; init; }

    public Guid ObjectId { get; } = Guid.NewGuid();
}

public sealed partial class Person : Entity
{
    public string Name { get; init; }

    public int Age { get; init; }
}

public class VersionedEntity : Entity
{
    [EqualityMember]
    public int Version { get; init; }
}