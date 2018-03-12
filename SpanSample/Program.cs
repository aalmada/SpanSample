using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SpanSample
{
    public class Program
    {
        static void Main(string[] args)
        {
#if RELEASE
            var summary = BenchmarkRunner.Run<EnumerationBenchmarks>();
#else
            using (var stream = new MemoryStream())
            {
                var buffer = new byte[8];
                for (var counter = 0; counter < 55; counter++)
                {
                    BitConverter.TryWriteBytes(buffer, (long)counter);
                    stream.Write(buffer);
                    BitConverter.TryWriteBytes(buffer, (double)(counter * 10.0));
                    stream.Write(buffer);
                }

                stream.Seek(0, SeekOrigin.Begin);
                foreach (var foo in new Enumerable(stream, 10))
                    Console.WriteLine($"Integer: {foo.Integer} Double: {foo.Double}");

            }
            Console.ReadLine();
#endif
        }


    }
}
