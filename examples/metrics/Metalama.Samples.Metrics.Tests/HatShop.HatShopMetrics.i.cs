using System.Diagnostics.Metrics;
using System.Reflection;
using Metalama.Samples.Metrics;
using Metalama.Samples.Metrics.Example;
namespace Metrics
{
  public class HatShopMetrics
  {
    internal readonly Counter<long> PlaceOrderExceptionCount;
    internal readonly Counter<long> PlaceOrderExecutionCount;
    internal readonly Counter<long> PlaceOrderExecutionTime;
    private IMeterFactory meterFactory;
    private IMetricHost metricHost;
    public HatShopMetrics(IMeterFactory? meterFactory = null, IMetricHost? metricHost = null)
    {
      this.meterFactory = meterFactory ?? throw new System.ArgumentNullException(nameof(meterFactory));
      this.metricHost = metricHost ?? throw new System.ArgumentNullException(nameof(metricHost));
      var meter = this.meterFactory.Create(this.metricHost.ApplicationName, this.metricHost.ApplicationVersion, this.metricHost.Tags);
      PlaceOrderExceptionCount = meter.CreateCounter<long>("PlaceOrder.ExceptionCount");
      this.metricHost.RegisterInstrument(PlaceOrderExceptionCount, typeof(HatShop).GetMethod("PlaceOrder", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null), "ExceptionCount");
      PlaceOrderExecutionCount = meter.CreateCounter<long>("PlaceOrder.ExecutionCount");
      this.metricHost.RegisterInstrument(PlaceOrderExecutionCount, typeof(HatShop).GetMethod("PlaceOrder", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null), "ExecutionCount");
      PlaceOrderExecutionTime = meter.CreateCounter<long>("PlaceOrder.ExecutionTime");
      this.metricHost.RegisterInstrument(PlaceOrderExecutionTime, typeof(HatShop).GetMethod("PlaceOrder", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null), "ExecutionTime");
    }
  }
}