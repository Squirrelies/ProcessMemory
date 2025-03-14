using System;
using System.ComponentModel;
using System.Threading;
using static Windows.Win32.PInvoke;
using Windows.Win32.System.ProcessStatus;
using Windows.Win32.System.Threading;
using Windows.Win32.Foundation;
using Windows.Win32;
using Microsoft.Win32.SafeHandles;
using Windows.Win32.System.Memory;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ProcessMemory
{
    public static class NativeWrappers
    {
        public static bool GetProcessWoW64(uint pid)
        {
            using (var processHandle = OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION, false, pid).ToSafeProcessHandle())
                return GetProcessWoW64(processHandle);
        }

        public unsafe static bool GetProcessWoW64(SafeProcessHandle processHandle)
        {
            BOOL returnValue = new BOOL();
            if (IsWow64Process(processHandle, out returnValue).Value == 0)
                throw new Win32Exception();

            return returnValue;
        }

        public unsafe static string? GetProcessPath(uint pid)
        {
            using (var processHandle = OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ, false, pid).ToSafeProcessHandle())
            {
                // Query process image name.
                Span<char> imageFileName = stackalloc char[2048];
                uint imageFileNameSize = (uint)imageFileName.Length;
                if (QueryFullProcessImageNameW(processHandle, PROCESS_NAME_FORMAT.PROCESS_NAME_WIN32, imageFileName, ref imageFileNameSize))
                    return imageFileName.ToString();
                else
                    return default;
            }
        }

        public static SafeProcessHandle GetSafeProcessHandle(ushort pid, bool inheritHandle = false, PROCESS_ACCESS_RIGHTS processAccessRights = PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ) => OpenProcess(processAccessRights, inheritHandle, pid).ToSafeProcessHandle();

        public unsafe static FreeLibrarySafeHandle GetProcessBaseAddress(uint pid, ENUM_PROCESS_MODULES_EX_FLAGS moduleTypes = ENUM_PROCESS_MODULES_EX_FLAGS.LIST_MODULES_ALL)
        {
            using (var processHandle = OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ, false, pid).ToSafeProcessHandle())
            {
                // Query process image name.
                Span<char> imageFileName = stackalloc char[2048];
                uint imageFileNameSize = (uint)imageFileName.Length;
                if (QueryFullProcessImageNameW(processHandle, PROCESS_NAME_FORMAT.PROCESS_NAME_WIN32, imageFileName, ref imageFileNameSize))
                {
                    // Query process handle's modules.
                    FreeLibrarySafeHandle hModule = new FreeLibrarySafeHandle();
                    uint cb = (uint)(sizeof(HINSTANCE) * 1024U);
                    uint lpcbNeeded = 0;
                    BOOL enumProcessModulesExReturn = new BOOL();
                    enumProcessModulesExReturn = K32EnumProcessModulesEx(processHandle, out hModule, cb, out lpcbNeeded, (uint)moduleTypes);

                    // If we failed, attempt to repeat the run a few times.
                    if (!enumProcessModulesExReturn)
                    {
                        // https://referencesource.microsoft.com/#system/services/monitoring/system/diagnosticts/ProcessManager.cs,639
                        // Per Microsoft's Reference Source page:
                        // "Also, EnumProcessModules is not a reliable method to get the modules for a process. 
                        // If OS loader is touching module information, this method might fail and copy part of the data.
                        // This is no easy solution to this problem. The only reliable way to fix this is to 
                        // suspend all the threads in target process. Of course we don't want to do this in Process class.
                        // So we just to try avoid the ---- by calling the same method 50 (an arbitary number) times."
                        bool sourceProcessIsWow64 = GetProcessWoW64(GetCurrentProcessId());
                        bool targetProcessIsWow64 = GetProcessWoW64(pid);

                        if (sourceProcessIsWow64 && !targetProcessIsWow64)
                            throw new Win32Exception((int)Win32Error.ERROR_PARTIAL_COPY, "299 (ERROR_PARTIAL_COPY) - One process is WOW64 and the other is not.");

                        for (int i = 0; i < 50; ++i)
                        {
                            enumProcessModulesExReturn = K32EnumProcessModulesEx(processHandle, out hModule, cb, out lpcbNeeded, (uint)moduleTypes);
                            if (enumProcessModulesExReturn)
                                break; // If we succeeded, break out of the loop early.

                            // Sleep for 1ms then try again.
                            Thread.Sleep(1);
                        }
                    }

                    // If we successfully retrieved an array of process modules, enumerate through them to find the main module.
                    if (enumProcessModulesExReturn)
                    {
                        uint moduleCount = lpcbNeeded / sizeof(long);
                        for (uint i = 0; i < moduleCount; i++)
                        {
                            Span<char> moduleFileName = stackalloc char[2048];
                            uint moduleFileNameSize = K32GetModuleFileNameExW(processHandle, hModule, moduleFileName);

                            // Compare the module name with the name of the process image.
                            if (imageFileNameSize == moduleFileNameSize && string.Equals(imageFileName.ToString(), moduleFileName.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                // We found the main module, return it's base address.
                                return hModule;
                            }
                        }
                    }
                    else
                        throw new Win32Exception();
                }

                // If we reach this point, we didn't find the main module...
                return new FreeLibrarySafeHandle();
            }
        }

        /// <summary>
        /// A wrapper around ProcessMemory.Native.32/64.dll's signature_scan() method for scanning for patterns within a process' memory space.
        /// </summary>
#if x64
        public static unsafe IList<nint> NativeSigScan(ushort pid, string pattern, byte alignment = SigScan.SIGSCAN_DEFAULT_ALIGNMENT, ulong startAddress = SigScan.SIGSCAN_DEFAULT_START_ADDRESS, ulong endAddress = SigScan.SIGSCAN_DEFAULT_END_ADDRESS, PAGE_PROTECTION_FLAGS pageProtectionFlags = SigScan.SIGSCAN_DEFAULT_PAGE_PROTECTION_FLAGS)
#else
        public static unsafe IList<nint> NativeSigScan(ushort pid, string pattern, byte alignment = SigScan.SIGSCAN_DEFAULT_ALIGNMENT, uint startAddress = SigScan.SIGSCAN_DEFAULT_START_ADDRESS, uint endAddress = SigScan.SIGSCAN_DEFAULT_END_ADDRESS, PAGE_PROTECTION_FLAGS pageProtectionFlags = SigScan.SIGSCAN_DEFAULT_PAGE_PROTECTION_FLAGS)
#endif
        {
            IList<nint> pointers = new List<nint>();
            using (var processHandle = OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ, false, pid).ToSafeProcessHandle())
            {
                Native.SigScan.SIGSCAN_RESULTS_T results;
                Unsafe.SkipInit(out results);
                try
                {
                    Native.SigScan.SIGSCAN_OPTIONS_T* options = stackalloc Native.SigScan.SIGSCAN_OPTIONS_T[1];
                    options->startAddress = (UIntPtr)startAddress;
                    options->endAddress = (UIntPtr)endAddress;
                    options->alignment = alignment;
                    options->pageProtectionFlags = pageProtectionFlags;
                    results = Native.SigScan.signature_scan(processHandle.DangerousGetHandle(), pattern, options);
                    for (nuint i = 0; i < results.elements; ++i)
                        pointers.Add((nint)results.pointers[i]);
                }
                finally
                {
                    Native.SigScan.free_sigscan_results(&results);
                }
            }
            return pointers;
        }
    }
}
