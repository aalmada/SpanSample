using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Buffers
{
    internal sealed class NativeMemoryManager<T> : MemoryManager<T>
    {
        private readonly int _length;
        private readonly int _byteLength;
        private readonly IntPtr _ptr;
        private int _retainedCount;
        private bool _disposed;

        public NativeMemoryManager(int length)
        {
            _length = length;
            _byteLength = length * Unsafe.SizeOf<T>();
            _ptr = Marshal.AllocHGlobal(_byteLength);
            GC.AddMemoryPressure(_byteLength);
        }

        ~NativeMemoryManager()
        {
            Dispose(false);
        }

        public bool IsDisposed
        {
            get
            {
                lock (this)
                {
                    return _disposed && _retainedCount == 0;
                }
            }
        }

        public override int Length => _length;

        public bool IsRetained
        {
            get
            {
                lock (this)
                {
                    return _retainedCount > 0;
                }
            }
        }

        public override unsafe Span<T> GetSpan() => 
            new Span<T>(_ptr.ToPointer(), _length);

        public override unsafe MemoryHandle Pin(int elementIndex = 0)
        {
            if (elementIndex < 0 || elementIndex >= _length)
                throw new ArgumentOutOfRangeException(nameof(elementIndex));

            lock (this)
            {
                if (_disposed && _retainedCount == 0)
                {
                    throw new ObjectDisposedException(nameof(NativeMemoryManager<T>));
                }
                _retainedCount++;
            }

            var pointer = Unsafe.Add<T>(_ptr.ToPointer(), elementIndex);    
            return new MemoryHandle(pointer, default, this);
        }

        public override void Unpin()
        {
            lock (this)
            {
                if (_retainedCount > 0)
                {
                    _retainedCount--;
                    if (!_disposed && _retainedCount == 0)
                    {
                        _disposed = true;
                        Marshal.FreeHGlobal(_ptr);
                        GC.RemoveMemoryPressure(_byteLength);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            lock (this)
            {
                if (!_disposed && _retainedCount == 0)
                {
                    _disposed = true;
                    Marshal.FreeHGlobal(_ptr);
                    GC.RemoveMemoryPressure(_byteLength);
                }
            }
        }
    }
}