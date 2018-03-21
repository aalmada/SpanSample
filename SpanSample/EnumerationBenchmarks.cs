using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SpanSample
{
    [MemoryDiagnoser]
    public class EnumerationBenchmarks
    {
        Stream stream;

        [Params(1_000_000)]
        public int ItemsCount { get; set; }

        [Params(100)]
        public int ItemsBufferCount { get; set; }


        [GlobalSetup]
        public void Setup()
        {
            stream = new MemoryStream();
            var buffer = new byte[8];
            for (var counter = 0; counter < ItemsCount; counter++)
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
        public long Sum_RawForeach()
        {
            stream.Seek(0, SeekOrigin.Begin);

            var itemSize = Unsafe.SizeOf<Foo>();

            Span<Foo> buffer = new Foo[ItemsBufferCount]; // alloc items buffer
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
        public long Sum_RawEnumerator()
        {
            stream.Seek(0, SeekOrigin.Begin);

            var itemSize = Unsafe.SizeOf<Foo>();

            Span<Foo> buffer = new Foo[ItemsBufferCount]; // alloc items buffer
            var rawBuffer = buffer.NonPortableCast<Foo, byte>(); // cast items buffer to bytes buffer (no copies)

            var bytesRead = stream.Read(rawBuffer);
            var sum = 0L;
            while (bytesRead > 0)
            {
                var itemsRead = bytesRead / itemSize;
                var enumerator = buffer.Slice(0, itemsRead).GetEnumerator();
                while(enumerator.MoveNext()) // iterate through the item buffer
                    sum += enumerator.Current.Integer; // Current property return a reference (no copy)
                bytesRead = stream.Read(rawBuffer);
            }
            return sum;
        }

        [Benchmark(Baseline = true)]
        public long Sum_RawSquareBrackets()
        {
            stream.Seek(0, SeekOrigin.Begin);

            var itemSize = Unsafe.SizeOf<Foo>();

            Span<Foo> buffer = new Foo[ItemsBufferCount]; // alloc items buffer
            var rawBuffer = buffer.NonPortableCast<Foo, byte>(); // cast items buffer to bytes buffer (no copies)

            var bytesRead = stream.Read(rawBuffer);
            var sum = 0L;
            while (bytesRead > 0)
            {
                var itemsRead = bytesRead / itemSize;
                var values = buffer.Slice(0, itemsRead);
                for (var index = 0; index < itemsRead; index++) // iterate through the item buffer
                    sum += values[index].Integer; // square-brackets operator returns a reference (no copy)
                bytesRead = stream.Read(rawBuffer);
            }
            return sum;
        }

        [Benchmark]
        public long Sum_RawStackalloc()
        {
            stream.Seek(0, SeekOrigin.Begin);

            var itemSize = Unsafe.SizeOf<Foo>();

            Span<Foo> buffer = stackalloc Foo[ItemsBufferCount]; // alloc items buffer
            var rawBuffer = buffer.NonPortableCast<Foo, byte>(); // cast items buffer to bytes buffer (no copies)

            var bytesRead = stream.Read(rawBuffer);
            var sum = 0L;
            while (bytesRead > 0)
            {
                var itemsRead = bytesRead / itemSize;
                var values = buffer.Slice(0, itemsRead);
                for (var index = 0; index < itemsRead; index++) // iterate through the item buffer
                    sum += values[index].Integer;
                bytesRead = stream.Read(rawBuffer);
            }
            return sum;
        }

        [Benchmark]
        public long Sum_RefEnumerable()
        {
            stream.Seek(0, SeekOrigin.Begin);

            var sum = 0L;
            foreach (var foo in new RefEnumerable(stream, ItemsBufferCount))
                sum += foo.Integer;
            return sum;
        }

        [Benchmark]
        public long Sum_Enumerable()
        {
            stream.Seek(0, SeekOrigin.Begin);

            var sum = 0L;
            foreach (var foo in new Enumerable(stream, ItemsBufferCount))
                sum += foo.Integer;
            return sum;
        }

        [Benchmark]
        public long Sum_NativeEnumerable()
        {
            var sum = 0L;
            foreach (var foo in new NativeEnumerable(ItemsCount, ItemsBufferCount))
                sum += foo;
            return sum;
        }
    }
}
