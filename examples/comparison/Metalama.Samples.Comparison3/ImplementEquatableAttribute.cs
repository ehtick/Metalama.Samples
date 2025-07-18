using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;

namespace Metalama.Samples.Comparison3;

[Inheritable]
public class ImplementEquatableAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        // Identify the field and automatic properties that might be part of the comparison, look for custom attributes.
        var targetType = builder.Target;

        var fields = targetType.FieldsAndProperties.Where( f =>
                                                               f.IsAutoPropertyOrField == true
                                                               && f is { IsStatic: false, IsImplicitlyDeclared: false }
                                                               && f.Attributes.Any( typeof(EqualityMemberAttribute) ) )
            .ToList();

        // If there are no members, do not implement the aspect.
        if ( fields.Count == 0 )
        {
            // Write an error unless the aspect was applied through inheritance.
            if ( builder.AspectInstance.Predecessors[0].Kind != AspectPredecessorKind.Inherited )
            {
                builder.Diagnostics.Report( DiagnosticDefinitions.NoEqualityMemberError.WithArguments( targetType ) );
            }

            builder.SkipAspect();

            return;
        }

        // Find the base Equals method.
        var ancestors = new List<INamedType>();

        for ( var parent = builder.Target.BaseType;
              parent != null && parent.SpecialType != SpecialType.Object;
              parent = parent.BaseType )
        {
            ancestors.Add( parent );
        }

        var baseEqualsMethod = targetType.AllMethods
            .OfName( "Equals" )
            .Where( m => m.Parameters.Count == 1 && m.Parameters[0].Type is INamedType )
            .Select( m => (Method: m, m.Parameters[0].Type,
                           Level: ancestors.IndexOf( (INamedType) m.Parameters[0].Type )) )
            .Where( m => m.Level >= 0 )
            .OrderBy( m => m.Level )
            .FirstOrDefault();

        // Add the IEquatable interface to the type (interface members will be added lower).
        builder.ImplementInterface(
            ((INamedType) TypeFactory.GetType( typeof(IEquatable<>) )).WithTypeArguments( targetType ) );

        // Introduce the Equals methods.
        builder.IntroduceMethod(
            nameof(this.TypedEqualsTemplate),
            args: new
            {
                TBase = baseEqualsMethod.Type ?? TypeFactory.GetType( SpecialType.Object ),
                TDerived = targetType,
                fields,
                baseEqualsMethod = baseEqualsMethod.Method
            },
            buildMethod: m => m.IsVirtual = !targetType.IsSealed );

        builder.IntroduceMethod(
            nameof(this.UntypedEqualsTemplate),
            whenExists: OverrideStrategy.Override,
            args: new { T = targetType, fields } );

        if ( baseEqualsMethod.Method != null )
        {
            builder.IntroduceMethod(
                nameof(this.BaseTypeEqualsTemplate),
                whenExists: OverrideStrategy.Override,
                args: new { TBase = baseEqualsMethod.Type, TDerived = targetType } );
        }

        // Introduce the GetHashCode method.
        builder.IntroduceMethod(
            nameof(this.AddToHashCode),
            whenExists: OverrideStrategy.Override,
            args: new { T = targetType, fields },
            buildMethod: m => m.IsVirtual = !targetType.IsSealed );

        builder.IntroduceMethod(
            nameof(this.GetHashCodeTemplate),
            whenExists: OverrideStrategy.Override,
            args: new { T = targetType, fields } );

        // Introduce the operators.
        builder.IntroduceBinaryOperator(
            nameof(this.EqualityOperatorTemplate),
            targetType,
            targetType,
            TypeFactory.GetType( typeof(bool) ),
            OperatorKind.Equality,
            args: new { T = targetType } );

        builder.IntroduceBinaryOperator(
            nameof(this.InequalityOperatorTemplate),
            targetType,
            targetType,
            TypeFactory.GetType( typeof(bool) ),
            OperatorKind.Inequality,
            args: new { T = targetType } );
    }

    // Template for the top-level Equals(T) method.
    [Template( Name = "Equals" )]
    public bool TypedEqualsTemplate<[CompileTime] TBase, [CompileTime] TDerived>(
        TDerived? other,
        IReadOnlyList<IFieldOrProperty> fields,
        IMethod? baseEqualsMethod )
        where TDerived : TBase
    {
        // The following `if` is evaluated at compile time, so the block is only
        // emitted for reference types.
        if ( meta.Target.Type.IsReferenceType == true )
        {
            if ( other == null )
            {
                return false;
            }

            if ( ReferenceEquals( meta.This, other ) )
            {
                return true;
            }
        }

        // Call the base strongly-typed Equals method, which typically has a parameter of the base type, but
        // is overridden in the current type by the BaseTypeEqualsTemplate template.
        if ( baseEqualsMethod != null )
        {
            if ( !baseEqualsMethod.With( InvokerOptions.Base ).Invoke( other ) )
            {
                return false;
            }
        }

        foreach ( var field in fields )
        {
            var defaultComparer = ((INamedType) TypeFactory.GetType( typeof(EqualityComparer<>) ))
                .WithTypeArguments( field.Type )
                .Properties["Default"];

            if ( !defaultComparer.Value!.Equals( field.Value, field.With( other ).Value ) )
            {
                return false;
            }
        }

        return true;
    }

    // Templates for the new method hiding the base typed Equals method.
    [Template( Name = "Equals" )]
    public bool BaseTypeEqualsTemplate<[CompileTime] TBase, [CompileTime] TDerived>( TBase? other )
    {
        // If we have a reference type, first check for reference equality because this is very fast.
        if ( meta.Target.Type.IsReferenceType == true )
        {
            if ( ReferenceEquals( meta.This, other ) )
            {
                return true;
            }
        }

        return (other is TDerived typed && meta.This.Equals( typed ));
    }

    // Template for the Equals(object) method.
    [Template( Name = "Equals" )]
    public bool UntypedEqualsTemplate<[CompileTime] T>( object? other )
    {
        // If we have a reference type, first check for reference equality because this is very fast.
        if ( meta.Target.Type.IsReferenceType == true )
        {
            if ( ReferenceEquals( meta.This, other ) )
            {
                return true;
            }
        }

        return other is T typed && meta.This.Equals( typed );
    }

    // Template for AddToHashCode.
    [Template]
    protected void AddToHashCode( ref HashCode hashCode, IReadOnlyList<IFieldOrProperty> fields )
    {
        // Call the base method, or ignore if there is none.
        if ( meta.Target.Method.OverriddenMethod != null )
        {
            meta.Proceed();
        }

        foreach ( var field in fields )
        {
            var defaultComparer = ((INamedType) TypeFactory.GetType( typeof(EqualityComparer<>) ))
                .WithTypeArguments( field.Type )
                .Properties["Default"];

            hashCode.Add( field.Value, defaultComparer.Value );
        }
    }

    // Template for the GetHashCode method.
    [Template( Name = "GetHashCode" )]
    public int GetHashCodeTemplate( IReadOnlyList<IFieldOrProperty> fields )
    {
        var hashCode = default(HashCode);

        meta.This.AddToHashCode( ref hashCode );

        return hashCode.ToHashCode();
    }

   
    // Template for the == operator.
    [Template]
    public bool EqualityOperatorTemplate<[CompileTime] T>( T a, T b )
    {
        if ( meta.Target.Type.IsReferenceType == true )
        {
            return (a == null && b == null) || (a != null && a.Equals( b ));
        }
        else
        {
            return a!.Equals( b );
        }
    }

    // Template for the != operator.
    [Template]
    public bool InequalityOperatorTemplate<[CompileTime] T>( T a, T b )
    {
        if ( meta.Target.Type.IsReferenceType == true )
        {
            return ((a == null) ^ (b == null)) || (a != null && !a.Equals( b ));
        }
        else
        {
            return !a!.Equals( b );
        }
    }
}