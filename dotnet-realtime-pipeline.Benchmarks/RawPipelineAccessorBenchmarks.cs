[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 1000)]
[MemoryDiagnoser]
public class RawPipelineAccessorBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Set up realistic test data here
    }

    [Benchmark]
    public void Benchmark_RawPipelineAccessor_GetData()
    {
        // Benchmark getting data from RawPipelineAccessor
        var accessor = new RawPipelineAccessor();
        var data = accessor.GetData();
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_RawPipelineAccessor_GetDataWithParams()
    {
        // Benchmark getting data from RawPipelineAccessor with input size
        var accessor = new RawPipelineAccessor();
        for (int i = 10; i <= 1000; i *= 10)
        {
            var data = accessor.GetData(i);
        }
    }

    [Benchmark]
    public void Benchmark_RawPipelineAccessor_GetDataWithComplexParams()
    {
        // Benchmark getting data from RawPipelineAccessor with complex input
        var accessor = new RawPipelineAccessor();
        var complexData = new Dictionary<string, string>();
        complexData.Add("key1", "value1");
        complexData.Add("key2", "value2");
        var data = accessor.GetData(complexData);
    }
}