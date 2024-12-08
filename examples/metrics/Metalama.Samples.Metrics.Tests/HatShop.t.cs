using System.Diagnostics;
using Metalama.Samples.Metrics;
#pragma warning disable CA2201
[assembly: GenerateAddMetricsExtension]
namespace Metalama.Samples.Metrics.Example;
public class HatShop
{
  private int _executionCount;
  [MeasureExecutionCount]
  [MeasureExecutionTime]
  [MeasureExceptionCount]
  public void PlaceOrder()
  {
    var timestamp = Stopwatch.GetTimestamp();
    try
    {
      (_hatShopMetrics?.PlaceOrderExecutionCount).Add(1);
      try
      {
    this._executionCount++;
    if (this._executionCount % 10 == 0)
    {
      throw new Exception();
    }
    else
    {
      Console.WriteLine("Ordering a hat.");
        }
      }
      catch
      {
        (_hatShopMetrics?.PlaceOrderExceptionCount).Add(1);
        throw;
      }
      return;
    }
    finally
    {
      (_hatShopMetrics?.PlaceOrderExecutionTime).Add(Stopwatch.GetTimestamp() - timestamp);
    }
  }
  private HatShopMetrics _hatShopMetrics;
  public HatShop(HatShopMetrics hatShopMetrics = null)
  {
    this._hatShopMetrics = hatShopMetrics;
  }
}