using ProcessMemory.Types;
using System;

namespace ProcessMemory
{
    public unsafe class MultilevelPointer
    {
        private ProcessMemoryHandler memoryAccess;
        public IntPtr BaseAddress { get => (IntPtr)_baseAddress; }
        public IntPtr Address { get => (IntPtr)_address; }

#if x64
        private readonly long* _baseAddress;
        private long _address;
        private long[] offsets;
        public bool IsNullPointer => _address == 0L;
#else
        private readonly int* _baseAddress;
        private int _address;
        private int[] offsets;
        public bool IsNullPointer => _address == 0;
#endif


#if x64
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress) : this(memoryAccess, (long*)baseAddress.ToPointer()) { }
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress, params long[] offsets) : this(memoryAccess, (long*)baseAddress.ToPointer(), offsets) { }
#else
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress) : this(memoryAccess, (int*)baseAddress.ToPointer()) { }
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress, params int[] offsets) : this(memoryAccess, (int*)baseAddress.ToPointer(), offsets) { }
#endif

#if x64
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, long* baseAddress)
        {
            this._address = 0L;
#else
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, int* baseAddress)
        {
            this._address = 0;
#endif
            this.memoryAccess = memoryAccess;
            this._baseAddress = baseAddress;
            this.offsets = null;
            UpdatePointers();
        }

#if x64
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, long* baseAddress, params long[] offsets)
        {
            this._address = 0L;
#else
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, int* baseAddress, params int[] offsets)
        {
            this._address = 0;
#endif
            this.memoryAccess = memoryAccess;
            this._baseAddress = baseAddress;
            this.offsets = offsets;
            UpdatePointers();
        }

        public unsafe void UpdatePointers()
        {
#if x64
            fixed (long* p = &_address)
                memoryAccess.TryGetLongAt(_baseAddress, p);

            if (_address == 0L)
                return;

            if (offsets != null)
            {
                foreach (long offset in offsets)
                {
                    fixed (long* p = &_address)
                        memoryAccess.TryGetLongAt((long*)(_address + offset), p);

                    // Out of range.
                    if (_address == 0L)
                        return;
                }
            }
#else
            fixed (int* p = &_address)
                memoryAccess.TryGetIntAt(_baseAddress, p);

            if (_address == 0)
                return;

            if (offsets != null)
            {
                foreach (int offset in offsets)
                {
                    fixed (int* p = &_address)
                        memoryAccess.TryGetIntAt((int*)(_address + offset), p);

                    // Out of range.
                    if (_address == 0)
                        return;
                }
            }
#endif
        }

        public T Deref<T>(int offset) where T : unmanaged => (!IsNullPointer) ? this.memoryAccess.GetAt<T>(IntPtr.Add(Address, offset)) : default;
        public Span<byte> DerefSpanByte(int offset, int size) => (!IsNullPointer) ? this.memoryAccess.GetSpanByteAt(IntPtr.Add(Address, offset), size) : default;
        public byte[] DerefByteArray(int offset, int size) => (!IsNullPointer) ? this.memoryAccess.GetByteArrayAt(IntPtr.Add(Address, offset), size) : default;
        public sbyte DerefSByte(int offset) => (!IsNullPointer) ? this.memoryAccess.GetSByteAt(IntPtr.Add(Address, offset)) : default;
        public byte DerefByte(int offset) => (!IsNullPointer) ? this.memoryAccess.GetByteAt(IntPtr.Add(Address, offset)) : default;
        public short DerefShort(int offset) => (!IsNullPointer) ? this.memoryAccess.GetShortAt(IntPtr.Add(Address, offset)) : default;
        public ushort DerefUShort(int offset) => (!IsNullPointer) ? this.memoryAccess.GetUShortAt(IntPtr.Add(Address, offset)) : default;
        //public Int24 DerefInt24(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetInt24At(IntPtr.Add(Address, offset)) : default;
        //public UInt24 DerefUInt24(int offset) => (!IsAddressPointerNull) ? this.memoryAccess.GetUInt24At(IntPtr.Add(Address, offset)) : default;
        public int DerefInt(int offset) => (!IsNullPointer) ? this.memoryAccess.GetIntAt(IntPtr.Add(Address, offset)) : default;
        public uint DerefUInt(int offset) => (!IsNullPointer) ? this.memoryAccess.GetUIntAt(IntPtr.Add(Address, offset)) : default;
        public long DerefLong(int offset) => (!IsNullPointer) ? this.memoryAccess.GetLongAt(IntPtr.Add(Address, offset)) : default;
        public ulong DerefULong(int offset) => (!IsNullPointer) ? this.memoryAccess.GetULongAt(IntPtr.Add(Address, offset)) : default;
        public float DerefFloat(int offset) => (!IsNullPointer) ? this.memoryAccess.GetFloatAt(IntPtr.Add(Address, offset)) : default;
        public double DerefDouble(int offset) => (!IsNullPointer) ? this.memoryAccess.GetDoubleAt(IntPtr.Add(Address, offset)) : default;
        public string DerefASCIIString(int offset, int size) => (!IsNullPointer) ? this.memoryAccess.GetASCIIStringAt(IntPtr.Add(Address, offset), size) : default;
        public string DerefUnicodeString(int offset, int size) => (!IsNullPointer) ? this.memoryAccess.GetUnicodeStringAt(IntPtr.Add(Address, offset), size) : default;

        public bool TryDerefByteArray(int offset, int size, IntPtr result) => (!IsNullPointer && result != IntPtr.Zero) ? this.memoryAccess.TryGetByteArrayAt(IntPtr.Add(Address, offset), size, result) : false;
        public bool TryDeref<T>(int offset, ref T result) where T : unmanaged => (!IsNullPointer) ? this.memoryAccess.TryGetAt(IntPtr.Add(Address, offset), ref result) : false;
        public bool TryDerefSByte(int offset, ref sbyte result) => (!IsNullPointer) ? this.memoryAccess.TryGetSByteAt(IntPtr.Add(Address, offset), ref result) : false;
        public bool TryDerefByte(int offset, ref byte result) => (!IsNullPointer) ? this.memoryAccess.TryGetByteAt(IntPtr.Add(Address, offset), ref result) : false;
        public bool TryDerefShort(int offset, ref short result) => (!IsNullPointer) ? this.memoryAccess.TryGetShortAt(IntPtr.Add(Address, offset), ref result) : false;
        public bool TryDerefUShort(int offset, ref ushort result) => (!IsNullPointer) ? this.memoryAccess.TryGetUShortAt(IntPtr.Add(Address, offset), ref result) : false;
        //public bool TryDerefInt24(int offset, ref sbyte result) => (!IsAddressPointerNull) ? this.memoryAccess.TryGetInt24At(IntPtr.Add(Address, offset), ref result) : false;
        //public bool TryDerefUInt24(int offset, ref sbyte result) => (!IsAddressPointerNull) ? this.memoryAccess.TryGetUInt24At(IntPtr.Add(Address, offset), ref result) : false;
        public bool TryDerefInt(int offset, ref int result) => (!IsNullPointer) ? this.memoryAccess.TryGetIntAt(IntPtr.Add(Address, offset), ref result) : false;
        public bool TryDerefUInt(int offset, ref uint result) => (!IsNullPointer) ? this.memoryAccess.TryGetUIntAt(IntPtr.Add(Address, offset), ref result) : false;
        public bool TryDerefLong(int offset, ref long result) => (!IsNullPointer) ? this.memoryAccess.TryGetLongAt(IntPtr.Add(Address, offset), ref result) : false;
        public bool TryDerefULong(int offset, ref ulong result) => (!IsNullPointer) ? this.memoryAccess.TryGetULongAt(IntPtr.Add(Address, offset), ref result) : false;
        public bool TryDerefFloat(int offset, ref float result) => (!IsNullPointer) ? this.memoryAccess.TryGetFloatAt(IntPtr.Add(Address, offset), ref result) : false;
        public bool TryDerefDouble(int offset, ref double result) => (!IsNullPointer) ? this.memoryAccess.TryGetDoubleAt(IntPtr.Add(Address, offset), ref result) : false;
        public bool TryDerefASCIIString(int offset, int size, ref string result) => (!IsNullPointer) ? this.memoryAccess.TryGetASCIIStringAt(IntPtr.Add(Address, offset), size, ref result) : false;
        public bool TryDerefUnicodeString(int offset, int size, ref string result) => (!IsNullPointer) ? this.memoryAccess.TryGetUnicodeStringAt(IntPtr.Add(Address, offset), size, ref result) : false;

#if x64
        public bool TryDerefByteArray(int offset, int size, byte* result) => (!IsNullPointer && result != (byte*)0) ? this.memoryAccess.TryGetByteArrayAt((long*)(_address + offset), size, result) : false;
        public bool TryDeref<T>(int offset, T* result) where T : unmanaged => (!IsNullPointer && result != (T*)0) ? this.memoryAccess.TryGetAt<T>((long*)(_address + offset), ref *result) : false;
        public bool TryDerefSByte(int offset, sbyte* result) => (!IsNullPointer && result != (sbyte*)0) ? this.memoryAccess.TryGetSByteAt((long*)(_address + offset), result) : false;
        public bool TryDerefByte(int offset, byte* result) => (!IsNullPointer && result != (byte*)0) ? this.memoryAccess.TryGetByteAt((long*)(_address + offset), result) : false;
        public bool TryDerefShort(int offset, short* result) => (!IsNullPointer && result != (short*)0) ? this.memoryAccess.TryGetShortAt((long*)(_address + offset), result) : false;
        public bool TryDerefUShort(int offset, ushort* result) => (!IsNullPointer && result != (ushort*)0) ? this.memoryAccess.TryGetUShortAt((long*)(_address + offset), result) : false;
        //public bool TryDerefInt24(int offset, byte* result) => (!IsAddressPointerNull && result != (byte*)0) ? this.memoryAccess.TryGetInt24At((long*)(_address + offset), result) : false;
        //public bool TryDerefUInt24(int offset, byte* result) => (!IsAddressPointerNull && result != (byte*)0) ? this.memoryAccess.TryGetUInt24At((long*)(_address + offset), result) : false;
        public bool TryDerefInt(int offset, int* result) => (!IsNullPointer && result != (int*)0) ? this.memoryAccess.TryGetIntAt((long*)(_address + offset), result) : false;
        public bool TryDerefUInt(int offset, uint* result) => (!IsNullPointer && result != (uint*)0) ? this.memoryAccess.TryGetUIntAt((long*)(_address + offset), result) : false;
        public bool TryDerefLong(int offset, long* result) => (!IsNullPointer && result != (long*)0) ? this.memoryAccess.TryGetLongAt((long*)(_address + offset), result) : false;
        public bool TryDerefULong(int offset, ulong* result) => (!IsNullPointer && result != (ulong*)0) ? this.memoryAccess.TryGetULongAt((long*)(_address + offset), result) : false;
        public bool TryDerefFloat(int offset, float* result) => (!IsNullPointer && result != (float*)0) ? this.memoryAccess.TryGetFloatAt((long*)(_address + offset), result) : false;
        public bool TryDerefDouble(int offset, double* result) => (!IsNullPointer && result != (double*)0) ? this.memoryAccess.TryGetDoubleAt((long*)(_address + offset), result) : false;
#else
        public bool TryDerefByteArray(int offset, int size, byte* result) => (!IsNullPointer && result != (byte*)0) ? this.memoryAccess.TryGetByteArrayAt((int*)(_address + offset), size, result) : false;
        public bool TryDeref<T>(int offset, T* result) where T : unmanaged => (!IsNullPointer && result != (T*)0) ? this.memoryAccess.TryGetAt<T>((int*)(_address + offset), ref *result) : false;
        public bool TryDerefSByte(int offset, sbyte* result) => (!IsNullPointer && result != (sbyte*)0) ? this.memoryAccess.TryGetSByteAt((int*)(_address + offset), result) : false;
        public bool TryDerefByte(int offset, byte* result) => (!IsNullPointer && result != (byte*)0) ? this.memoryAccess.TryGetByteAt((int*)(_address + offset), result) : false;
        public bool TryDerefShort(int offset, short* result) => (!IsNullPointer && result != (short*)0) ? this.memoryAccess.TryGetShortAt((int*)(_address + offset), result) : false;
        public bool TryDerefUShort(int offset, ushort* result) => (!IsNullPointer && result != (ushort*)0) ? this.memoryAccess.TryGetUShortAt((int*)(_address + offset), result) : false;
        //public bool TryDerefInt24(int offset, byte* result) => (!IsAddressPointerNull && result != (byte*)0) ? this.memoryAccess.TryGetInt24At((int*)(_address + offset), result) : false;
        //public bool TryDerefUInt24(int offset, byte* result) => (!IsAddressPointerNull && result != (byte*)0) ? this.memoryAccess.TryGetUInt24At((int*)(_address + offset), result) : false;
        public bool TryDerefInt(int offset, int* result) => (!IsNullPointer && result != (int*)0) ? this.memoryAccess.TryGetIntAt((int*)(_address + offset), result) : false;
        public bool TryDerefUInt(int offset, uint* result) => (!IsNullPointer && result != (uint*)0) ? this.memoryAccess.TryGetUIntAt((int*)(_address + offset), result) : false;
        public bool TryDerefLong(int offset, long* result) => (!IsNullPointer && result != (long*)0) ? this.memoryAccess.TryGetLongAt((int*)(_address + offset), result) : false;
        public bool TryDerefULong(int offset, ulong* result) => (!IsNullPointer && result != (ulong*)0) ? this.memoryAccess.TryGetULongAt((int*)(_address + offset), result) : false;
        public bool TryDerefFloat(int offset, float* result) => (!IsNullPointer && result != (float*)0) ? this.memoryAccess.TryGetFloatAt((int*)(_address + offset), result) : false;
        public bool TryDerefDouble(int offset, double* result) => (!IsNullPointer && result != (double*)0) ? this.memoryAccess.TryGetDoubleAt((int*)(_address + offset), result) : false;
#endif
    }
}
