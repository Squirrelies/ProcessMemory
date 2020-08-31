using System;
using System.ComponentModel;
using System.Threading;
using static ProcessMemory.Common.PInvoke;

namespace ProcessMemory.Common
{
    public static class NativeWrappers
    {
        public static bool GetProcessWoW64(int pid)
        {
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.QueryInformation, false, pid);
            try
            {
                return GetProcessWoW64(processHandle);
            }
            finally
            {
                CloseHandle(processHandle);
            }
        }

        public static bool GetProcessWoW64(IntPtr processHandle)
        {
            bool returnValue = false;

            if (!IsWow64Process(processHandle, ref returnValue))
                throw new Win32Exception();

            return returnValue;
        }

        public static string GetProcessPath(int pid)
        {
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, pid);

            try
            {
                // Query process image name.
                char[] imageFileName = new char[2048];
                int imageFileNameSize = imageFileName.Length;
                if (QueryFullProcessImageNameW(processHandle, 0, imageFileName, ref imageFileNameSize))
                    return new string(imageFileName, 0, imageFileNameSize);
                else
                    return null;
            }
            finally
            {
                CloseHandle(processHandle);
            }
        }

        public unsafe static IntPtr GetProcessBaseAddress(int pid, ListModules moduleTypes = ListModules.LIST_MODULES_ALL)
        {
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.QueryLimitedInformation | ProcessAccessFlags.VirtualMemoryRead, false, pid);

            try
            {
                // Query process image name.
                char[] imageFileName = new char[2048];
                int imageFileNameSize = imageFileName.Length;
                if (QueryFullProcessImageNameW(processHandle, 0, imageFileName, ref imageFileNameSize))
                {
                    // Query process handle's modules.
                    IntPtr[] hModules = new IntPtr[1024];
                    int cb = 1024 * sizeof(long);
                    int lpcbNeeded = 0;
                    bool enumProcessModulesExReturn = false;
                    fixed (IntPtr* lphModule = hModules)
                    {
                        enumProcessModulesExReturn = EnumProcessModulesEx(processHandle, lphModule, cb, out lpcbNeeded, moduleTypes);

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
                                enumProcessModulesExReturn = EnumProcessModulesEx(processHandle, lphModule, cb, out lpcbNeeded, moduleTypes);
                                if (enumProcessModulesExReturn)
                                    break; // If we succeeded, break out of the loop early.

                                // Sleep for 1ms then try again.
                                Thread.Sleep(1);
                            }
                        }
                    }

                    // If we successfully retrieved an array of process modules, enumerate through them to find the main module.
                    if (enumProcessModulesExReturn)
                    {
                        int moduleCount = lpcbNeeded / sizeof(long);
                        for (int i = 0; i < moduleCount; i++)
                        {
                            char[] moduleFileName = new char[2048];
                            int moduleFileNameSize = GetModuleFileNameEx(processHandle, hModules[i], moduleFileName, moduleFileName.Length);

                            // Compare the module name with the name of the process image.
                            if (imageFileNameSize == moduleFileNameSize && string.Equals(new string(imageFileName, 0, imageFileNameSize), new string(moduleFileName, 0, moduleFileNameSize), StringComparison.InvariantCultureIgnoreCase))
                            {
                                // We found the main module, return it's base address.
                                return hModules[i];
                            }
                        }
                    }
                    else
                        throw new Win32Exception();
                }

                // If we reach this point, we didn't find the main module...
                return IntPtr.Zero;
            }
            finally
            {
                CloseHandle(processHandle);
            }
        }
    }
}
