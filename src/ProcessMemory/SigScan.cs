using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;

namespace ProcessMemory
{
    // TODO: AI-generated (Claude 3.7 Extended (Thinking)). Manually adjusted to use CsWin32-generated types and methods. Re-evaluate and refactor as needed.
    public static unsafe class SigScan
    {
        /// <summary>
        /// Scans the memory of a process for a specified pattern and returns the addresses where matches are found.
        /// </summary>
        /// <param name="pid">Process ID to scan</param>
        /// <param name="pattern">Pattern to search for, e.g., "FF562012????030D" where ? represents any byte</param>
        /// <returns>Array of IntPtr addresses where the pattern was found</returns>
        public static IList<IntPtr> ScanMemory(ushort pid, ReadOnlySpan<char> pattern)
        {
            IList<IntPtr> results = new List<IntPtr>();
            using (var processHandle = OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_VM_OPERATION | PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ | PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION, false, pid).ToSafeProcessHandle())
            {
                if (processHandle.IsInvalid)
                {
                    int win32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(win32Error, $"Failed to open process with PID {pid}. Error code: {win32Error}");
                }

                // Parse the pattern into a byte array and a mask
                (byte[] patternBytes, bool[] maskBytes) = ParsePattern(pattern);

                // Start scanning from address 0
                void* currentAddress = (void*)0;

                while (true)
                {
                    // Query memory region information
                    MEMORY_BASIC_INFORMATION memInfo;
                    nuint result = VirtualQueryEx(processHandle, currentAddress, out memInfo, (nuint)Unsafe.SizeOf<MEMORY_BASIC_INFORMATION>());

                    if (result == 0)
                    {
                        // No more memory regions to scan
                        break;
                    }

                    // Move to the next region for the next iteration
                    void* nextAddress = Unsafe.Add<byte>(memInfo.BaseAddress, (int)memInfo.RegionSize);
                    if (nextAddress <= currentAddress)
                    {
                        // Address wrapped around, we're done
                        break;
                    }
                    currentAddress = nextAddress;

                    // Check if memory region is committed and readable
                    if (memInfo.State != VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT ||
                        (memInfo.Protect & (PAGE_PROTECTION_FLAGS.PAGE_READONLY |
                                                  PAGE_PROTECTION_FLAGS.PAGE_READWRITE |
                                                  PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READ |
                                                  PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE)) == 0)
                    {
                        continue;
                    }

                    // Cap the read size to avoid huge allocations
                    nuint regionSize = memInfo.RegionSize;
                    if (regionSize > uint.MaxValue)
                    {
                        regionSize = uint.MaxValue;
                    }

                    // Read the memory region
                    byte[] buffer = new byte[(int)regionSize];
                    nuint bytesRead = 0;
                    fixed (byte* bufferPtr = buffer)
                    {
                        if (ReadProcessMemory(processHandle, memInfo.BaseAddress, bufferPtr, (nuint)buffer.Length, &bytesRead).Value != 0)
                        {
                            // Failed to read this memory region, skip it
                            continue;
                        }
                    }

                    // Scan for pattern matches in this memory region
                    for (nuint i = 0; i <= bytesRead - (nuint)patternBytes.Length; ++i)
                    {
                        bool found = true;
                        for (nuint j = 0; j < (nuint)patternBytes.Length; ++j)
                        {
                            // Skip checking if mask byte is a wildcard
                            if (maskBytes[j])
                                continue;

                            if (buffer[i + j] != patternBytes[j])
                            {
                                found = false;
                                break;
                            }
                        }

                        if (found)
                        {
                            // Pattern found, add the address to results
                            results.Add((IntPtr)Unsafe.Add<byte>(memInfo.BaseAddress, (int)i));
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Parses a pattern string into a byte array and a mask array.
        /// </summary>
        /// <param name="pattern">Pattern string like "FF562012????030D"</param>
        /// <returns>Tuple containing the byte array and mask array</returns>
        private static (byte[] patternBytes, bool[] maskBytes) ParsePattern(ReadOnlySpan<char> pattern)
        {
            if (pattern.IsEmpty)
                throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

            if (pattern.Length % 2 != 0)
                throw new ArgumentException("Pattern length must be even", nameof(pattern));

            int length = pattern.Length / 2;
            byte[] patternBytes = new byte[length];
            bool[] maskBytes = new bool[length]; // true means wildcard, false means exact match

            for (int i = 0; i < length; i++)
            {
                ReadOnlySpan<char> byteString = pattern.Slice(i * 2, 2);

                if (byteString == "??")
                {
                    // This is a wildcard byte
                    patternBytes[i] = 0; // Placeholder value, won't be used
                    maskBytes[i] = true;
                }
                else
                {
                    try
                    {
                        // This is an exact match byte
                        patternBytes[i] = Convert.ToByte(byteString.ToString(), 16);
                        maskBytes[i] = false;
                    }
                    catch (FormatException)
                    {
                        throw new ArgumentException($"Invalid hex value in pattern: {byteString.ToString()}", nameof(pattern));
                    }
                }
            }

            return (patternBytes, maskBytes);
        }
    }
}
