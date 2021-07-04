using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessMemory.Types
{
    [DebuggerDisplay("{Value,nq}")]
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 3)]
    public unsafe readonly ref struct Int24
    {
        public const int MinValue = unchecked((int)0xFF800000); // -8388608
        public const int MaxValue = unchecked((int)0x007FFFFF); // 8388607

        [FieldOffset(0x00)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Span<byte> _data;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Value => _data[0] | _data[1] << 8 | (sbyte)_data[2] << 16;

        public Int24(byte[] value, int startIndex = 0)
        {
            if (value.Length - startIndex != 3)
                throw new ArgumentOutOfRangeException();

            _data = new byte[3];
            _data[0] = value[startIndex + 2];
            _data[1] = value[startIndex + 1];
            _data[2] = value[startIndex];
        }

        public Int24(int value)
        {
            _data = new byte[3];
            _data[0] = (byte)(value & 0xFF);
            _data[1] = (byte)((value >> 8) & 0xFF);
            _data[2] = (byte)((value >> 16) & 0xFF);
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
        public ReadOnlySpan<byte> GetSpan() => _data;
    }
}
