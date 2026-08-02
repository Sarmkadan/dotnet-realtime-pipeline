[MemoryDiagnoser]
public class DeadLetterQueueBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // setup test data
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // benchmark code
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2(int inputSize)
    {
        // benchmark code
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // benchmark code
    }
}