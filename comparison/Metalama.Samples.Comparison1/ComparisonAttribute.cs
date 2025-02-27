using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Samples.Comparison1;

public class ComparisonAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        builder.ImplementInterface( ((INamedType) TypeFactory.GetType( typeof(IEquatable<>) )).WithTypeArguments( builder.Target ) );

        builder.IntroduceMethod( nameof(IntroducedEquals), args: new { T = builder.Target } );
    }

    [Template( Name = "Equals" )]
    public bool IntroducedEquals<[CompileTime] T>( T? other )
    {
        foreach ( var member in meta.Target.Type.AllFieldsAndProperties )
        {
            if ( member.IsStatic || member.IsImplicitlyDeclared )
            {
                continue;
            }

            var defaultComparer = ((INamedType) TypeFactory.GetType( typeof(EqualityComparer<>) ))
                .WithTypeArguments( member.Type )
                .Properties["Default"];

            if ( !defaultComparer.Value!.Equals( member.Value, member.With( other ).Value ) )
            {
                return false;
            }
        }

        return true;
    }
}