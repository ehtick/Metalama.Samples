using System.Diagnostics.Metrics;
using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Samples.Metrics;

internal class ImplementMetricsAspect : TypeAspect
{
    [IntroduceDependency] private readonly IMeterFactory _meterFactory;
    [IntroduceDependency] private readonly IMetricHost _metricHost;

    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        // Create the Metrics nested type.
        var metricsType = builder.IntroduceClass("Metrics", whenExists: OverrideStrategy.New);

        // Introduce a dependency to the Metrics type into the target type.
        if (!builder.TryIntroduceDependency(
                new DependencyProperties(builder.Target, metricsType.Declaration, // TODO 2025.0: make nullable.
                    "metrics"), out var metricsField))
        {
            builder.SkipAspect();
            return;
        }

        // Iterate metrics and simultaneously build the Metrics type and override the instrumented methods.
        var metrics = new List<MetricMetadata>();
        foreach (var predecessor in builder.AspectInstance.Predecessors)
        {
            var precedingAspectInstance = (IAspectInstance) predecessor.Instance;
            var precedingAspect = (MetricAttribute)precedingAspectInstance.Aspect;

            var targetMethod =                (IMethod)precedingAspectInstance.TargetDeclaration.GetTarget();
            var fieldName = targetMethod.Name + precedingAspect.MetricKind;
            var metricName = targetMethod.Name + "." + precedingAspect.MetricKind;

            // Introduce a field to store the metric.
            var metricProperty = metricsType
                .IntroduceAutomaticProperty(fieldName, precedingAspect.MetricType, IntroductionScope.Instance)
                .Declaration;

            
            // Override the method.
            builder.With(targetMethod).WithTemplateProvider(precedingAspect)
                .Override(nameof(precedingAspect.OverrideMethodTemplate), args: new { metricsField, metricProperty });

            // Remember the field metadata to advice the constructor.
            metrics.Add(new MetricMetadata(targetMethod, precedingAspect, metricProperty, metricName,
                metrics.Count));
        }

        metricsType.AddInitializer(nameof(this.CreateMetrics),
            InitializerKind.BeforeInstanceConstructor, args: new { metrics });
        
        builder.Outbound.Select(x=>x.Compilation).RequireAspect<ImplementAddMetricsAspect>();
    }

    [Template]
    private void CreateMetrics(List<MetricMetadata> metrics)
    {
        var meter = this._meterFactory.Create(this._metricHost.ApplicationName,
            this._metricHost.ApplicationVersion, this._metricHost.Tags);

        foreach (var metric in metrics)
        {
            metric.Aspect.CreateMetricTemplate(ExpressionFactory.Capture(meter), metric.MetricProperty,
                metric.MetricName);

            this._metricHost.RegisterInstrument(metric.MetricProperty.Value,
                metric.Method.ToMethodInfo(), metric.Aspect.MetricKind);
        }
    }
}