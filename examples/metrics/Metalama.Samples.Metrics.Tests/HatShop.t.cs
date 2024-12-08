using System.Diagnostics;
using Metalama.Samples.Metrics;
[assembly: GenerateAddMetricsExtensionAttribute]
namespace Metalama.Samples.Metrics.Example;
public class HatShop
{
  private int _executionCount;
  [MeasureExecutionCount, MeasureExecutionTime, MeasureExceptionCount]
  public void PlaceOrder()
  {
    var timestamp = Stopwatch.GetTimestamp();
    try
    {
      (hatShopMetrics?.PlaceOrderExecutionCount).Add(1);
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
        (hatShopMetrics?.PlaceOrderExceptionCount).Add(1);
        throw;
      }
      return;
    }
    finally
    {
      (hatShopMetrics?.PlaceOrderExecutionTime).Add(Stopwatch.GetTimestamp() - timestamp);
    }
  }
  private HatShopMetrics hatShopMetrics;
  public HatShop(HatShopMetrics hatShopMetrics = null)
  {
    this.hatShopMetrics = hatShopMetrics;
  }
}