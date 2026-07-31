[MemoryDiagnoser]
public class EventSubscriberBaseBenchmarks
{
    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Setup and test data
        var testList = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            testList.Add("test" + i);
        }
        // Benchmark code
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(100)] int n)
    {
        // Setup and test data
        var testDict = new Dictionary<string, int>();
        for (int i = 0; i < n; i++)
        {
            testDict.Add("test" + i, i);
        }
        // Benchmark code
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Setup and test data
        var testArray = new string[100];
        for (int i = 0; i < 100; i++)
        {
            testArray[i] = "test" + i;
        }
        // Benchmark code
    }
}