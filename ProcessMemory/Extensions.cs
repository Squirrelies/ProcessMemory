using ProcessMemory.Types;
using System;

namespace ProcessMemory
{
    public static class Extensions
    {
        public static unsafe T AsStruct<T>(this Memory<byte> data) where T : unmanaged => AsStruct<T>(data.Span);
        public static unsafe T AsStruct<T>(this ReadOnlyMemory<byte> data) where T : unmanaged => AsStruct<T>(data.Span);
        public static unsafe T AsStruct<T>(this byte[] data) where T : unmanaged => AsStruct<T>((ReadOnlySpan<byte>)data);
        public static unsafe T AsStruct<T>(this Span<byte> data) where T : unmanaged => AsStruct<T>((ReadOnlySpan<byte>)data);
        public static unsafe T AsStruct<T>(this ReadOnlySpan<byte> data) where T : unmanaged
        {
            fixed (byte* pb = &data[0])
                return *(T*)&pb;
        }

        public static ushort EndianSwap16(this ushort value) => (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        public static UInt24 EndianSwap24(this UInt24 value) => (value.Value & 0x000000FFU) << 16 | value.Value & 0x0000FF00U | (value.Value & 0x00FF0000U) >> 16;
        public static uint EndianSwap32(this uint value) => (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 | (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        public static ulong EndianSwap64(this ulong value) => (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 | (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 | (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 | (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
    }
}