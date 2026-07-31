using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Diagnostics;
using BenchmarkDotNet.Diagnostics.Memory;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace dotnet_realtime_pipeline.Benchmarks
{
    [MemoryDiagnoser]
    public class PipelineEventPublisherBenchmarks
    {
        [GlobalSetup]
        public void Setup()
        {
            // TODO: set up test data
        }

        [Benchmark]
        public void Benchmark_PublishEvent()
        {
            // TODO: implement benchmark
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void Benchmark_PublishEvent_Params([Params] int n)
        {
            // TODO: implement benchmark
        }

        [Benchmark]
        public async Task Benchmark_PublishEvent_Async()
        {
            // TODO: implement benchmark
        }
    }
}