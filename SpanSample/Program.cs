using BenchmarkDotNet.Running;
using System;

namespace SpanSample
{
    public class Program
    {
        static void Main(string[] args)
        {
            var switcher = new BenchmarkSwitcher(new[] {
                typeof(EnumerationBenchmarks),
                typeof(PinvokeBenchmarks),
                typeof(StringBenchmarks),
            });
            switcher.Run(args);
        }
    }
}
