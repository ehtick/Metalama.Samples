using System.Reflection;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Samples.Proxy;

public sealed class InterceptionMetadata(MethodInfo method, bool returnsAwaitable)
{
    public MethodInfo Method { get; } = method;
    public bool ReturnsAwaitable { get; } = returnsAwaitable;
}

[CompileTime]
internal sealed record InterceptionMetadataInfo ( IMethod Method, IField MetadataField, bool ReturnsAwaitable);
