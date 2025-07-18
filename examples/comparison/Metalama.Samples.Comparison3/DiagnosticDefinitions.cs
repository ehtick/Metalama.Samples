using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Samples.Comparison3;

[CompileTime]
internal static class DiagnosticDefinitions
{
    public static readonly DiagnosticDefinition<INamedType>
        NoEqualityMemberError
            = new(
                "EQU001",
                Severity.Error,
                "The type '{0}' does not have any field or property annotated with the [EqualityMember] attribute." );
}