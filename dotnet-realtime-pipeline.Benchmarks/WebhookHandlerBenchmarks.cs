[Benchmark]
[MemoDiagnoser]
public class WebhookHandlerBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Setup realistic test data here
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Benchmark the first public method here
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2()
    {
        // Benchmark the second public method here
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Benchmark the third public method here
    }
}
