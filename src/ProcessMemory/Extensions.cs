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

        public static unsafe string FromASCIIBytes(this Memory<byte> data) => FromASCIIBytes(data.Span);
        public static unsafe string FromASCIIBytes(this ReadOnlyMemory<byte> data) => FromASCIIBytes(data.Span);
        public static unsafe string FromASCIIBytes(this byte[] data) => FromASCIIBytes((ReadOnlySpan<byte>)data);
        public static unsafe string FromASCIIBytes(this Span<byte> data) => FromASCIIBytes((ReadOnlySpan<byte>)data);
        public static unsafe string FromASCIIBytes(this ReadOnlySpan<byte> array)
        {
            fixed (byte* bp = array) // Get a byte* of the parameter passed in.
                return new string((sbyte*)bp); // Return a string using our sbyte* which points to where the data is for the byte[], Span<byte>, w/e.
        }

        public static unsafe string FromUnicodeBytes(this Memory<byte> data) => FromUnicodeBytes(data.Span);
        public static unsafe string FromUnicodeBytes(this ReadOnlyMemory<byte> data) => FromUnicodeBytes(data.Span);
        public static unsafe string FromUnicodeBytes(this byte[] data) => FromUnicodeBytes((ReadOnlySpan<byte>)data);
        public static unsafe string FromUnicodeBytes(this Span<byte> data) => FromUnicodeBytes((ReadOnlySpan<byte>)data);
        public static unsafe string FromUnicodeBytes(this ReadOnlySpan<byte> array)
        {
            fixed (byte* bp = array) // Get a byte* of the parameter passed in.
                return new string((char*)bp); // Return a string using our char* which points to where the data is for the byte[], Span<byte>, w/e.
        }

        public static ushort EndianSwap16(this ushort value) => (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        public static UInt24 EndianSwap24(this UInt24 value) => (value.Value & 0x000000FFU) << 16 | value.Value & 0x0000FF00U | (value.Value & 0x00FF0000U) >> 16;
        public static uint EndianSwap32(this uint value) => (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 | (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        public static ulong EndianSwap64(this ulong value) => (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 | (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 | (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 | (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
    }
}