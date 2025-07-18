using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Samples.Comparison3;

[assembly:
    AspectOrder(
        AspectOrderDirection.CompileTime,
        typeof(EqualityMemberAttribute),
        typeof(ImplementEquatableAttribute) )]

namespace Metalama.Samples.Comparison3;

public class EqualityMemberAttribute : FieldOrPropertyAspect
{
    public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
    {
        base.BuildAspect( builder );

        builder.With( builder.Target.DeclaringType ).RequireAspect<ImplementEquatableAttribute>();
    }

    public override void BuildEligibility( IEligibilityBuilder<IFieldOrProperty> builder )
    {
        base.BuildEligibility( builder );

        builder.MustNotBeStatic();
        builder.MustBeExplicitlyDeclared();
        builder.MustSatisfy( p => p.IsAutoPropertyOrField == true, p => $"{p} must be an automatic property" );
    }
}