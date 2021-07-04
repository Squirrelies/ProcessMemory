using System;
using System.Collections.Generic;
using static ProcessMemory.PInvoke;

namespace ProcessMemory
{
    public unsafe class ProcessMemoryHandler : IDisposable
    {
        public readonly IntPtr ProcessHandle = IntPtr.Zero;

        public ProcessMemoryHandler(int pid, bool readOnly = true)
        {
            ProcessHandle = OpenProcess((readOnly) ? ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryRead : ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryRead | ProcessAccessFlags.VirtualMemoryWrite, false, pid);
        }

        public bool ProcessRunning
        {
            get
            {
                int exitCode = 0;
                return GetExitCodeProcess(ProcessHandle, ref exitCode) && exitCode == 259;
            }
        }

        public int ProcessExitCode
        {
            get
            {
                int exitCode = 0;
                GetExitCodeProcess(ProcessHandle, ref exitCode);
                return exitCode;
            }
        }

#if x64
        private static readonly int memoryBasicInfoSize = sizeof(MEMORY_BASIC_INFORMATION64);
#else
        private static readonly int memoryBasicInfoSize = sizeof(MEMORY_BASIC_INFORMATION32);
#endif

        private string GetMemoryProtectFlags(IntPtr offset)
        {
            try
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
#if x64
                MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
                VirtualQueryEx(ProcessHandle, offset, out memBasicInfo, memoryBasicInfoSize);

                sb.AppendLine("[MEMORY_BASIC_INFORMATION64]");
                sb.AppendFormat("BaseAddress: {0}\r\n", memBasicInfo.BaseAddress);
                sb.AppendFormat("AllocationBase: {0}\r\n", memBasicInfo.AllocationBase);
                sb.AppendFormat("AllocationProtect: {0}\r\n", memBasicInfo.AllocationProtect);
                sb.AppendFormat("__alignment1: {0}\r\n", memBasicInfo.__alignment1);
                sb.AppendFormat("RegionSize: {0}\r\n", memBasicInfo.RegionSize);
                sb.AppendFormat("State: {0}\r\n", memBasicInfo.State);
                sb.AppendFormat("Protect: {0}\r\n", memBasicInfo.Protect);
                sb.AppendFormat("Type: {0}\r\n", memBasicInfo.Type);
                sb.AppendFormat("__alignment2: {0}\r\n", memBasicInfo.__alignment2);
#else
                MEMORY_BASIC_INFORMATION32 memBasicInfo = new MEMORY_BASIC_INFORMATION32();
                VirtualQueryEx(ProcessHandle, offset, out memBasicInfo, memoryBasicInfoSize);
                
                sb.AppendLine("[MEMORY_BASIC_INFORMATION32]");
                sb.AppendFormat("BaseAddress: {0}\r\n", memBasicInfo.BaseAddress);
                sb.AppendFormat("AllocationBase: {0}\r\n", memBasicInfo.AllocationBase);
                sb.AppendFormat("AllocationProtect: {0}\r\n", memBasicInfo.AllocationProtect);
                sb.AppendFormat("RegionSize: {0}\r\n", memBasicInfo.RegionSize);
                sb.AppendFormat("State: {0}\r\n", memBasicInfo.State);
                sb.AppendFormat("Protect: {0}\r\n", memBasicInfo.Protect);
                sb.AppendFormat("Type: {0}\r\n", memBasicInfo.Type);
#endif
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return string.Format("[GetMemoryProtectFlags EXCEPTION: {0}]", ex.ToString());
            }
        }

        public bool TrySetByteArrayAt(IntPtr offset, int size, IntPtr result) => WriteProcessMemory(ProcessHandle, offset, result, size, out IntPtr bytesWritten);

        public bool TrySetByteArrayAt(IntPtr offset, int size, void* result) => WriteProcessMemory(ProcessHandle, offset, result, size, out IntPtr bytesWritten);

#if x64
        public bool TrySetByteArrayAt(long* offset, int size, IntPtr result)
#else
        public bool TrySetByteArrayAt(int* offset, int size, IntPtr result)
#endif
        {
            return WriteProcessMemory(ProcessHandle, offset, result, size, out IntPtr bytesWritten);
        }

#if x64
        public bool TrySetByteArrayAt(long* offset, int size, void* result)
#else
        public bool TrySetByteArrayAt(int* offset, int size, void* result)
#endif
        {
            return WriteProcessMemory(ProcessHandle, offset, result, size, out IntPtr bytesWritten);
        }

        public bool TryGetByteArrayAt(IntPtr offset, int size, IntPtr result) => ReadProcessMemory(ProcessHandle, offset, result, size, out IntPtr bytesRead);

        public bool TryGetByteArrayAt(IntPtr offset, int size, void* result) => ReadProcessMemory(ProcessHandle, offset, result, size, out IntPtr bytesRead);

#if x64
        public bool TryGetByteArrayAt(long* offset, int size, IntPtr result)
#else
        public bool TryGetByteArrayAt(int* offset, int size, IntPtr result)
#endif
        {
            IntPtr bytesRead = IntPtr.Zero;
            return ReadProcessMemory(ProcessHandle, offset, result, size, out bytesRead);
        }

#if x64
        public bool TryGetByteArrayAt(long* offset, int size, void* result)
#else
        public bool TryGetByteArrayAt(int* offset, int size, void* result)
#endif
        {
            IntPtr bytesRead = IntPtr.Zero;
            return ReadProcessMemory(ProcessHandle, offset, result, size, out bytesRead);
        }

        public T GetAt<T>(IntPtr offset) where T : unmanaged
        {
            T* rv = stackalloc T[1];
            if (ReadProcessMemory(ProcessHandle, offset, rv, sizeof(T), out IntPtr bytesRead))
                return *rv;
            else
                return default;
        }

#if x64
        public T GetAt<T>(long* offset) where T : unmanaged
#else
        public T GetAt<T>(int* offset) where T : unmanaged
#endif
        {
            T* rv = stackalloc T[1];
            if (ReadProcessMemory(ProcessHandle, offset, rv, sizeof(T), out IntPtr bytesRead))
                return *rv;
            else
                return default;
        }

        public int SetAt<T>(IntPtr offset, T value) where T : unmanaged
        {
            return WriteProcessMemory(ProcessHandle, offset, &value, sizeof(T), out IntPtr bytesWritten) ? bytesWritten.ToInt32() : 0;
        }

#if x64
        public int SetAt<T>(long* offset, T value) where T : unmanaged
#else
        public int SetAt<T>(int* offset, ref T value) where T : unmanaged
#endif
        {
            return WriteProcessMemory(ProcessHandle, offset, &value, sizeof(T), out IntPtr bytesWritten) ? bytesWritten.ToInt32() : 0;
        }

        public bool TryGetAt<T>(IntPtr offset, ref T value) where T : unmanaged
        {
            fixed (T* pointer = &value)
            {
                return ReadProcessMemory(ProcessHandle, offset, pointer, sizeof(T), out IntPtr bytesRead);
            }
        }

#if x64
        public bool TryGetAt<T>(long* offset, ref T value) where T : unmanaged
#else
        public bool TryGetAt<T>(int* offset, ref T value) where T : unmanaged
#endif
        {
            fixed (T* pointer = &value)
            {
                return ReadProcessMemory(ProcessHandle, offset, pointer, sizeof(T), out IntPtr bytesRead);
            }
        }

        public bool TrySetAt<T>(IntPtr offset, ref T value) where T : unmanaged
        {
            fixed (T* pointer = &value)
            {
                return WriteProcessMemory(ProcessHandle, offset, pointer, sizeof(T), out IntPtr bytesRead);
            }
        }

#if x64
        public bool TrySetAt<T>(long* offset, ref T value) where T : unmanaged
#else
        public bool TrySetAt<T>(int* offset, ref T value) where T : unmanaged
#endif
        {
            fixed (T* pointer = &value)
            {
                return WriteProcessMemory(ProcessHandle, offset, pointer, sizeof(T), out IntPtr bytesRead);
            }
        }

        public sbyte GetSByteAt(IntPtr offset) => GetAt<sbyte>(offset);
        public byte GetByteAt(IntPtr offset) => GetAt<byte>(offset);
        public short GetShortAt(IntPtr offset) => GetAt<short>(offset);
        public ushort GetUShortAt(IntPtr offset) => GetAt<ushort>(offset);
        //public Int24 GetInt24At(IntPtr offset) => GetAt<Int24>(offset);
        //public UInt24 GetUInt24At(IntPtr offset) => GetAt<UInt24>(offset);
        public int GetIntAt(IntPtr offset) => GetAt<int>(offset);
        public uint GetUIntAt(IntPtr offset) => GetAt<uint>(offset);
        public long GetLongAt(IntPtr offset) => GetAt<long>(offset);
        public ulong GetULongAt(IntPtr offset) => GetAt<ulong>(offset);
        public float GetFloatAt(IntPtr offset) => GetAt<float>(offset);
        public double GetDoubleAt(IntPtr offset) => GetAt<double>(offset);

        public int SetSByteAt(IntPtr offset, sbyte value) => SetAt(offset, value);
        public int SetByteAt(IntPtr offset, byte value) => SetAt(offset, value);
        public int SetShortAt(IntPtr offset, short value) => SetAt(offset, value);
        public int SetUShortAt(IntPtr offset, ushort value) => SetAt(offset, value);
        //public int SetInt24At(IntPtr offset, Int24 value) => SetAt(offset, value);
        //public int SetUInt24At(IntPtr offset, UInt24 value) => SetAt(offset, value);
        public int SetIntAt(IntPtr offset, int value) => SetAt(offset, value);
        public int SetUIntAt(IntPtr offset, uint value) => SetAt(offset, value);
        public int SetLongAt(IntPtr offset, long value) => SetAt(offset, value);
        public int SetULongAt(IntPtr offset, ulong value) => SetAt(offset, value);
        public int SetFloatAt(IntPtr offset, float value) => SetAt(offset, value);
        public int SetDoubleAt(IntPtr offset, double value) => SetAt(offset, value);

        public bool TryGetSByteAt(IntPtr address, ref sbyte result) => TryGetAt(address, ref result);
        public bool TryGetByteAt(IntPtr address, ref byte result) => TryGetAt(address, ref result);
        public bool TryGetShortAt(IntPtr address, ref short result) => TryGetAt(address, ref result);
        public bool TryGetUShortAt(IntPtr address, ref ushort result) => TryGetAt(address, ref result);
        //public bool TryGetInt24At(IntPtr address, ref Int24 result) => TryGetAt(address, ref result);
        //public bool TryGetUInt24At(IntPtr address, ref UInt24 result) => TryGetAt(address, ref result);
        public bool TryGetIntAt(IntPtr address, ref int result) => TryGetAt(address, ref result);
        public bool TryGetUIntAt(IntPtr address, ref uint result) => TryGetAt(address, ref result);
        public bool TryGetLongAt(IntPtr address, ref long result) => TryGetAt(address, ref result);
        public bool TryGetULongAt(IntPtr address, ref ulong result) => TryGetAt(address, ref result);
        public bool TryGetFloatAt(IntPtr address, ref float result) => TryGetAt(address, ref result);
        public bool TryGetDoubleAt(IntPtr address, ref double result) => TryGetAt(address, ref result);

        public bool TryGetSByteAt(IntPtr address, sbyte* result) => TryGetAt(address, ref *result);
        public bool TryGetByteAt(IntPtr address, byte* result) => TryGetAt(address, ref *result);
        public bool TryGetShortAt(IntPtr address, short* result) => TryGetAt(address, ref *result);
        public bool TryGetUShortAt(IntPtr address, ushort* result) => TryGetAt(address, ref *result);
        //public bool TryGetInt24At(IntPtr address, Int24* result) => TryGetAt(address, ref *result);
        //public bool TryGetUInt24At(IntPtr address, UInt24* result) => TryGetAt(address, ref *result);
        public bool TryGetIntAt(IntPtr address, int* result) => TryGetAt(address, ref *result);
        public bool TryGetUIntAt(IntPtr address, uint* result) => TryGetAt(address, ref *result);
        public bool TryGetLongAt(IntPtr address, long* result) => TryGetAt(address, ref *result);
        public bool TryGetULongAt(IntPtr address, ulong* result) => TryGetAt(address, ref *result);
        public bool TryGetFloatAt(IntPtr address, float* result) => TryGetAt(address, ref *result);
        public bool TryGetDoubleAt(IntPtr address, double* result) => TryGetAt(address, ref *result);

#if x64
        public bool TryGetSByteAt(long* address, sbyte* result) => TryGetAt(address, ref *result);
        public bool TryGetByteAt(long* address, byte* result) => TryGetAt(address, ref *result);
        public bool TryGetShortAt(long* address, short* result) => TryGetAt(address, ref *result);
        public bool TryGetUShortAt(long* address, ushort* result) => TryGetAt(address, ref *result);
        //public bool TryGetInt24At(long* address, Int24* result) => TryGetAt(address, ref *result);
        //public bool TryGetUInt24At(long* address, UInt24* result) => TryGetAt(address, ref *result);
        public bool TryGetIntAt(long* address, int* result) => TryGetAt(address, ref *result);
        public bool TryGetUIntAt(long* address, uint* result) => TryGetAt(address, ref *result);
        public bool TryGetLongAt(long* address, long* result) => TryGetAt(address, ref *result);
        public bool TryGetULongAt(long* address, ulong* result) => TryGetAt(address, ref *result);
        public bool TryGetFloatAt(long* address, float* result) => TryGetAt(address, ref *result);
        public bool TryGetDoubleAt(long* address, double* result) => TryGetAt(address, ref *result);
#else
        public bool TryGetSByteAt(int* address, sbyte* result) => TryGetAt(address, ref *result);
        public bool TryGetByteAt(int* address, byte* result) => TryGetAt(address, ref *result);
        public bool TryGetShortAt(int* address, short* result) => TryGetAt(address, ref *result);
        public bool TryGetUShortAt(int* address, ushort* result) => TryGetAt(address, ref *result);
        //public bool TryGetInt24At(int* address, Int24* result) => TryGetAt(address, ref *result);
        //public bool TryGetUInt24At(int* address, UInt24* result) => TryGetAt(address, ref *result);
        public bool TryGetIntAt(int* address, int* result) => TryGetAt(address, ref *result);
        public bool TryGetUIntAt(int* address, uint* result) => TryGetAt(address, ref *result);
        public bool TryGetLongAt(int* address, long* result) => TryGetAt(address, ref *result);
        public bool TryGetULongAt(int* address, ulong* result) => TryGetAt(address, ref *result);
        public bool TryGetFloatAt(int* address, float* result) => TryGetAt(address, ref *result);
        public bool TryGetDoubleAt(int* address, double* result) => TryGetAt(address, ref *result);
#endif

        public static int FindIndexOf(byte[] array, int start, byte[] sequence)
        {
            int end = array.Length - sequence.Length; // past here no match is possible
            byte firstByte = sequence[0]; // cached to tell compiler there's no aliasing

            while (start < end)
            {
                // Check first byte.
                if (array[start] == firstByte && sequence.Length == 1)
                {
                    return start; // Found a winner!
                }
                else if (array[start] == firstByte && sequence.Length > 1)
                {
                    // First byte matched, perform internal scanning.
                    for (int offset = 1; offset < sequence.Length; ++offset)
                    {
                        if (array[start + offset] != sequence[offset])
                        {
                            break; // Mismatch, eject and resume normal searching.
                        }
                        else if (offset == sequence.Length - 1)
                        {
                            return start; // Found a winner!
                        }
                    }
                }
                ++start;
            }

            return -1; // Failed to find a winner.
        }

        public static IEnumerable<int> FindIndexesOf(byte[] array, int start, byte[] sequence)
        {
            int end = array.Length - sequence.Length;
            int indexOf = 0;
            while (start < end && (indexOf = FindIndexOf(array, start, sequence)) != -1)
            {
                start = indexOf + 1;
                yield return indexOf;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                CloseHandle(ProcessHandle);

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Memory() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
