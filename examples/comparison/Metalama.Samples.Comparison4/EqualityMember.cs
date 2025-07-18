using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Samples.Comparison4;

[CompileTime]
public record EqualityMember( IFieldOrProperty Field, EqualityMemberAttribute Aspect );