using System.Collections.Generic;

namespace ProcessMemory
{
    public class MultilevelPointer
    {
        private ProcessMemory memoryAccess;
        public readonly long BaseAddress;
        public IReadOnlyList<long> Addresses;
        private int[] offsets32;
        private long[] offsets64;

        public MultilevelPointer(ProcessMemory memoryAccess, long baseAddress, params int[] offsets)
        {
            this.memoryAccess = memoryAccess;
            this.BaseAddress = baseAddress;
            this.offsets32 = offsets;
            this.offsets64 = null;
            UpdatePointers();
        }

        public MultilevelPointer(ProcessMemory memoryAccess, long baseAddress, params long[] offsets)
        {
            this.memoryAccess = memoryAccess;
            this.BaseAddress = baseAddress;
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
        }

        private void UpdatePointers32()
        {
            List<long> local = new List<long>();

            local.Add(this.memoryAccess.GetIntAt(this.BaseAddress));
            foreach (int offset in this.offsets32)
                local.Add(this.memoryAccess.GetIntAt(local[local.Count - 1] + offset));

            Addresses = local.AsReadOnly();
        }

        private void UpdatePointers64()
        {
            List<long> local = new List<long>();

            local.Add(this.memoryAccess.GetLongAt(this.BaseAddress));
            foreach (long offset in this.offsets64)
                local.Add(this.memoryAccess.GetLongAt(local[local.Count - 1] + offset));

            Addresses = local.AsReadOnly();
        }

        public sbyte DerefSByte(long offset) => this.memoryAccess.GetSByteAt(Addresses[Addresses.Count - 1] + offset);
        public byte DerefByte(long offset) => this.memoryAccess.GetByteAt(Addresses[Addresses.Count - 1] + offset);
        public short DerefShort(long offset) => this.memoryAccess.GetShortAt(Addresses[Addresses.Count - 1] + offset);
        public ushort DerefUShort(long offset) => this.memoryAccess.GetUShortAt(Addresses[Addresses.Count - 1] + offset);
        public int DerefInt(long offset) => this.memoryAccess.GetIntAt(Addresses[Addresses.Count - 1] + offset);
        public uint DerefUInt(long offset) => this.memoryAccess.GetUIntAt(Addresses[Addresses.Count - 1] + offset);
        public long DerefLong(long offset) => this.memoryAccess.GetLongAt(Addresses[Addresses.Count - 1] + offset);
        public ulong DerefULong(long offset) => this.memoryAccess.GetULongAt(Addresses[Addresses.Count - 1] + offset);
        public float DerefFloat(long offset) => this.memoryAccess.GetFloatAt(Addresses[Addresses.Count - 1] + offset);
        public double DerefDouble(long offset) => this.memoryAccess.GetDoubleAt(Addresses[Addresses.Count - 1] + offset);
    }
}
