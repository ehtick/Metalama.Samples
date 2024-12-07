namespace Metalama.Samples.Metrics.Example;

public class HatShop
{
    private int _executionCount;
    
    [MeasureExecutionCount]
    public void PlaceOrder()
    {
        _executionCount++;

        if (_executionCount % 10 == 0)
        {
            throw new Exception();
        }
        else
        {
            Console.WriteLine("Ordering a hat.");
        }
    }
}