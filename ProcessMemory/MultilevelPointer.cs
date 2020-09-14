using ProcessMemory.Types;
using System;

namespace ProcessMemory
{
    public unsafe abstract class MultilevelPointer
    {
        private ProcessMemoryHandler memoryAccess;
        public IntPtr BaseAddress { get => (IntPtr)_baseAddress; }
        public IntPtr Address { get => (IntPtr)_address; }
        private int[] offsets;

#if x64
        private readonly long* _baseAddress;
        private long* _address;
        private bool IsAddressPointerNull => _address == (long*)0;
#else
        private readonly int* _baseAddress;
        private int* _address;
        private bool IsAddressPointerNull => _address == (int*)0;
#endif


#if x64
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress) : this(memoryAccess, (long*)baseAddress.ToPointer()) { }
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress, params int[] offsets) : this(memoryAccess, (long*)baseAddress.ToPointer(), offsets) { }
#else
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress) : this(memoryAccess, (int*)baseAddress.ToPointer()) { }
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress, params int[] offsets) : this(memoryAccess, (int*)baseAddress.ToPointer(), offsets) { }
#endif

#if x64
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, long* baseAddress)
        {
            this._address = (long*)0;
#else
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, int* baseAddress)
        {
            this._address = (int*)0;
#endif
            this.memoryAccess = memoryAccess;
            this._baseAddress = baseAddress;
            this.offsets = null;
            UpdatePointers();
        }

#if x64
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, long* baseAddress, params int[] offsets)
        {
            this._address = (long*)0;
#else
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, int* baseAddress, params int[] offsets)
        {
            this._address = (int*)0;
#endif
            this.memoryAccess = memoryAccess;
            this._baseAddress = baseAddress;
            this.offsets = offsets;
            UpdatePointers();
        }

        public void UpdatePointers()
        {
#if x64
            memoryAccess.TryGetLongAt(_baseAddress, _address);

            if (_address == (long*)0)
                return;

            if (offsets != null)
            {
                foreach (int offset in offsets)
                {
                    memoryAccess.TryGetLongAt(_address + offset, _address);

                    // Out of range.
                    if (_address == (long*)0)
                        return;
                }
            }
#else
            memoryAccess.TryGetIntAt(_baseAddress, _address);

            if (_address == (int*)0)
                return;

            if (offsets != null)
            {
                foreach (int offset in offsets)
                {
                    memoryAccess.TryGetIntAt(_address + offset, _address);

                    // Out of range.
                    if (_address == (int*)0)
                        return;
                }
            }
#endif
        }

        public byte[] DerefByteArray(int offset, int size) => (!IsAddressPointerNull) ? this.memoryAccess.GetByteArrayAt(IntPtr.Add(Address, offset), size) : default;
        public sbyte DerefSByte(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetSByteAt(IntPtr.Add(Address, offset)) : default;
        public byte DerefByte(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetByteAt(IntPtr.Add(Address, offset)) : default;
        public short DerefShort(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetShortAt(IntPtr.Add(Address, offset)) : default;
        public ushort DerefUShort(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetUShortAt(IntPtr.Add(Address, offset)) : default;
        public Int24 DerefInt24(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetInt24At(IntPtr.Add(Address, offset)) : default;
        public UInt24 DerefUInt24(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetUInt24At(IntPtr.Add(Address, offset)) : default;
        public int DerefInt(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetIntAt(IntPtr.Add(Address, offset)) : default;
        public uint DerefUInt(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetUIntAt(IntPtr.Add(Address, offset)) : default;
        public long DerefLong(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetLongAt(IntPtr.Add(Address, offset)) : default;
        public ulong DerefULong(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetULongAt(IntPtr.Add(Address, offset)) : default;
        public float DerefFloat(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetFloatAt(IntPtr.Add(Address, offset)) : default;
        public double DerefDouble(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetDoubleAt(IntPtr.Add(Address, offset)) : default;

        public bool TryDerefByteArray(int offset, int size, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetByteArrayAt(IntPtr.Add(Address, offset), size, result) : false;
        public bool TryDerefSByte(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetSByteAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefByte(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetByteAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefShort(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetShortAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUShort(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetUShortAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefInt24(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetInt24At(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUInt24(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetUInt24At(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefInt(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetIntAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefUInt(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetUIntAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefLong(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetLongAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefULong(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetULongAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefFloat(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetFloatAt(IntPtr.Add(Address, offset), result) : false;
        public bool TryDerefDouble(int offset, IntPtr result) => (!IsAddressPointerNull && result != IntPtr.Zero) ? this.memoryAccess.TryGetDoubleAt(IntPtr.Add(Address, offset), result) : false;

        public bool TryDerefByteArray(int offset, int size, byte* result) => (!IsAddressPointerNull && result != (byte*)0) ? this.memoryAccess.TryGetByteArrayAt(_address + offset, size, result) : false;
        public bool TryDerefSByte(int offset, sbyte* result) => (!IsAddressPointerNull && result != (sbyte*)0) ? this.memoryAccess.TryGetSByteAt(_address + offset, result) : false;
        public bool TryDerefByte(int offset, byte* result) => (!IsAddressPointerNull && result != (byte*)0) ? this.memoryAccess.TryGetByteAt(_address + offset, result) : false;
        public bool TryDerefShort(int offset, short* result) => (!IsAddressPointerNull && result != (short*)0) ? this.memoryAccess.TryGetShortAt(_address + offset, result) : false;
        public bool TryDerefUShort(int offset, ushort* result) => (!IsAddressPointerNull && result != (ushort*)0) ? this.memoryAccess.TryGetUShortAt(_address + offset, result) : false;
        public bool TryDerefInt24(int offset, byte* result) => (!IsAddressPointerNull && result != (byte*)0) ? this.memoryAccess.TryGetInt24At(_address + offset, result) : false;
        public bool TryDerefUInt24(int offset, byte* result) => (!IsAddressPointerNull && result != (byte*)0) ? this.memoryAccess.TryGetUInt24At(_address + offset, result) : false;
        public bool TryDerefInt(int offset, int* result) => (!IsAddressPointerNull && result != (int*)0) ? this.memoryAccess.TryGetIntAt(_address + offset, result) : false;
        public bool TryDerefUInt(int offset, uint* result) => (!IsAddressPointerNull && result != (uint*)0) ? this.memoryAccess.TryGetUIntAt(_address + offset, result) : false;
        public bool TryDerefLong(int offset, long* result) => (!IsAddressPointerNull && result != (long*)0) ? this.memoryAccess.TryGetLongAt(_address + offset, result) : false;
        public bool TryDerefULong(int offset, ulong* result) => (!IsAddressPointerNull && result != (ulong*)0) ? this.memoryAccess.TryGetULongAt(_address + offset, result) : false;
        public bool TryDerefFloat(int offset, float* result) => (!IsAddressPointerNull && result != (float*)0) ? this.memoryAccess.TryGetFloatAt(_address + offset, result) : false;
        public bool TryDerefDouble(int offset, double* result) => (!IsAddressPointerNull && result != (double*)0) ? this.memoryAccess.TryGetDoubleAt(_address + offset, result) : false;
    }
}
