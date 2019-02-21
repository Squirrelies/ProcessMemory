using static ProcessMemory.PInvoke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessMemory
{
    public class ProcessMemory : IDisposable
    {
        private static TaskCreationOptions taskOpts = TaskCreationOptions.DenyChildAttach;
        private static TaskScheduler taskSched = TaskScheduler.Default;

        public IntPtr ProcessHandle = IntPtr.Zero;
        private SYSTEM_INFO sysInfo;

        public ProcessMemory(int pid, bool readOnly = true)
        {
            sysInfo = new SYSTEM_INFO();
            GetSystemInfo(out sysInfo);
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

        public byte[] GetByteArrayAt(long offset, int size)
        {
            byte[] returnValue = new byte[size];
            MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
            IntPtr bytesRead = IntPtr.Zero;

            VirtualQueryEx(ProcessHandle, new IntPtr(offset), out memBasicInfo, new IntPtr(48));

            bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE);
            if (hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT))
            {
                bool success = ReadProcessMemory(ProcessHandle, offset, returnValue, size, out bytesRead);
                int win32Error = Marshal.GetLastWin32Error();
            }

            return returnValue;
        }

        public Task<byte[]> GetByteArrayAtAsync(long offset, int size, CancellationToken cancelToken) => Task.Factory.StartNew(() => GetByteArrayAt(offset, size), cancelToken, taskOpts, taskSched);

        public int SetByteArrayAt(long offset, byte[] data)
        {
            MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
            IntPtr bytesWritten = IntPtr.Zero;

            VirtualQueryEx(ProcessHandle, new IntPtr(offset), out memBasicInfo, new IntPtr(48));

            bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE);
            if (hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT))
            {
                bool success = WriteProcessMemory(ProcessHandle, offset, data, data.Length, out bytesWritten);
                int win32Error = Marshal.GetLastWin32Error();
            }

            return bytesWritten.ToInt32();
        }

        public Task<int> SetByteArrayAtAsync(long offset, byte[] data, CancellationToken cancelToken) => Task.Factory.StartNew(() => SetByteArrayAt(offset, data), cancelToken, taskOpts, taskSched);

        public sbyte GetSByteAt(long offset) => (sbyte)GetByteArrayAt(offset, 1)[0];
        public byte GetByteAt(long offset) => GetByteArrayAt(offset, 1)[0];
        public short GetShortAt(long offset) => HighPerfBitConverter.ToInt16(GetByteArrayAt(offset, 2), 0);
        public ushort GetUShortAt(long offset) => HighPerfBitConverter.ToUInt16(GetByteArrayAt(offset, 2), 0);
        public int GetIntAt(long offset) => HighPerfBitConverter.ToInt32(GetByteArrayAt(offset, 4), 0);
        public uint GetUIntAt(long offset) => HighPerfBitConverter.ToUInt32(GetByteArrayAt(offset, 4), 0);
        public long GetLongAt(long offset) => HighPerfBitConverter.ToInt64(GetByteArrayAt(offset, 8), 0);
        public ulong GetULongAt(long offset) => HighPerfBitConverter.ToUInt64(GetByteArrayAt(offset, 8), 0);
        public float GetFloatAt(long offset) => BitConverter.ToSingle(GetByteArrayAt(offset, 4), 0);
        public double GetDoubleAt(long offset) => BitConverter.ToDouble(GetByteArrayAt(offset, 8), 0);

        public async Task<sbyte> GetSByteAtAsync(long offset, CancellationToken cancelToken) => (sbyte)(await GetByteArrayAtAsync(offset, 1, cancelToken))[0];
        public async Task<byte> GetByteAtAsync(long offset, CancellationToken cancelToken) => (await GetByteArrayAtAsync(offset, 1, cancelToken))[0];
        public async Task<short> GetShortAtAsync(long offset, CancellationToken cancelToken) => HighPerfBitConverter.ToInt16(await GetByteArrayAtAsync(offset, 2, cancelToken), 0);
        public async Task<ushort> GetUShortAtAsync(long offset, CancellationToken cancelToken) => HighPerfBitConverter.ToUInt16(await GetByteArrayAtAsync(offset, 2, cancelToken), 0);
        public async Task<int> GetIntAtAsync(long offset, CancellationToken cancelToken) => HighPerfBitConverter.ToInt32(await GetByteArrayAtAsync(offset, 4, cancelToken), 0);
        public async Task<uint> GetUIntAtAsync(long offset, CancellationToken cancelToken) => HighPerfBitConverter.ToUInt32(await GetByteArrayAtAsync(offset, 4, cancelToken), 0);
        public async Task<long> GetLongAtAsync(long offset, CancellationToken cancelToken) => HighPerfBitConverter.ToInt64(await GetByteArrayAtAsync(offset, 8, cancelToken), 0);
        public async Task<ulong> GetULongAtAsync(long offset, CancellationToken cancelToken) => HighPerfBitConverter.ToUInt64(await GetByteArrayAtAsync(offset, 8, cancelToken), 0);
        public async Task<float> GetFloatAtAsync(long offset, CancellationToken cancelToken) => BitConverter.ToSingle(await GetByteArrayAtAsync(offset, 4, cancelToken), 0);
        public async Task<double> GetDoubleAtAsync(long offset, CancellationToken cancelToken) => BitConverter.ToDouble(await GetByteArrayAtAsync(offset, 8, cancelToken), 0);

        public int SetSByteAt(long offset, sbyte value) => SetByteArrayAt(offset, new byte[1] { (byte)value });
        public int SetByteAt(long offset, byte value) => SetByteArrayAt(offset, new byte[1] { value });
        public int SetShortAt(long offset, short value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetUShortAt(long offset, ushort value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetIntAt(long offset, int value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetUIntAt(long offset, uint value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetLongAt(long offset, long value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetULongAt(long offset, ulong value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetFloatAt(long offset, float value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));
        public int SetDoubleAt(long offset, double value) => SetByteArrayAt(offset, BitConverter.GetBytes(value));

        public async Task<int> SetSByteAtAsync(long offset, sbyte value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, new byte[1] { (byte)value }, cancelToken);
        public async Task<int> SetByteAtAsync(long offset, byte value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, new byte[1] { value }, cancelToken);
        public async Task<int> SetShortAtAsync(long offset, short value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetUShortAtAsync(long offset, ushort value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetIntAtAsync(long offset, int value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetUIntAtAsync(long offset, uint value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetLongAtAsync(long offset, long value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetULongAtAsync(long offset, ulong value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetFloatAtAsync(long offset, float value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetDoubleAtAsync(long offset, double value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);

        public Task<HashSet<long>> ScanMemoryAsync(byte[] searchValue, CancellationToken cancelToken)
        {
            return Task.Factory.StartNew(() =>
            {
                HashSet<long> returnValue = new HashSet<long>();

                long procMinAddress = sysInfo.minimumApplicationAddress.ToInt64();
                long procMaxAddress = sysInfo.maximumApplicationAddress.ToInt64();

                MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
                IntPtr bytesRead = IntPtr.Zero;

                byte[] buffer = null;
                while (procMinAddress < procMaxAddress)
                {
                    VirtualQueryEx(ProcessHandle, new IntPtr(procMinAddress), out memBasicInfo, new IntPtr(48));

                    bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE);
                    if (hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT))
                    {
                        buffer = new byte[memBasicInfo.RegionSize.ToInt64()];
                        ReadProcessMemory(ProcessHandle, memBasicInfo.BaseAddress.ToInt64(), buffer, (int)memBasicInfo.RegionSize, out bytesRead);

                        foreach (int offset in FindIndexesOf(buffer, 0, searchValue))
                        {
                            returnValue.Add(memBasicInfo.BaseAddress.ToInt64() + (long)offset);
                        }

                        buffer = null;
                        GC.Collect();
                    }

                    procMinAddress += memBasicInfo.RegionSize.ToInt64();
                }

                return returnValue;
            }, cancelToken, taskOpts, taskSched);
        }

        public IEnumerable<byte[]> DumpMemory()
        {
            long procMinAddress = sysInfo.minimumApplicationAddress.ToInt64();
            long procMaxAddress = sysInfo.maximumApplicationAddress.ToInt64();

            MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
            IntPtr bytesRead = IntPtr.Zero;

            while (procMinAddress < procMaxAddress)
            {
                VirtualQueryEx(ProcessHandle, new IntPtr(procMinAddress), out memBasicInfo, new IntPtr(48));

                bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE);
                if (hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT))
                {
                    byte[] buffer = new byte[memBasicInfo.RegionSize.ToInt64()];
                    ReadProcessMemory(ProcessHandle, memBasicInfo.BaseAddress.ToInt64(), buffer, (int)memBasicInfo.RegionSize, out bytesRead);

                    yield return buffer;
                }

                procMinAddress += memBasicInfo.RegionSize.ToInt64();
            }
        }

        public void DumpMemoryToFile(string fileName = "Memory.DMP")
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                foreach (byte[] memorySection in DumpMemory())
                {
                    fs.Write(memorySection, 0, memorySection.Length);
                }
            }
        }

        private static int FindIndexOf(byte[] array, int start, byte[] sequence)
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

        private static IEnumerable<int> FindIndexesOf(byte[] array, int start, byte[] sequence)
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
