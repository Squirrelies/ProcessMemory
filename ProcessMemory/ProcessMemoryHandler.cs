using ProcessMemory.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static ProcessMemory.PInvoke;

namespace ProcessMemory
{
    public unsafe class ProcessMemoryHandler : IDisposable
    {
        public readonly IntPtr ProcessHandle = IntPtr.Zero;

        protected ProcessMemoryHandler(int pid, bool readOnly = true)
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

        public byte[] GetByteArrayAt(IntPtr offset, int size)
        {
            byte[] returnValue = new byte[size];
            IntPtr bytesRead = IntPtr.Zero;
            if (!ReadProcessMemory(ProcessHandle, offset, returnValue, size, out bytesRead))
            {
                int win32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(win32Error, string.Format("{0}: {1}: {2}", ((Win32Error)win32Error).ToString(), bytesRead.ToInt32(), GetMemoryProtectFlags(offset)));
            }

            return returnValue;
        }

        public int SetByteArrayAt(IntPtr offset, byte[] data)
        {
            IntPtr bytesWritten = IntPtr.Zero;
            if (!WriteProcessMemory(ProcessHandle, offset, data, data.Length, out bytesWritten))
            {
                int win32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(win32Error, ((Win32Error)win32Error).ToString());
            }

            return bytesWritten.ToInt32();
        }

        public bool TryGetByteArrayAt(IntPtr offset, int size, IntPtr result)
        {
            IntPtr bytesRead = IntPtr.Zero;
            return ReadProcessMemory(ProcessHandle, offset, result, size, out bytesRead);
        }

        public bool TryGetByteArrayAt(IntPtr offset, int size, void* result)
        {
            IntPtr bytesRead = IntPtr.Zero;
            return ReadProcessMemory(ProcessHandle, offset, result, size, out bytesRead);
        }

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

        public sbyte GetSByteAt(IntPtr offset) => (sbyte)GetByteArrayAt(offset, 1)[0];
        public byte GetByteAt(IntPtr offset) => GetByteArrayAt(offset, 1)[0];
        public short GetShortAt(IntPtr offset) => HighPerfBitConverter.ToInt16(GetByteArrayAt(offset, 2), 0);
        public ushort GetUShortAt(IntPtr offset) => HighPerfBitConverter.ToUInt16(GetByteArrayAt(offset, 2), 0);
        public Int24 GetInt24At(IntPtr offset) => new Int24(GetByteArrayAt(offset, 3), 0);
        public UInt24 GetUInt24At(IntPtr offset) => new UInt24(GetByteArrayAt(offset, 3), 0);
        public int GetIntAt(IntPtr offset) => HighPerfBitConverter.ToInt32(GetByteArrayAt(offset, 4), 0);
        public uint GetUIntAt(IntPtr offset) => HighPerfBitConverter.ToUInt32(GetByteArrayAt(offset, 4), 0);
        public long GetLongAt(IntPtr offset) => HighPerfBitConverter.ToInt64(GetByteArrayAt(offset, 8), 0);
        public ulong GetULongAt(IntPtr offset) => HighPerfBitConverter.ToUInt64(GetByteArrayAt(offset, 8), 0);
        public float GetFloatAt(IntPtr offset) => BitConverter.ToSingle(GetByteArrayAt(offset, 4), 0);
        public double GetDoubleAt(IntPtr offset) => BitConverter.ToDouble(GetByteArrayAt(offset, 8), 0);

        public int SetSByteAt(IntPtr offset, sbyte value) => SetByteArrayAt(offset, new byte[1] { (byte)value });
        public int SetByteAt(IntPtr offset, byte value) => SetByteArrayAt(offset, new byte[1] { value });
        public int SetShortAt(IntPtr offset, short value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetUShortAt(IntPtr offset, ushort value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetInt24At(IntPtr offset, Int24 value) => SetByteArrayAt(offset, value.GetBytes());
        public int SetUInt24At(IntPtr offset, UInt24 value) => SetByteArrayAt(offset, value.GetBytes());
        public int SetIntAt(IntPtr offset, int value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetUIntAt(IntPtr offset, uint value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetLongAt(IntPtr offset, long value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetULongAt(IntPtr offset, ulong value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetFloatAt(IntPtr offset, float value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetDoubleAt(IntPtr offset, double value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));

        public bool TryGetSByteAt(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 1, result);
        public bool TryGetByteAt(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 1, result);
        public bool TryGetShortAt(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 2, result);
        public bool TryGetUShortAt(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 2, result);
        public bool TryGetInt24At(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 3, result);
        public bool TryGetUInt24At(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 3, result);
        public bool TryGetIntAt(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetUIntAt(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetLongAt(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 8, result);
        public bool TryGetULongAt(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 8, result);
        public bool TryGetFloatAt(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetDoubleAt(IntPtr address, IntPtr result) => TryGetByteArrayAt(address, 8, result);

        public bool TryGetSByteAt(IntPtr address, sbyte* result) => TryGetByteArrayAt(address, 1, result);
        public bool TryGetByteAt(IntPtr address, byte* result) => TryGetByteArrayAt(address, 1, result);
        public bool TryGetShortAt(IntPtr address, short* result) => TryGetByteArrayAt(address, 2, result);
        public bool TryGetUShortAt(IntPtr address, ushort* result) => TryGetByteArrayAt(address, 2, result);
        public bool TryGetInt24At(IntPtr address, byte* result) => TryGetByteArrayAt(address, 3, result);
        public bool TryGetUInt24At(IntPtr address, byte* result) => TryGetByteArrayAt(address, 3, result);
        public bool TryGetIntAt(IntPtr address, int* result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetUIntAt(IntPtr address, uint* result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetLongAt(IntPtr address, long* result) => TryGetByteArrayAt(address, 8, result);
        public bool TryGetULongAt(IntPtr address, ulong* result) => TryGetByteArrayAt(address, 8, result);
        public bool TryGetFloatAt(IntPtr address, float* result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetDoubleAt(IntPtr address, double* result) => TryGetByteArrayAt(address, 8, result);

#if x64
        public bool TryGetSByteAt(long* address, sbyte* result) => TryGetByteArrayAt(address, 1, result);
        public bool TryGetByteAt(long* address, byte* result) => TryGetByteArrayAt(address, 1, result);
        public bool TryGetShortAt(long* address, short* result) => TryGetByteArrayAt(address, 2, result);
        public bool TryGetUShortAt(long* address, ushort* result) => TryGetByteArrayAt(address, 2, result);
        public bool TryGetInt24At(long* address, byte* result) => TryGetByteArrayAt(address, 3, result);
        public bool TryGetUInt24At(long* address, byte* result) => TryGetByteArrayAt(address, 3, result);
        public bool TryGetIntAt(long* address, int* result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetUIntAt(long* address, uint* result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetLongAt(long* address, long* result) => TryGetByteArrayAt(address, 8, result);
        public bool TryGetULongAt(long* address, ulong* result) => TryGetByteArrayAt(address, 8, result);
        public bool TryGetFloatAt(long* address, float* result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetDoubleAt(long* address, double* result) => TryGetByteArrayAt(address, 8, result);
#else
        public bool TryGetSByteAt(int* address, sbyte* result) => TryGetByteArrayAt(address, 1, result);
        public bool TryGetByteAt(int* address, byte* result) => TryGetByteArrayAt(address, 1, result);
        public bool TryGetShortAt(int* address, short* result) => TryGetByteArrayAt(address, 2, result);
        public bool TryGetUShortAt(int* address, ushort* result) => TryGetByteArrayAt(address, 2, result);
        public bool TryGetInt24At(int* address, byte* result) => TryGetByteArrayAt(address, 3, result);
        public bool TryGetUInt24At(int* address, byte* result) => TryGetByteArrayAt(address, 3, result);
        public bool TryGetIntAt(int* address, int* result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetUIntAt(int* address, uint* result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetLongAt(int* address, long* result) => TryGetByteArrayAt(address, 8, result);
        public bool TryGetULongAt(int* address, ulong* result) => TryGetByteArrayAt(address, 8, result);
        public bool TryGetFloatAt(int* address, float* result) => TryGetByteArrayAt(address, 4, result);
        public bool TryGetDoubleAt(int* address, double* result) => TryGetByteArrayAt(address, 8, result);
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
