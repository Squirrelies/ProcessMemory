using System.Collections.Generic;

namespace ProcessMemory
{
    public class MultilevelPointer
    {
        private ProcessMemory memoryAccess;
        public readonly long BaseAddress;
        public long Address { get; private set; }
        private int[] offsets32;
        private long[] offsets64;

        public MultilevelPointer(ProcessMemory memoryAccess, long baseAddress)
        {
            this.memoryAccess = memoryAccess;
            this.BaseAddress = baseAddress;
            this.Address = 0L;
            this.offsets32 = null;
            this.offsets64 = null;
            UpdatePointers();
        }

        public MultilevelPointer(ProcessMemory memoryAccess, long baseAddress, params int[] offsets)
        {
            this.memoryAccess = memoryAccess;
            this.BaseAddress = baseAddress;
            this.Address = 0;
            this.offsets32 = offsets;
            this.offsets64 = null;
            UpdatePointers();
        }

        public MultilevelPointer(ProcessMemory memoryAccess, long baseAddress, params long[] offsets)
        {
            this.memoryAccess = memoryAccess;
            this.BaseAddress = baseAddress;
            this.Address = 0L;
            this.offsets32 = null;
            this.offsets64 = offsets;
            UpdatePointers();
        }

        public void UpdatePointers()
        {
            if (offsets32 != null)
                UpdatePointers32();
            else if (offsets64 != null)
                UpdatePointers64();
            else
            {
                Address = this.memoryAccess.GetLongAt(this.BaseAddress);

                if (Address < 0)
                    Address = 0L;
            }
        }

        private void UpdatePointers32()
        {
            Address = this.memoryAccess.GetIntAt(this.BaseAddress);

            // Out of range.
            if (Address <= 0)
            {
                Address = 0;
                return;
            }

            foreach (int offset in this.offsets32)
            {
                Address = this.memoryAccess.GetIntAt(Address + offset);

                // Out of range.
                if (Address <= 0)
                {
                    Address = 0;
                    return;
                }
            }
        }

        private void UpdatePointers64()
        {
            Address = this.memoryAccess.GetLongAt(this.BaseAddress);

            // Out of range.
            if (Address <= 0L)
            {
                Address = 0L;
                return;
            }

            foreach (long offset in this.offsets64)
            {
                Address = this.memoryAccess.GetLongAt(Address + offset);

                // Out of range.
                if (Address <= 0L)
                {
                    Address = 0L;
                    return;
                }
            }
        }

        public byte[] DerefByteArray(long offset, int size) => (Address > 0) ? this.memoryAccess.GetByteArrayAt(Address + offset, size) : default;
        public sbyte DerefSByte(long offset) => (Address > 0) ? this.memoryAccess.GetSByteAt(Address + offset) : default;
        public byte DerefByte(long offset) => (Address > 0) ? this.memoryAccess.GetByteAt(Address + offset) : default;
        public short DerefShort(long offset) => (Address > 0) ? this.memoryAccess.GetShortAt(Address + offset) : default;
        public ushort DerefUShort(long offset) => (Address > 0) ? this.memoryAccess.GetUShortAt(Address + offset) : default;
        public int DerefInt(long offset) => (Address > 0) ? this.memoryAccess.GetIntAt(Address + offset) : default;
        public uint DerefUInt(long offset) => (Address > 0) ? this.memoryAccess.GetUIntAt(Address + offset) : default;
        public long DerefLong(long offset) => (Address > 0) ? this.memoryAccess.GetLongAt(Address + offset) : default;
        public ulong DerefULong(long offset) => (Address > 0) ? this.memoryAccess.GetULongAt(Address + offset) : default;
        public float DerefFloat(long offset) => (Address > 0) ? this.memoryAccess.GetFloatAt(Address + offset) : default;
        public double DerefDouble(long offset) => (Address > 0) ? this.memoryAccess.GetDoubleAt(Address + offset) : default;
    }
}
