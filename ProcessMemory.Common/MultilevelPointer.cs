using ProcessMemory.Common.Types;
using System;

namespace ProcessMemory.Common
{
    public unsafe abstract class MultilevelPointer
    {
        private ProcessMemoryHandler memoryAccess;
        public readonly IntPtr BaseAddress;
        public IntPtr Address { get; private set; }
        private int[] offsets;
        protected abstract int intPtrSize { get; }

        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress)
        {
            this.memoryAccess = memoryAccess;
            this.BaseAddress = baseAddress;
            this.Address = IntPtr.Zero;
            this.offsets = null;
            UpdatePointers();
        }

        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress, params int[] offsets)
        {
            this.memoryAccess = memoryAccess;
            this.BaseAddress = baseAddress;
            this.Address = IntPtr.Zero;
            this.offsets = offsets;
            UpdatePointers();
        }

        public void UpdatePointers()
        {
            if (intPtrSize == 4)
                Address = new IntPtr(this.memoryAccess.GetIntAt(this.BaseAddress));
            else if (intPtrSize == 8)
                Address = new IntPtr(this.memoryAccess.GetLongAt(this.BaseAddress));

            if (Address == IntPtr.Zero)
                return;

            if (offsets != null)
            {
                foreach (int offset in this.offsets)
                {
                    if (intPtrSize == 4)
                        Address = new IntPtr(this.memoryAccess.GetIntAt(IntPtr.Add(Address, offset)));
                    else if (intPtrSize == 8)
                        Address = new IntPtr(this.memoryAccess.GetLongAt(IntPtr.Add(Address, offset)));

                    // Out of range.
                    if (Address == IntPtr.Zero)
                        return;
                }
            }
        }

        public byte[] DerefByteArray(int offset, int size) => (Address != IntPtr.Zero) ? this.memoryAccess.GetByteArrayAt(IntPtr.Add(Address, offset), size) : default;
        public sbyte DerefSByte(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetSByteAt(IntPtr.Add(Address, offset)) : default;
        public byte DerefByte(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetByteAt(IntPtr.Add(Address, offset)) : default;
        public short DerefShort(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetShortAt(IntPtr.Add(Address, offset)) : default;
        public ushort DerefUShort(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetUShortAt(IntPtr.Add(Address, offset)) : default;
        public Int24 DerefInt24(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetInt24At(IntPtr.Add(Address, offset)) : default;
        public UInt24 DerefUInt24(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetUInt24At(IntPtr.Add(Address, offset)) : default;
        public int DerefInt(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetIntAt(IntPtr.Add(Address, offset)) : default;
        public uint DerefUInt(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetUIntAt(IntPtr.Add(Address, offset)) : default;
        public long DerefLong(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetLongAt(IntPtr.Add(Address, offset)) : default;
        public ulong DerefULong(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetULongAt(IntPtr.Add(Address, offset)) : default;
        public float DerefFloat(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetFloatAt(IntPtr.Add(Address, offset)) : default;
        public double DerefDouble(int offset) => (Address != IntPtr.Zero) ? this.memoryAccess.GetDoubleAt(IntPtr.Add(Address, offset)) : default;

        public bool TryDerefByteArray(int offset, int size, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetByteArrayAt(IntPtr.Add(Address, offset), size, result) : false;
        public bool TryDerefSByte(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetSByteAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefByte(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetByteAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefShort(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetShortAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUShort(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetUShortAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefInt24(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetInt24At(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUInt24(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetUInt24At(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefInt(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetIntAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUInt(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetUIntAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefLong(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetLongAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefULong(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetULongAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefFloat(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetFloatAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefDouble(int offset, IntPtr result) => (Address != IntPtr.Zero && result != IntPtr.Zero) ? this.memoryAccess.TryGetDoubleAt(IntPtr.Add(Address, offset), result) : false;

        public bool TryDerefByteArray(int offset, int size, byte* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetByteArrayAt(IntPtr.Add(Address, offset), size, result) : false;
        public bool TryDerefSByte(int offset, sbyte* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetSByteAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefByte(int offset, byte* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetByteAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefShort(int offset, short* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetShortAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUShort(int offset, ushort* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetUShortAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefInt24(int offset, byte* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetInt24At(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUInt24(int offset, byte* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetUInt24At(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefInt(int offset, int* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetIntAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUInt(int offset, uint* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetUIntAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefLong(int offset, long* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetLongAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefULong(int offset, ulong* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetULongAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefFloat(int offset, float* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetFloatAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefDouble(int offset, double* result) => (Address != IntPtr.Zero && (IntPtr)result != IntPtr.Zero) ? this.memoryAccess.TryGetDoubleAt(IntPtr.Add(Address, offset), result) : false;
    }
}
