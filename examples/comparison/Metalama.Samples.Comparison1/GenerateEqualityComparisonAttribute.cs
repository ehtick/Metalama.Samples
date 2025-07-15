using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;

namespace Metalama.Samples.Comparison1;

public class GenerateEqualityComparisonAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        // Identify the field and automatic properties that will be part of the comparison.
        var targetType = builder.Target;
        var fields = targetType.FieldsAndProperties.Where( f =>
            f.IsAutoPropertyOrField == true && f is { IsStatic: false, IsImplicitlyDeclared: false } )
            .OrderBy( f => f.Name )
            .ToList();

        // Add the IEquatable interface to the type (members will be added lower).
        builder.ImplementInterface( ((INamedType) TypeFactory.GetType( typeof( IEquatable<> ) )).WithTypeArguments( targetType ) );


        // Introduce the Equals methods.
        builder.IntroduceMethod( nameof( this.IntroducedTypedEquals ), args: new { T = targetType, fields } );
        builder.IntroduceMethod( nameof( this.IntroducedUntypedEquals ), whenExists: OverrideStrategy.Override, args: new { T = targetType, fields } );

        // Introduce the GetHashCode method.
        builder.IntroduceMethod( nameof( this.IntroducedGetHashCode ), whenExists: OverrideStrategy.Override, args: new { T = targetType, fields } );

        // Introduce the operators.
        builder.IntroduceBinaryOperator( nameof( this.IntroducedEqualityOperator ), targetType, targetType,
            TypeFactory.GetType( typeof( bool ) ), OperatorKind.Equality, args: new { T = targetType } );
        builder.IntroduceBinaryOperator( nameof( this.IntroducedInequalityOperator ), targetType, targetType,
            TypeFactory.GetType( typeof( bool ) ), OperatorKind.Inequality, args: new { T = targetType } );
    }



    // Template for the Equals(T) method.
    [Template( Name = "Equals" )]
    public bool IntroducedTypedEquals<[CompileTime] T>( T? other, IReadOnlyList<IFieldOrProperty> fields )
    {
        foreach ( var field in fields )
        {
            var defaultComparer = ((INamedType) TypeFactory.GetType( typeof( EqualityComparer<> ) ))
                .WithTypeArguments( field.Type )
                .Properties["Default"];

            if ( !defaultComparer.Value!.Equals( field.Value, field.With( other ).Value ) )
            {
                return false;
            }
        }

        return true;
    }

    // Template for the Equals(object) method.
    [Template( Name = "Equals" )]
    public bool IntroducedUntypedEquals<[CompileTime] T>( object? other )
        => other is T typed && meta.This.Equals( typed );

    // Template for the GetHashCode method.
    [Template( Name = "GetHashCode" )]
    public int IntroducedGetHashCode( IReadOnlyList<IFieldOrProperty> fields )
    {
        var hashCode = default( HashCode );

        foreach ( var field in fields )
        {
            var defaultComparer = ((INamedType) TypeFactory.GetType( typeof( EqualityComparer<> ) ))
                .WithTypeArguments( field.Type )
                .Properties["Default"];

            hashCode.Add( field.Value, defaultComparer.Value );
        }

        return hashCode.ToHashCode();
    }

    // Template for the == operator.
    [Template]
    public bool IntroducedEqualityOperator<[CompileTime] T>( T a, T b )
       => (a == null && b == null) || (a != null && a.Equals( b ));

    // Template for the != operator.
    [Template]
    public bool IntroducedInequalityOperator<[CompileTime] T>( T a, T b )
        => ((a == null) ^ (b == null)) || (a != null && !a.Equals( b ));

}



