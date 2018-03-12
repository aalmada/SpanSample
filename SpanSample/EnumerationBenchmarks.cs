using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SpanSample
{
    [MemoryDiagnoser]
    public class EnumerationBenchmarks
    {
        const int itemsBufferCount = 100;
        Stream stream;

        [GlobalSetup]
        public void Setup()
        {
            stream = new MemoryStream();
            var buffer = new byte[8];
            for (var counter = 0; counter < 1_000_000; counter++)
            {
                BitConverter.TryWriteBytes(buffer, (long)counter);
                stream.Write(buffer);
                BitConverter.TryWriteBytes(buffer, (double)(counter * 10.0));
                stream.Write(buffer);
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            stream.Close();
        }

        [Benchmark]
        public long Sum_Raw()
        {
            stream.Seek(0, SeekOrigin.Begin);

            var itemSize = Marshal.SizeOf<Foo>();

            Span<Foo> buffer = new Foo[itemsBufferCount]; // alloc items buffer
            var rawBuffer = buffer.NonPortableCast<Foo, byte>(); // cast items buffer to bytes buffer (no copies)

            var bytesRead = stream.Read(rawBuffer);
            var sum = 0L;
            while (bytesRead > 0)
            {
                var itemsRead = bytesRead / itemSize;
                foreach (var foo in buffer.Slice(0, itemsRead)) // iterate through the item buffer
                    sum += foo.Integer;
                bytesRead = stream.Read(rawBuffer);
            }
            return sum;
        }

        [Benchmark(Baseline = true)]
        public long Sum_RawStack()
        {
            stream.Seek(0, SeekOrigin.Begin);

            var itemSize = Marshal.SizeOf<Foo>();

            Span<Foo> buffer = stackalloc Foo[itemsBufferCount]; // alloc items buffer
            var rawBuffer = buffer.NonPortableCast<Foo, byte>(); // cast items buffer to bytes buffer (no copies)

            var bytesRead = stream.Read(rawBuffer);
            var sum = 0L;
            while (bytesRead > 0)
            {
                var itemsRead = bytesRead / itemSize;
                foreach (var foo in buffer.Slice(0, itemsRead)) // iterate through the item buffer
                    sum += foo.Integer;
                bytesRead = stream.Read(rawBuffer);
            }
            return sum;
        }

        [Benchmark]
        public long Sum_RefEnumerable()
        {
            stream.Seek(0, SeekOrigin.Begin);

            var sum = 0L;
            foreach (var foo in new RefEnumerable(stream, itemsBufferCount))
                sum += foo.Integer;
            return sum;
        }

        [Benchmark]
        public long Sum_Enumerable()
        {
            stream.Seek(0, SeekOrigin.Begin);

            var sum = 0L;
            foreach (var foo in new Enumerable(stream, itemsBufferCount))
                sum += foo.Integer;
            return sum;
        }
    }
}
