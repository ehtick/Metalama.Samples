using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Samples.Metrics;

[CompileTime]
internal class MetricMetadata
{
    public IMethod Method { get; }
    public MetricAttribute Aspect { get; }
    public IProperty MetricProperty { get; }

    public string MetricName { get; }
    public int Order { get; }

    public MetricMetadata(IMethod method, MetricAttribute aspect, IProperty metricProperty,
        string metricName, int order)
    {
        this.Method = method;
        this.Aspect = aspect;
        this.MetricProperty = metricProperty;
        this.MetricName = metricName;
        this.Order = order;
    }
}