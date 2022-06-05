using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
namespace ProcessMemory.Types
{
    [DebuggerDisplay("{Value,nq}")]
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 3)]
    public unsafe readonly ref struct Int24
    {
#pragma warning disable IDE1006 // Naming Styles
        public const int MinValue = unchecked((int)0xFF800000); // -8388608
        public const int MaxValue = unchecked((int)0x007FFFFF); // 8388607
#pragma warning restore IDE1006 // Naming Styles

        [FieldOffset(0x00)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Span<byte> data;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Value => data[0] | data[1] << 8 | (sbyte)data[2] << 16;

        public Int24(byte[] value, int startIndex = 0)
        {
            if (value.Length - startIndex != 3)
                throw new ArgumentOutOfRangeException();

            data = new byte[3];
            data[0] = value[startIndex + 2];
            data[1] = value[startIndex + 1];
            data[2] = value[startIndex];
        }

        public Int24(int value)
        {
            data = new byte[3];
            data[0] = (byte)(value & 0xFF);
            data[1] = (byte)((value >> 8) & 0xFF);
            data[2] = (byte)((value >> 16) & 0xFF);
        }

        public static implicit operator Int24(int v) => new Int24(v);

        public static bool operator ==(Int24 value1, Int24 value2) => value1.Value == value2.Value;

        public static bool operator !=(Int24 value1, Int24 value2) => !(value1.Value == value2.Value);

        public bool Equals(Int24 other) => this == other;

        public bool Equals(Int24 x, Int24 y) => x == y;

        public int GetHashCode(Int24 obj) => obj.GetHashCode();

        public override bool Equals(object obj) => Value.Equals(obj);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();
        public string ToString(IFormatProvider? provider) => Value.ToString(provider);
        public string ToString(string? format) => Value.ToString(format);
        public string ToString(string? format, IFormatProvider? provider) => Value.ToString(format, provider);
        public ReadOnlySpan<byte> GetSpan() => data;
    }
}
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.