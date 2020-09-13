using ProcessMemory.Common.Types;
using System;
using System.Runtime.CompilerServices;

namespace ProcessMemory.Common
{
    public unsafe abstract class MultilevelPointer
    {
        private ProcessMemoryHandler memoryAccess;
        public IntPtr BaseAddress { get => (IntPtr)_baseAddress; }
        private readonly void* _baseAddress;
        public IntPtr Address { get => (IntPtr)_address; }
        private void* _address;
        private int[] offsets;
        protected abstract int intPtrSize { get; }

        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress) : this(memoryAccess, baseAddress.ToPointer()) { }

        public MultilevelPointer(ProcessMemoryHandler memoryAccess, void* baseAddress)
        {
            this.memoryAccess = memoryAccess;
            this._baseAddress = baseAddress;
            this._address = (void*)0;
            this.offsets = null;
            UpdatePointers();
        }

        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress, params int[] offsets) : this(memoryAccess, baseAddress.ToPointer(), offsets) { }

        public MultilevelPointer(ProcessMemoryHandler memoryAccess, void* baseAddress, params int[] offsets)
        {
            this.memoryAccess = memoryAccess;
            this._baseAddress = baseAddress;
            this._address = (void*)0;
            this.offsets = offsets;
            UpdatePointers();
        }

        public void UpdatePointers()
        {
            if (intPtrSize == 4)
                fixed (void* p = &_address)
                    memoryAccess.TryGetIntAt(_baseAddress, (int*)p);
            else if (intPtrSize == 8)
                fixed (void* p = &_address)
                    memoryAccess.TryGetLongAt(_baseAddress, (long*)p);

            if (_address == (void*)0)
                return;

            if (offsets != null)
            {
                foreach (int offset in offsets)
                {
                    if (intPtrSize == 4)
                        fixed (void* p = &_address)
                            memoryAccess.TryGetIntAt((int*)p + offset, (int*)p);
                    else if (intPtrSize == 8)
                        fixed (void* p = &_address)
                            memoryAccess.TryGetLongAt((long*)p + offset, (long*)p);

                    // Out of range.
                    if (_address == (void*)0)
                        return;
                }
            }
        }

        public byte[] DerefByteArray(int offset, int size) => (_address != (void*)0) ? this.memoryAccess.GetByteArrayAt(IntPtr.Add(Address, offset), size) : default;
        public sbyte DerefSByte(int offset) => (_address != (void*)0) ? this.memoryAccess.GetSByteAt(IntPtr.Add(Address, offset)) : default;
        public byte DerefByte(int offset) => (_address != (void*)0) ? this.memoryAccess.GetByteAt(IntPtr.Add(Address, offset)) : default;
        public short DerefShort(int offset) => (_address != (void*)0) ? this.memoryAccess.GetShortAt(IntPtr.Add(Address, offset)) : default;
        public ushort DerefUShort(int offset) => (_address != (void*)0) ? this.memoryAccess.GetUShortAt(IntPtr.Add(Address, offset)) : default;
        public Int24 DerefInt24(int offset) => (_address != (void*)0) ? this.memoryAccess.GetInt24At(IntPtr.Add(Address, offset)) : default;
        public UInt24 DerefUInt24(int offset) => (_address != (void*)0) ? this.memoryAccess.GetUInt24At(IntPtr.Add(Address, offset)) : default;
        public int DerefInt(int offset) => (_address != (void*)0) ? this.memoryAccess.GetIntAt(IntPtr.Add(Address, offset)) : default;
        public uint DerefUInt(int offset) => (_address != (void*)0) ? this.memoryAccess.GetUIntAt(IntPtr.Add(Address, offset)) : default;
        public long DerefLong(int offset) => (_address != (void*)0) ? this.memoryAccess.GetLongAt(IntPtr.Add(Address, offset)) : default;
        public ulong DerefULong(int offset) => (_address != (void*)0) ? this.memoryAccess.GetULongAt(IntPtr.Add(Address, offset)) : default;
        public float DerefFloat(int offset) => (_address != (void*)0) ? this.memoryAccess.GetFloatAt(IntPtr.Add(Address, offset)) : default;
        public double DerefDouble(int offset) => (_address != (void*)0) ? this.memoryAccess.GetDoubleAt(IntPtr.Add(Address, offset)) : default;

        public bool TryDerefByteArray(int offset, int size, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetByteArrayAt(IntPtr.Add(Address, offset), size, result) : false;
        public bool TryDerefSByte(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetSByteAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefByte(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetByteAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefShort(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetShortAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUShort(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetUShortAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefInt24(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetInt24At(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUInt24(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetUInt24At(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefInt(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetIntAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUInt(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetUIntAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefLong(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetLongAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefULong(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetULongAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefFloat(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetFloatAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefDouble(int offset, IntPtr result) => (_address != (void*)0 && result != IntPtr.Zero) ? this.memoryAccess.TryGetDoubleAt(IntPtr.Add(Address, offset), result) : false;

        public bool TryDerefByteArray(int offset, int size, byte* result) => (_address != (void*)0 && result != (byte*)0) ? this.memoryAccess.TryGetByteArrayAt(IntPtr.Add(Address, offset), size, result) : false;
        public bool TryDerefSByte(int offset, sbyte* result) => (_address != (void*)0 && result != (sbyte*)0) ? this.memoryAccess.TryGetSByteAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefByte(int offset, byte* result) => (_address != (void*)0 && result != (byte*)0) ? this.memoryAccess.TryGetByteAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefShort(int offset, short* result) => (_address != (void*)0 && result != (short*)0) ? this.memoryAccess.TryGetShortAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUShort(int offset, ushort* result) => (_address != (void*)0 && result != (ushort*)0) ? this.memoryAccess.TryGetUShortAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefInt24(int offset, byte* result) => (_address != (void*)0 && result != (byte*)0) ? this.memoryAccess.TryGetInt24At(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUInt24(int offset, byte* result) => (_address != (void*)0 && result != (byte*)0) ? this.memoryAccess.TryGetUInt24At(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefInt(int offset, int* result) => (_address != (void*)0 && result != (int*)0) ? this.memoryAccess.TryGetIntAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUInt(int offset, uint* result) => (_address != (void*)0 && result != (uint*)0) ? this.memoryAccess.TryGetUIntAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefLong(int offset, long* result) => (_address != (void*)0 && result != (long*)0) ? this.memoryAccess.TryGetLongAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefULong(int offset, ulong* result) => (_address != (void*)0 && result != (ulong*)0) ? this.memoryAccess.TryGetULongAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefFloat(int offset, float* result) => (_address != (void*)0 && result != (float*)0) ? this.memoryAccess.TryGetFloatAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefDouble(int offset, double* result) => (_address != (void*)0 && result != (double*)0) ? this.memoryAccess.TryGetDoubleAt(IntPtr.Add(Address, offset), result) : false;
    }
}
