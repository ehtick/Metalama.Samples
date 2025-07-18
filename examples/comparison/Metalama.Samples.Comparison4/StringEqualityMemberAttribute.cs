using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Samples.Comparison4;

public class StringEqualityMemberAttribute : EqualityMemberAttribute
{
    private readonly StringComparison _stringComparison;
    private readonly bool _trim;

    public StringEqualityMemberAttribute( int order = DefaultOrder ) : base( order ) { }

    public StringEqualityMemberAttribute(
        StringComparison stringComparison = StringComparison.Ordinal,
        bool trim = false )
    {
        this._trim = trim;
        this._stringComparison = stringComparison;
    }

    public override void BuildEligibility( IEligibilityBuilder<IFieldOrProperty> builder )
    {
        base.BuildEligibility( builder );
        builder.Type().MustEqual( typeof(string) );
    }

    protected internal override IExpression GetComparerExpression( IFieldOrProperty field )
    {
        var comparerType =
            this._trim ? typeof(TrimmingStringEqualityComparer) : typeof(StringComparer);

        return ((INamedType) TypeFactory.GetType( comparerType ))
            .Properties[this._stringComparison.ToString()];
    }

    internal override int GetCost( IFieldOrProperty field )
    {
        // TODO: Use benchmark to determine relative costs.

        var cost = this._stringComparison switch
        {
            StringComparison.Ordinal => 5,
            StringComparison.OrdinalIgnoreCase => 10,
            _ => 50
        };

        if ( this._trim )
        {
            cost += 20;
        }

        return cost;
    }
}