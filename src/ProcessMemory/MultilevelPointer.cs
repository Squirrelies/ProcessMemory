using ProcessMemory.Types;
using System;

namespace ProcessMemory
{
    public unsafe class MultilevelPointer
    {
        private readonly ProcessMemoryHandler memoryAccess;
        private readonly nint* baseAddress;
        private nint address;
        private readonly nint[]? offsets;

        public nint* BaseAddress => baseAddress;
        public nint Address => address;
        public bool IsNullPointer => address == 0;

        public MultilevelPointer(ProcessMemoryHandler memoryAccess, nint* baseAddress, params nint[]? offsets)
        {
            this.address = 0;
            this.memoryAccess = memoryAccess;
            this.baseAddress = baseAddress;
            this.offsets = offsets;
            UpdatePointers();
        }

        public unsafe void UpdatePointers()
        {
            if (!memoryAccess.TryGetNIntAt(baseAddress, ref address) || IsNullPointer)
                return;

            if (offsets != null)
            {
                foreach (nint offset in offsets)
                {
                    // Out of range.
                    if (!memoryAccess.TryGetNIntAt((nint*)(address + offset), ref address) || IsNullPointer)
                        return;
                }
            }
        }

        public T Deref<T>(nint offset) where T : unmanaged => (!IsNullPointer) ? this.memoryAccess.GetAt<T>((nint*)(Address + offset)) : default;
        public Span<byte> DerefSpanByte(nint offset, nuint size) => (!IsNullPointer) ? this.memoryAccess.GetSpanByteAt((nint*)(Address + offset), size) : default;
        public byte[]? DerefByteArray(nint offset, nuint size) => (!IsNullPointer) ? this.memoryAccess.GetByteArrayAt((nint*)(Address + offset), size) : default;
        public sbyte DerefSByte(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetSByteAt((nint*)(Address + offset)) : default;
        public byte DerefByte(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetByteAt((nint*)(Address + offset)) : default;
        public short DerefShort(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetShortAt((nint*)(Address + offset)) : default;
        public ushort DerefUShort(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetUShortAt((nint*)(Address + offset)) : default;
        public Int24 DerefInt24(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetInt24At((nint*)(Address + offset)) : default;
        public UInt24 DerefUInt24(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetUInt24At((nint*)(Address + offset)) : default;
        public int DerefInt(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetIntAt((nint*)(Address + offset)) : default;
        public uint DerefUInt(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetUIntAt((nint*)(Address + offset)) : default;
        public long DerefLong(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetLongAt((nint*)(Address + offset)) : default;
        public ulong DerefULong(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetULongAt((nint*)(Address + offset)) : default;
        public float DerefFloat(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetFloatAt((nint*)(Address + offset)) : default;
        public double DerefDouble(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetDoubleAt((nint*)(Address + offset)) : default;
        public string? DerefASCIIString(nint offset, nuint size) => (!IsNullPointer) ? this.memoryAccess.GetASCIIStringAt((nint*)(Address + offset), size) : default;
        public string? DerefUnicodeString(nint offset, nuint size) => (!IsNullPointer) ? this.memoryAccess.GetUnicodeStringAt((nint*)(Address + offset), size) : default;

        public bool TryDerefByteArray(nint offset, nuint size, void* result) => (!IsNullPointer && result != (void*)0) ? this.memoryAccess.TryGetByteArrayAt((nint*)(Address + offset), size, result) : false;
        public bool TryDeref<T>(nint offset, ref T result) where T : unmanaged => (!IsNullPointer) ? this.memoryAccess.TryGetAt((nint*)(Address + offset), ref result) : false;
        public bool TryDerefSByte(nint offset, ref sbyte result) => (!IsNullPointer) ? this.memoryAccess.TryGetSByteAt((nint*)(Address + offset), ref result) : false;
        public bool TryDerefByte(nint offset, ref byte result) => (!IsNullPointer) ? this.memoryAccess.TryGetByteAt((nint*)(Address + offset), ref result) : false;
        public bool TryDerefShort(nint offset, ref short result) => (!IsNullPointer) ? this.memoryAccess.TryGetShortAt((nint*)(Address + offset), ref result) : false;
        public bool TryDerefUShort(nint offset, ref ushort result) => (!IsNullPointer) ? this.memoryAccess.TryGetUShortAt((nint*)(Address + offset), ref result) : false;
        public bool TryDerefInt24(nint offset, ref Int24 result) => (!IsNullPointer) ? this.memoryAccess.TryGetInt24At((nint*)(Address + offset), ref result) : false;
        public bool TryDerefUInt24(nint offset, ref UInt24 result) => (!IsNullPointer) ? this.memoryAccess.TryGetUInt24At((nint*)(Address + offset), ref result) : false;
        public bool TryDerefInt(nint offset, ref int result) => (!IsNullPointer) ? this.memoryAccess.TryGetIntAt((nint*)(Address + offset), ref result) : false;
        public bool TryDerefUInt(nint offset, ref uint result) => (!IsNullPointer) ? this.memoryAccess.TryGetUIntAt((nint*)(Address + offset), ref result) : false;
        public bool TryDerefLong(nint offset, ref long result) => (!IsNullPointer) ? this.memoryAccess.TryGetLongAt((nint*)(Address + offset), ref result) : false;
        public bool TryDerefULong(nint offset, ref ulong result) => (!IsNullPointer) ? this.memoryAccess.TryGetULongAt((nint*)(Address + offset), ref result) : false;
        public bool TryDerefFloat(nint offset, ref float result) => (!IsNullPointer) ? this.memoryAccess.TryGetFloatAt((nint*)(Address + offset), ref result) : false;
        public bool TryDerefDouble(nint offset, ref double result) => (!IsNullPointer) ? this.memoryAccess.TryGetDoubleAt((nint*)(Address + offset), ref result) : false;
        public bool TryDerefASCIIString(nint offset, nuint size, ref string result) => (!IsNullPointer) ? this.memoryAccess.TryGetASCIIStringAt((nint*)(Address + offset), size, ref result) : false;
        public bool TryDerefUnicodeString(nint offset, nuint size, ref string result) => (!IsNullPointer) ? this.memoryAccess.TryGetUnicodeStringAt((nint*)(Address + offset), size, ref result) : false;
    }
}
