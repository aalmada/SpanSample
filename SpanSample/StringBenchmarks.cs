using BenchmarkDotNet.Attributes;
using System;

namespace SpanSample
{
    [MemoryDiagnoser]
    public class StringBenchmarks
    {
        public string String { get; set; }

        [Params(100)]
        public int Start { get; set; }

        [Params(10, 100, 1_000)]
        public int Length { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            this.String = new string('a', 2_000);
        }

        ////////////////////////////////////////////////////////////////////////

        [Benchmark]
        public string Substring()
        {
            return this.String.Substring(this.Start, this.Length);
        }

        [Benchmark(Baseline = true)]
        public ReadOnlySpan<char> Slice()
        {
            return this.String.AsReadOnlySpan(this.Start, this.Length);
        }
    }
}
