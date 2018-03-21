using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpanSample
{
    [MemoryDiagnoser]
    public class PinvokeBenchmarks
    {
        [Params(100, 1_000)]
        public int BufferSize { get; set; }

        ////////////////////////////////////////////////////////////////////////

        [Benchmark]
        public unsafe int Array()
        {
            var buffer = new int[BufferSize]; 
            fixed(int* ptr = &buffer[0])
            {
                return Native.Sum(ptr, (UIntPtr)BufferSize);
            }
        }

        [Benchmark]
        public unsafe int StackAlloc()
        {
            var buffer = stackalloc int[BufferSize];
            return Native.Sum(buffer, (UIntPtr)BufferSize);
        }


        [Benchmark]
        public unsafe int HGlobal()
        {
            var size = BufferSize * Unsafe.SizeOf<int>();
            var buffer = IntPtr.Zero;
            try
            {
                buffer = Marshal.AllocHGlobal(size);
                GC.AddMemoryPressure(size);
                return Native.Sum((int*)buffer.ToPointer(), (UIntPtr)BufferSize);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                GC.RemoveMemoryPressure(size);
            }
        }

        ////////////////////////////////////////////////////////////////////////

        [Benchmark]
        public unsafe int Span_Array()
        {
            ReadOnlySpan<int> span = new int[BufferSize];
            fixed (int* ptr = &MemoryMarshal.GetReference(span))
            {
                return Native.SumRef(Unsafe.AsRef<int>(ptr), (UIntPtr)span.Length);
            }
        }

        [Benchmark]
        public int Span_StackAlloc()
        {
            ReadOnlySpan<int> span = stackalloc int[BufferSize];
            return Native.SumRef(MemoryMarshal.GetReference(span), (UIntPtr)span.Length);
        }

        [Benchmark]
        public unsafe int Span_HGlobal()
        {
            var size = BufferSize * Unsafe.SizeOf<int>();
            var buffer = IntPtr.Zero;
            try
            {
                buffer = Marshal.AllocHGlobal(size);
                GC.AddMemoryPressure(size);
                ReadOnlySpan<int> span = new ReadOnlySpan<int>(buffer.ToPointer(), BufferSize);
                return Native.SumRef(MemoryMarshal.GetReference(span), (UIntPtr)span.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                GC.RemoveMemoryPressure(size);
            }
        }

        ////////////////////////////////////////////////////////////////////////

        static unsafe int Sum(ReadOnlySpan<int> span)
        {
            fixed (int* ptr = &MemoryMarshal.GetReference(span))
            {
                return Native.SumRef(Unsafe.AsRef<int>(ptr), (UIntPtr)span.Length);
            }
        }

        [Benchmark]
        public int MethodCall_Array()
        {
            return Sum(new int[BufferSize]);
        }

        [Benchmark]
        public int MethodCall_StackAlloc()
        {
            ReadOnlySpan<int> span = stackalloc int[BufferSize];
            return Sum(span);
        }

        [Benchmark]
        public unsafe int MethodCall_HGlobal()
        {
            var size = BufferSize * Unsafe.SizeOf<int>();
            var buffer = IntPtr.Zero;
            try
            {
                buffer = Marshal.AllocHGlobal(size);
                GC.AddMemoryPressure(size);
                return Sum(new ReadOnlySpan<int>(buffer.ToPointer(), BufferSize));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                GC.RemoveMemoryPressure(size);
            }
        }

        [Benchmark]
        public int MethodCall_NativeOwnedMemory()
        {
            using (var buffer = new NativeOwnedMemory<int>(BufferSize))
            {
                return Sum(buffer.Span);
            }
        }
    }
}
