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
        public nint* Address => (nint*)address;
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
            if (!memoryAccess.TryGetNIntAt(baseAddress, ref address) || address == 0)
                return;

            if (offsets != null)
            {
                foreach (nint offset in offsets)
                {
                    // Out of range.
                    if (!memoryAccess.TryGetNIntAt((nint*)(address + offset), ref address) || address == 0)
                        return;
                }
            }
        }

        public T Deref<T>(nint offset) where T : unmanaged => (!IsNullPointer) ? this.memoryAccess.GetAt<T>(Address + offset) : default;
        public Span<byte> DerefSpanByte(nint offset, nuint size) => (!IsNullPointer) ? this.memoryAccess.GetSpanByteAt(Address + offset, size) : default;
        public byte[]? DerefByteArray(nint offset, nuint size) => (!IsNullPointer) ? this.memoryAccess.GetByteArrayAt(Address + offset, size) : default;
        public sbyte DerefSByte(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetSByteAt(Address + offset) : default;
        public byte DerefByte(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetByteAt(Address + offset) : default;
        public short DerefShort(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetShortAt(Address + offset) : default;
        public ushort DerefUShort(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetUShortAt(Address + offset) : default;
        //public Int24 DerefInt24(nint offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetInt24At(Address + offset) : default;
        //public UInt24 DerefUInt24(nint offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetUInt24At(Address + offset) : default;
        public int DerefInt(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetIntAt(Address + offset) : default;
        public uint DerefUInt(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetUIntAt(Address + offset) : default;
        public long DerefLong(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetLongAt(Address + offset) : default;
        public ulong DerefULong(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetULongAt(Address + offset) : default;
        public float DerefFloat(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetFloatAt(Address + offset) : default;
        public double DerefDouble(nint offset) => (!IsNullPointer) ? this.memoryAccess.GetDoubleAt(Address + offset) : default;
        public string? DerefASCIIString(nint offset, nuint size) => (!IsNullPointer) ? this.memoryAccess.GetASCIIStringAt(Address + offset, size) : default;
        public string? DerefUnicodeString(nint offset, nuint size) => (!IsNullPointer) ? this.memoryAccess.GetUnicodeStringAt(Address + offset, size) : default;

        public bool TryDerefByteArray(nint offset, nuint size, void* result) => (!IsNullPointer && result != (void*)0) ? this.memoryAccess.TryGetByteArrayAt(Address + offset, size, result) : false;
        public bool TryDeref<T>(nint offset, ref T result) where T : unmanaged => (!IsNullPointer) ? this.memoryAccess.TryGetAt(Address + offset, ref result) : false;
        public bool TryDerefSByte(nint offset, ref sbyte result) => (!IsNullPointer) ? this.memoryAccess.TryGetSByteAt(Address + offset, ref result) : false;
        public bool TryDerefByte(nint offset, ref byte result) => (!IsNullPointer) ? this.memoryAccess.TryGetByteAt(Address + offset, ref result) : false;
        public bool TryDerefShort(nint offset, ref short result) => (!IsNullPointer) ? this.memoryAccess.TryGetShortAt(Address + offset, ref result) : false;
        public bool TryDerefUShort(nint offset, ref ushort result) => (!IsNullPointer) ? this.memoryAccess.TryGetUShortAt(Address + offset, ref result) : false;
        //public bool TryDerefInt24(nint offset, ref sbyte result) => (!IsAddressPointerNull) ? this.memoryAccess.TryGetInt24At(Address + offset, ref result) : false;
        //public bool TryDerefUInt24(nint offset, ref sbyte result) => (!IsAddressPointerNull) ? this.memoryAccess.TryGetUInt24At(Address + offset, ref result) : false;
        public bool TryDerefInt(nint offset, ref int result) => (!IsNullPointer) ? this.memoryAccess.TryGetIntAt(Address + offset, ref result) : false;
        public bool TryDerefUInt(nint offset, ref uint result) => (!IsNullPointer) ? this.memoryAccess.TryGetUIntAt(Address + offset, ref result) : false;
        public bool TryDerefLong(nint offset, ref long result) => (!IsNullPointer) ? this.memoryAccess.TryGetLongAt(Address + offset, ref result) : false;
        public bool TryDerefULong(nint offset, ref ulong result) => (!IsNullPointer) ? this.memoryAccess.TryGetULongAt(Address + offset, ref result) : false;
        public bool TryDerefFloat(nint offset, ref float result) => (!IsNullPointer) ? this.memoryAccess.TryGetFloatAt(Address + offset, ref result) : false;
        public bool TryDerefDouble(nint offset, ref double result) => (!IsNullPointer) ? this.memoryAccess.TryGetDoubleAt(Address + offset, ref result) : false;
        public bool TryDerefASCIIString(nint offset, nuint size, ref string result) => (!IsNullPointer) ? this.memoryAccess.TryGetASCIIStringAt(Address + offset, size, ref result) : false;
        public bool TryDerefUnicodeString(nint offset, nuint size, ref string result) => (!IsNullPointer) ? this.memoryAccess.TryGetUnicodeStringAt(Address + offset, size, ref result) : false;
    }
}
