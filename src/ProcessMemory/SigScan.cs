using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;

namespace ProcessMemory
{
    // TODO: AI-generated (Claude 3.7 Extended (Thinking)). Manually adjusted to use CsWin32-generated types and methods. Re-evaluate and refactor as needed.
    public static unsafe class SigScan
    {
        private const string SIGSCAN_WILDCARD_PATTERN = "??";
        private const nuint SIGSCAN_CHUNK_SIZE = 16 * 1024 * 1024; // 16 MB

        public const nuint SIGSCAN_DEFAULT_ALIGNMENT = 4;
        public const ulong SIGSCAN_DEFAULT_START_ADDRESS = 0x0000000000000000UL;
        public const ulong SIGSCAN_DEFAULT_END_ADDRESS = 0x00007fffffffffffUL;
        public const PAGE_PROTECTION_FLAGS SIGSCAN_DEFAULT_PAGE_PROTECTION_FLAGS = PAGE_PROTECTION_FLAGS.PAGE_READONLY |
            PAGE_PROTECTION_FLAGS.PAGE_READWRITE |
            PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READ |
            PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE;
        public const VIRTUAL_ALLOCATION_TYPE SIGSCAN_DEFAULT_VIRTUAL_ALLOCATION_TYPE = VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT;

        /// <inheritdoc cref="ScanMemory(SafeProcessHandle, ReadOnlySpan<char>)"/>
        /// <paramref name="pid"/>The process identifier to scan.</param>
        public static IList<IntPtr> ScanMemory(
            ushort pid,
            ReadOnlySpan<char> pattern,
            nuint alignment = SIGSCAN_DEFAULT_ALIGNMENT,
            ulong startAddress = SIGSCAN_DEFAULT_START_ADDRESS,
            ulong endAddress = SIGSCAN_DEFAULT_END_ADDRESS,
            PAGE_PROTECTION_FLAGS pageProtectionFlags = SIGSCAN_DEFAULT_PAGE_PROTECTION_FLAGS,
            VIRTUAL_ALLOCATION_TYPE virtualAllocationType = SIGSCAN_DEFAULT_VIRTUAL_ALLOCATION_TYPE)
        {
            using (var safeProcessHandle = OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_VM_OPERATION | PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ | PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION, false, pid).ToSafeProcessHandle())
                return ScanMemory(safeProcessHandle, pattern, alignment, startAddress, endAddress, pageProtectionFlags, virtualAllocationType);
        }

        /// <inheritdoc cref="ScanMemory(SafeProcessHandle, ReadOnlySpan<char>)"/>
        /// <paramref name="processHandle"/>The process' handle as an <seealso cref="IntPtr"/> to scan.</param>
        public static IList<IntPtr> ScanMemory(
            IntPtr processHandle,
            ReadOnlySpan<char> pattern,
            nuint alignment = SIGSCAN_DEFAULT_ALIGNMENT,
            ulong startAddress = SIGSCAN_DEFAULT_START_ADDRESS,
            ulong endAddress = SIGSCAN_DEFAULT_END_ADDRESS,
            PAGE_PROTECTION_FLAGS pageProtectionFlags = SIGSCAN_DEFAULT_PAGE_PROTECTION_FLAGS,
            VIRTUAL_ALLOCATION_TYPE virtualAllocationType = SIGSCAN_DEFAULT_VIRTUAL_ALLOCATION_TYPE)
        {
            using (var safeProcessHandle = processHandle.ToSafeProcessHandle())
                return ScanMemory(safeProcessHandle, pattern, alignment, startAddress, endAddress, pageProtectionFlags, virtualAllocationType);
        }

        /// <inheritdoc cref="ScanMemory(SafeProcessHandle, ReadOnlySpan<char>)"/>
        /// <paramref name="processHandle"/>The process' handle as an <seealso cref="HANDLE"/> to scan.</param>
        public static IList<IntPtr> ScanMemory(
            HANDLE processHandle,
            ReadOnlySpan<char> pattern,
            nuint alignment = SIGSCAN_DEFAULT_ALIGNMENT,
            ulong startAddress = SIGSCAN_DEFAULT_START_ADDRESS,
            ulong endAddress = SIGSCAN_DEFAULT_END_ADDRESS,
            PAGE_PROTECTION_FLAGS pageProtectionFlags = SIGSCAN_DEFAULT_PAGE_PROTECTION_FLAGS,
            VIRTUAL_ALLOCATION_TYPE virtualAllocationType = SIGSCAN_DEFAULT_VIRTUAL_ALLOCATION_TYPE)
        {
            using (var safeProcessHandle = processHandle.ToSafeProcessHandle())
                return ScanMemory(safeProcessHandle, pattern, alignment, startAddress, endAddress, pageProtectionFlags, virtualAllocationType);
        }

        /// <summary>
        /// Scans the memory of a process for a specified pattern and returns the addresses where matches are found.
        /// </summary>
        /// <param name="safeProcessHandle">The process' <seealso cref="SafeProcessHandle"/> to scan.</param>
        /// <param name="pattern">Pattern to search for, e.g., "FF562012????030D" where ?? represents any byte.</param>
        /// <param name="alignment">Whether to align the scan along memory boundries. To disable, set to 0.</param>
        /// <param name="startAddress">The memory address to start scanning from.</param>
        /// <param name="endAddress">The memory address to scan until.</param>
        /// <param name="pageProtectionFlags">The <seealso cref="PAGE_PROTECTION_FLAGS"> to filter on.</param>
        /// <param name="virtualAllocationType">The <seealso cref="VIRTUAL_ALLOCATION_TYPE"/> to filter on.</param>
        /// <returns>An array of <seealso cref="IntPtr"/> addresses where the pattern was found.</returns>
        public static IList<IntPtr> ScanMemory(
            SafeProcessHandle safeProcessHandle,
            ReadOnlySpan<char> pattern,
            nuint alignment = SIGSCAN_DEFAULT_ALIGNMENT,
            ulong startAddress = SIGSCAN_DEFAULT_START_ADDRESS,
            ulong endAddress = SIGSCAN_DEFAULT_END_ADDRESS,
            PAGE_PROTECTION_FLAGS pageProtectionFlags = SIGSCAN_DEFAULT_PAGE_PROTECTION_FLAGS,
            VIRTUAL_ALLOCATION_TYPE virtualAllocationType = SIGSCAN_DEFAULT_VIRTUAL_ALLOCATION_TYPE)
        {
            if (alignment < 0 || !(alignment > 0 && (alignment & (alignment - 1)) == 0))
                throw new ArgumentException("Alignment must be a positive power of 2", nameof(alignment));

            if (safeProcessHandle.IsInvalid)
            {
                int win32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(win32Error, $"The SafeProcessHandle is invalid. Error code: {win32Error}");
            }

            if (!Environment.Is64BitProcess)
                throw new PlatformNotSupportedException($"Only 64-bit (amd64/x64) is supported at this time.");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            IList<IntPtr> results = new List<IntPtr>();

            // Parse the pattern into a byte array and a mask
            (byte[] patternBytes, bool[] maskBytes) = ParsePattern(pattern);

            // Start scanning from address 0
            byte* currentAddress = (byte*)0;

            nuint alignmentMask = alignment > 0 ? (nuint)(alignment - 1) : 0;

            while (true)
            {
                // Query memory region information
                MEMORY_BASIC_INFORMATION memInfo;
                nuint virtualQueryExResult = VirtualQueryEx(safeProcessHandle, currentAddress, out memInfo, (nuint)Unsafe.SizeOf<MEMORY_BASIC_INFORMATION>());

                if (virtualQueryExResult == 0)
                {
                    // No more memory regions to scan
                    break;
                }

                // Move to the next region for the next iteration
                byte* nextAddress = (byte*)memInfo.BaseAddress + memInfo.RegionSize;
                if (nextAddress <= currentAddress)
                {
                    // Address wrapped around, we're done
                    break;
                }
                currentAddress = nextAddress;

                // If we're outside of the start or end range, go to the next block.
                if (currentAddress < (byte*)startAddress || currentAddress > (byte*)endAddress)
                    continue;

                //Check if memory region is committed and readable
                if (memInfo.State != virtualAllocationType || (memInfo.Protect & pageProtectionFlags) == 0)
                    continue;

                // Get the total region size - might be very large
                nuint totalRegionSize = memInfo.RegionSize;

                // We need to read the last (patternBytes.Length - 1) bytes of the previous chunk
                // along with the current chunk to catch patterns that span chunk boundaries
                nuint overlap = (nuint)(patternBytes.Length - 1);

                // Process the region in chunks
                for (nuint offset = 0; offset < totalRegionSize; offset += SIGSCAN_CHUNK_SIZE - overlap)
                {
                    // Calculate the size of this chunk
                    nuint currentChunkSize = (nuint)Math.Min(SIGSCAN_CHUNK_SIZE, totalRegionSize - offset);

                    // Adjust the base address for this chunk

                    byte* currentBaseAddress = (byte*)((nuint)memInfo.BaseAddress + offset);

                    // Read this chunk
                    byte[] buffer = new byte[currentChunkSize];
                    nuint bytesRead;

                    fixed (byte* bufferPtr = buffer)
                    {
                        if (!ReadProcessMemory(safeProcessHandle, currentBaseAddress, bufferPtr, (nuint)buffer.Length, &bytesRead))
                        {
                            // If we can't read this chunk, try the next one
                            continue;
                        }
                    }

                    // Don't scan the overlapping section in subsequent chunks (except for the first chunk)
                    nuint scanStart = (offset > 0) ? overlap : 0;

                    scanStart = (nuint)Math.Min(scanStart, bytesRead - (nuint)patternBytes.Length);

                    // If alignment is required, adjust the scan start to the next aligned position
                    if (alignment > 0 && (((nuint)currentBaseAddress + scanStart) & alignmentMask) != 0)
                    {
                        // Calculate how many bytes to add to reach the next aligned address
                        nuint alignmentAdjustment = alignment - (((nuint)currentBaseAddress + scanStart) & alignmentMask);
                        scanStart += alignmentAdjustment;

                        // If this pushes us beyond valid range, skip this chunk
                        if (scanStart > bytesRead - (nuint)patternBytes.Length)
                        {
                            continue;
                        }
                    }

                    // Scan for pattern matches in this chunk
                    for (nuint i = scanStart; bytesRead >= (nuint)patternBytes.Length && i <= bytesRead - (nuint)patternBytes.Length;)
                    {
                        // If using alignment, check if the current position is aligned
                        if (alignment > 0)
                        {
                            // If not aligned, move to next aligned position
                            if ((((nuint)currentBaseAddress + i) & alignmentMask) != 0)
                            {
                                // Move to the next alignment boundary
                                i += alignment - (((nuint)currentBaseAddress + i) & alignmentMask);
                                continue;
                            }
                        }

                        bool found = true;
                        for (nuint j = 0; j < (nuint)patternBytes.Length; ++j)
                        {
                            // Skip checking if mask byte is a wildcard
                            if (maskBytes[j])
                                continue;

                            if (i + j >= bytesRead)
                            {
                                found = false;
                                break;
                            }

                            if (buffer[i + j] != patternBytes[j])
                            {
                                found = false;
                                break;
                            }
                        }

                        if (found)
                        {
                            // Pattern found, add the address to results
                            IntPtr result = new IntPtr(currentBaseAddress + i);
                            Console.WriteLine($"Pattern found at 0x{result:X16}.");
                            results.Add(result);
                        }

                        // Increment by alignment if required, otherwise just move by 1
                        i += alignment > 0 ? alignment : 1;
                    }

                    // If we read less than we requested, we've hit the end of the region
                    if (bytesRead < currentChunkSize)
                        break;
                }
            }

            sw.Stop();
            Trace.WriteLine($"{nameof(SigScan)}.{nameof(ScanMemory)}(...) completed after {sw.Elapsed}.");
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

            {
                ReadOnlySpan<char> byteString;
                ReadOnlySpan<char> wildcardPattern = SIGSCAN_WILDCARD_PATTERN.AsSpan();
                for (int i = 0; i < length; i++)
                {
                    byteString = pattern.Slice(i * 2, 2);

                    if (MemoryExtensions.Equals(byteString, wildcardPattern, StringComparison.OrdinalIgnoreCase))
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
            }

            return (patternBytes, maskBytes);
        }
    }
}
