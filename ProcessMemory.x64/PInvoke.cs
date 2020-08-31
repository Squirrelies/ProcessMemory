using System;
using System.Runtime.InteropServices;
using static ProcessMemory.Common.PInvoke;

namespace ProcessMemory.x64
{
    public static partial class PInvoke
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, IntPtr dwLength);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION64 // 48
        {
            public IntPtr BaseAddress; // 8
            public IntPtr AllocationBase; // 8
            public AllocationProtect AllocationProtect; // 4
            public int __alignment1; // 4
            public IntPtr RegionSize; // 8
            public MemoryFlags State; // 4
            public AllocationProtect Protect; // 4
            public MemoryFlags Type; // 4
            public int __alignment2; // 4
        }
    }
}
