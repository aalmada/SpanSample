using System.Runtime.InteropServices;

namespace ConsoleApp3
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Foo
    {
        public readonly long Integer;
        public readonly double Double;

        public Foo(int integer, double @double)
        {
            this.Integer = integer;
            this.Double = @double;
        }
    }
}
