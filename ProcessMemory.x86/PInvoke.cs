using System;
using System.Runtime.InteropServices;
using static ProcessMemory.Common.PInvoke;

namespace ProcessMemory.x86
{
    public static partial class PInvoke
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION32 lpBuffer, IntPtr dwLength);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION32 // 28
        {
            public IntPtr BaseAddress; // 4
            public IntPtr AllocationBase; // 4
            public AllocationProtect AllocationProtect; // 4
            public IntPtr RegionSize; // 4
            public MemoryFlags State; // 4
            public AllocationProtect Protect; // 4
            public MemoryFlags Type; // 4
        }
    }
}
