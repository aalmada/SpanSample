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
        public int Span_Array()
        {
            Memory<int> memory = new int[BufferSize];
            using(var handle = memory.Pin())
            {
                return Native.SumRef(MemoryMarshal.GetReference(memory.Span), (UIntPtr)memory.Length);
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

        static unsafe int SumFixed(ReadOnlySpan<int> buffer)
        {
            fixed (int* ptr = &MemoryMarshal.GetReference(buffer))
            {
                return Native.SumRef(Unsafe.AsRef<int>(ptr), (UIntPtr)buffer.Length);
            }
        }

        static int SumPin(Memory<int> buffer)
        {
            using (var handle = buffer.Pin())
            {
                return Native.SumRef(MemoryMarshal.GetReference(buffer.Span), (UIntPtr)buffer.Length);
            }
        }

        static int SumPinned(ReadOnlySpan<int> buffer)
        {
            return Native.SumRef(MemoryMarshal.GetReference(buffer), (UIntPtr)buffer.Length);
        }

        [Benchmark]
        public int MethodCall_Array_Fixed()
        {
            return SumFixed(new int[BufferSize]);
        }

        [Benchmark]
        public int MethodCall_Array_Pin()
        {
            return SumPin(new int[BufferSize]);
        }

        [Benchmark]
        public unsafe int MethodCall_StackAlloc()
        {
            ReadOnlySpan<int> buffer = stackalloc int[BufferSize];
            return SumPinned(buffer);
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
                return SumPinned(new ReadOnlySpan<int>(buffer.ToPointer(), BufferSize));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                GC.RemoveMemoryPressure(size);
            }
        }

        [Benchmark]
        public int MethodCall_NativeMemoryManager()
        {
            using (var buffer = new NativeMemoryManager<int>(BufferSize))
            {
                return SumPin(buffer.Memory);
            }
        }
    }
}
