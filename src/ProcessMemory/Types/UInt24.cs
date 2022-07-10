using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessMemory.Types
{
    [DebuggerDisplay("{Value,nq}")]
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 3)]
    public unsafe readonly struct UInt24
    {
#pragma warning disable IDE1006 // Naming Styles
        public const uint MinValue = unchecked(0x00000000); // 0
        public const uint MaxValue = unchecked(0x00FFFFFF); // 16777215
#pragma warning restore IDE1006 // Naming Styles

        [FieldOffset(0x00)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly byte* data = (byte*)Marshal.AllocHGlobal(3);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public uint Value => (uint)(data[0] | data[1] << 8 | data[2] << 16);

        public UInt24(byte* value, int startIndex = 0)
        {
            data[0] = value[startIndex + 2];
            data[1] = value[startIndex + 1];
            data[2] = value[startIndex];
        }

        public UInt24(uint value)
        {
            data[0] = (byte)(value & 0xFF);
            data[1] = (byte)((value >> 8) & 0xFF);
            data[2] = (byte)((value >> 16) & 0xFF);
        }

        public static implicit operator UInt24(uint v) => new UInt24(v);

        public static bool operator ==(UInt24 value1, UInt24 value2) => value1.Value == value2.Value;

        public static bool operator !=(UInt24 value1, UInt24 value2) => !(value1.Value == value2.Value);

        public bool Equals(UInt24 other) => this == other;

        public bool Equals(UInt24 x, UInt24 y) => x == y;

        public int GetHashCode(UInt24 obj) => obj.GetHashCode();

        public override bool Equals(object obj) => Value.Equals(obj);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();
        public string ToString(IFormatProvider? provider) => Value.ToString(provider);
        public string ToString(string? format) => Value.ToString(format);
        public string ToString(string? format, IFormatProvider? provider) => Value.ToString(format, provider);
        public ReadOnlySpan<byte> GetSpan() => new ReadOnlySpan<byte>(data, 3);
    }
}