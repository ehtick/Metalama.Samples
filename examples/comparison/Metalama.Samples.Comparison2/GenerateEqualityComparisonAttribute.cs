
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System.Diagnostics;

namespace Metalama.Samples.Comparison2;

[Inheritable]
public class GenerateEqualityComparisonAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        var targetType = builder.Target;
        builder.ImplementInterface( ((INamedType) TypeFactory.GetType( typeof( IEquatable<> ) )).WithTypeArguments( targetType ) );

        var fields = targetType.FieldsAndProperties.Where( f =>
            f.IsAutoPropertyOrField == true && f is { IsStatic: false, IsImplicitlyDeclared: false } )
            .OrderBy( f => f.Name )
            .ToList();

        // Find the base Equals method.
        Debugger.Break();
        var ancestors = new List<INamedType>();
        for ( var parent = builder.Target.BaseType; parent.SpecialType != SpecialType.Object; parent = parent.BaseType )
        {
            ancestors.Add( parent );
        }

        var baseEqualsMethod = targetType.AllMethods
            .OfName( "Equals" )
            .Where( m => m.Parameters.Count == 1 && m.Parameters[0].Type is INamedType )
            .Select( m => (Method: m, Type: m.Parameters[0].Type, Level: ancestors.IndexOf( (INamedType) m.Parameters[0].Type )) )
            .Where( m => m.Level >= 0 )
            .OrderBy( m => m.Level )
            .FirstOrDefault();



        // Introduce the Equals methods.
        builder.IntroduceMethod( nameof( this.IntroducedTypedEquals ), 
            args: new { TBase = baseEqualsMethod.Type ?? TypeFactory.GetType( SpecialType.Object  ), TDerived = targetType, fields, baseEqualsMethod = baseEqualsMethod.Method } );
        builder.IntroduceMethod( nameof( this.IntroducedUntypedEquals ), whenExists: OverrideStrategy.Override, args: new { T = targetType, fields } );

        if ( baseEqualsMethod.Method != null )
        {
            builder.IntroduceMethod( nameof( this.IntroducedBaseTypeEquals ),
                whenExists: OverrideStrategy.Override,
                args: new { TBase = baseEqualsMethod.Type, TDerived = targetType } );
        }

        // Introduce the GetHashCode method.
        builder.IntroduceMethod( nameof( this.AddToHashCode ), whenExists: OverrideStrategy.Override, args: new { T = targetType, fields } );
        builder.IntroduceMethod( nameof( this.IntroducedGetHashCode ), whenExists: OverrideStrategy.Override, args: new { T = targetType, fields } );

        // Introduce the operators.
        builder.IntroduceBinaryOperator( nameof( this.IntroducedEqualityOperator ), targetType, targetType,
            TypeFactory.GetType( typeof( bool ) ), OperatorKind.Equality, args: new { T = targetType } );
        builder.IntroduceBinaryOperator( nameof( this.IntroducedInequalityOperator ), targetType, targetType,
            TypeFactory.GetType( typeof( bool ) ), OperatorKind.Inequality, args: new { T = targetType } );
    }



    // Template for the Equals(T) method.
    [Template( Name = "Equals" )]
    public virtual bool IntroducedTypedEquals<[CompileTime] TBase, [CompileTime] TDerived>( TDerived? other, IReadOnlyList<IFieldOrProperty> fields, IMethod? baseEqualsMethod )
        where TDerived : TBase
    {
        // Call the base strongly-typed Equals method, which typically has a parameter of the base type, but
        // is overridden in the current type by the IntroducedBaseTypeEquals template.
        if ( baseEqualsMethod != null )
        {
            meta.InsertComment( $"Invoking {baseEqualsMethod}" );
            
            if ( !baseEqualsMethod.Invoke( other  ) )
            {
                return false;
            }
        }

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

    [Template( Name = "Equals" )]
    public bool IntroducedBaseTypeEquals<[CompileTime] TBase, [CompileTime] TDerived>( TBase? other )
        => other is TDerived typed && meta.This.Equals( typed );

    // Template for the Equals(object) method.
    [Template( Name = "Equals" )]
    public bool IntroducedUntypedEquals<[CompileTime] T>( object? other )
        => other is T typed && meta.This.Equals( typed );

    [Template]
    protected virtual void AddToHashCode( ref HashCode hashCode, IReadOnlyList<IFieldOrProperty> fields )
    {
        // Call the base method, or ignore if there is none.
        if ( meta.Target.Method.OverriddenMethod != null )
        {
            meta.Proceed();
        }

        foreach ( var field in fields )
        {
            var defaultComparer = ((INamedType) TypeFactory.GetType( typeof( EqualityComparer<> ) ))
                .WithTypeArguments( field.Type )
                .Properties["Default"];

            hashCode.Add( field.Value, defaultComparer.Value );
        }
    }

    // Template for the GetHashCode method.
    [Template( Name = "GetHashCode" )]
    public int IntroducedGetHashCode( IReadOnlyList<IFieldOrProperty> fields )
    {
        var hashCode = default( HashCode );

        meta.This.AddToHashCode( ref hashCode );

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