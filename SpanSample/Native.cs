using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SpanSample
{
    [SuppressUnmanagedCodeSecurity]
    static class Native
    {
        const string LibName = "Native";

        [DllImport(LibName, EntryPoint = "sum")]
        public static extern unsafe int Sum(int* buffer, UIntPtr size);

        [DllImport(LibName, EntryPoint = "sum")]
        public static extern int SumRef(in int buffer, UIntPtr size);

        [DllImport(LibName, EntryPoint = "stream_new")]
        public static extern IntPtr StreamNew(UIntPtr size);

        [DllImport(LibName, EntryPoint = "stream_delete")]
        public static extern void StreamDelete(IntPtr stream);

        [DllImport(LibName, EntryPoint = "stream_getsize")]
        public static extern UIntPtr StreamGetSize(NativeStreamHandle stream);

        [DllImport(LibName, EntryPoint = "stream_getposition")]
        public static extern UIntPtr StreamGetPosition(NativeStreamHandle stream);

        [DllImport(LibName, EntryPoint = "stream_read")]
        public static extern UIntPtr StreamRead(NativeStreamHandle stream, ref int buffer, UIntPtr size);

        [DllImport(LibName, EntryPoint = "stream_reset")]
        public static extern bool StreamReset(NativeStreamHandle stream);
    }
}
