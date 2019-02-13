using System;
using static ProcessMemory.PInvoke;

namespace ProcessMemory
{
    public static class NativeWrappers
    {
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
                        enumProcessModulesExReturn = EnumProcessModulesEx(processHandle, lphModule, cb, out lpcbNeeded, moduleTypes);

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
