namespace Metalama.Samples.Comparison1;

[ImplementEquatable]
internal partial struct EntityKey
{
    public string Type { get; }

    public int Id { get; }
}