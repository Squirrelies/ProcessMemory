using System;

namespace ProcessMemory.x64
{
    public unsafe class MultilevelPointer : Common.MultilevelPointer
    {
        protected override int intPtrSize => 8;
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress) : base(memoryAccess, baseAddress) { }
        public MultilevelPointer(ProcessMemoryHandler memoryAccess, IntPtr baseAddress, params int[] offsets) : base(memoryAccess, baseAddress, offsets) { }
    }
}
