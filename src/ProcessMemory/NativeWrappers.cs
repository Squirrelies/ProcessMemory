using System;
using System.ComponentModel;
using System.Threading;
using static Windows.Win32.PInvoke;
using Windows.Win32.System.Threading;
using Windows.Win32.Foundation;
using Windows.Win32.System.ProcessStatus;

namespace ProcessMemory
{
    public static class NativeWrappers
    {
        public static bool GetProcessWoW64(uint pid)
        {
            HANDLE processHandle = OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION, false, pid);
            try
            {
                return GetProcessWoW64(processHandle);
            }
            finally
            {
                CloseHandle(processHandle);
            }
        }

        public unsafe static bool GetProcessWoW64(HANDLE processHandle)
        {
            BOOL returnValue = new BOOL();

            if (!IsWow64Process(processHandle, &returnValue))
                throw new Win32Exception();

            return returnValue;
        }

        public unsafe static string? GetProcessPath(uint pid)
        {
            HANDLE processHandle = OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION, false, pid);

            try
            {
                // Query process image name.
                char* imageFileNamePtr = stackalloc char[2048];
                PWSTR imageFileName = new PWSTR(imageFileNamePtr);
                uint imageFileNameSize = (uint)imageFileName.Length;
                if (QueryFullProcessImageNameW(processHandle, PROCESS_NAME_FORMAT.PROCESS_NAME_WIN32, imageFileName, &imageFileNameSize))
                    return imageFileName.ToString();
                else
                    return null;
            }
            finally
            {
                CloseHandle(processHandle);
            }
        }

        public unsafe static HINSTANCE GetProcessBaseAddress(uint pid, ENUM_PROCESS_MODULES_EX_FLAGS moduleTypes = ENUM_PROCESS_MODULES_EX_FLAGS.LIST_MODULES_ALL)
        {
            HANDLE processHandle = OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ, false, pid);

            try
            {
                // Query process image name.
                char* imageFileNamePtr = stackalloc char[2048];
                PWSTR imageFileName = new PWSTR(imageFileNamePtr);
                uint imageFileNameSize = (uint)imageFileName.Length;
                if (QueryFullProcessImageNameW(processHandle, PROCESS_NAME_FORMAT.PROCESS_NAME_WIN32, imageFileName, &imageFileNameSize))
                {
                    // Query process handle's modules.
                    HINSTANCE[] hModules = new HINSTANCE[1024];
                    uint cb = (uint)(sizeof(HINSTANCE) * 1024U);
                    uint lpcbNeeded = 0;
                    BOOL enumProcessModulesExReturn = new BOOL();
                    fixed (HINSTANCE* lphModule = hModules)
                    {
                        enumProcessModulesExReturn = K32EnumProcessModulesEx(processHandle, lphModule, cb, &lpcbNeeded, moduleTypes);

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
                                enumProcessModulesExReturn = K32EnumProcessModulesEx(processHandle, lphModule, cb, &lpcbNeeded, moduleTypes);
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
                        uint moduleCount = lpcbNeeded / sizeof(long);
                        for (uint i = 0; i < moduleCount; i++)
                        {
                            char* moduleFileNamePtr = stackalloc char[2048];
                            PWSTR moduleFileName = new PWSTR(imageFileNamePtr);
                            uint moduleFileNameSize = K32GetModuleFileNameExW(processHandle, hModules[i], moduleFileName, (uint)moduleFileName.Length);

                            // Compare the module name with the name of the process image.
                            if (imageFileNameSize == moduleFileNameSize && string.Equals(imageFileName.ToString(), moduleFileName.ToString(), StringComparison.InvariantCultureIgnoreCase))
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
                return new HINSTANCE();
            }
            finally
            {
                CloseHandle(processHandle);
            }
        }
    }
}
