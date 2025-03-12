using System;
using System.Runtime.InteropServices;
using Windows.Win32.System.Memory;

namespace ProcessMemory.Native
{
    public static unsafe class SigScan
    {
        public struct SIGSCAN_OPTIONS_T
        {
            public UIntPtr startAddress;
            public UIntPtr endAddress;
            public byte alignment;
            public PAGE_PROTECTION_FLAGS pageProtectionFlags;
        }

        public struct SIGSCAN_RESULTS_T
        {
            public nuint elements;
            public void** pointers;
        }

#if x64
        [DllImport("ProcessMemory.Native.64.dll", SetLastError = true)]
#elif x86
        [DllImport("ProcessMemory.Native.32.dll", SetLastError = true)]
#endif
        public static extern SIGSCAN_RESULTS_T signature_scan(IntPtr processHandle, string pattern, SIGSCAN_OPTIONS_T *options);

#if x64
        [DllImport("ProcessMemory.Native.64.dll", SetLastError = true)]
#elif x86
        [DllImport("ProcessMemory.Native.32.dll", SetLastError = true)]
#endif
        public static extern SIGSCAN_RESULTS_T signature_scan_default(IntPtr processHandle, string pattern);

#if x64
        [DllImport("ProcessMemory.Native.64.dll", SetLastError = true)]
#elif x86
        [DllImport("ProcessMemory.Native.32.dll", SetLastError = true)]
#endif
        public static extern void free_sigscan_results(SIGSCAN_RESULTS_T *results);
    }
}
