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
        private SYSTEM_INFO sysInfo;
        private UIntPtr processHandle = UIntPtr.Zero;

        private static TaskCreationOptions taskOpts = TaskCreationOptions.DenyChildAttach;
        private static TaskScheduler taskSched = TaskScheduler.Default;

        public ProcessMemory(int pid)
        {
            sysInfo = new SYSTEM_INFO();
            GetSystemInfo(out sysInfo);
            processHandle = OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryRead | ProcessAccessFlags.VirtualMemoryWrite, false, (uint)pid);
        }

        public Task<byte[]> GetByteArrayAtAsync(ulong offset, int size, CancellationToken cancelToken)
        {
            return Task.Factory.StartNew(() =>
            {
                byte[] returnValue = new byte[size];
                MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
                IntPtr bytesRead = IntPtr.Zero;

                VirtualQueryEx(processHandle, new UIntPtr(offset), out memBasicInfo, new UIntPtr(48));

                bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE);
                if (hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT))
                {
                    bool success = ReadProcessMemory(processHandle, offset, returnValue, size, out bytesRead);
                    int win32Error = Marshal.GetLastWin32Error();
                }

                return returnValue;
            }, cancelToken, taskOpts, taskSched);
        }

        public Task<int> SetByteArrayAtAsync(ulong offset, byte[] data, CancellationToken cancelToken)
        {
            return Task.Factory.StartNew(() =>
            {
                MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
                IntPtr bytesWritten = IntPtr.Zero;

                VirtualQueryEx(processHandle, new UIntPtr(offset), out memBasicInfo, new UIntPtr(48));

                bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE);
                if (hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT))
                {
                    bool success = WriteProcessMemory(processHandle, offset, data, data.Length, out bytesWritten);
                    int win32Error = Marshal.GetLastWin32Error();
                }

                return bytesWritten.ToInt32();
            }, cancelToken, taskOpts, taskSched);
        }

        public async Task<sbyte> GetSByteAtAsync(ulong offset, CancellationToken cancelToken) => (sbyte)(await GetByteArrayAtAsync(offset, 1, cancelToken))[0];
        public async Task<byte> GetByteAtAsync(ulong offset, CancellationToken cancelToken) => (await GetByteArrayAtAsync(offset, 1, cancelToken))[0];
        public async Task<short> GetShortAtAsync(ulong offset, CancellationToken cancelToken) => HighPerfBitConverter.ToInt16(await GetByteArrayAtAsync(offset, 2, cancelToken), 0);
        public async Task<ushort> GetUShortAtAsync(ulong offset, CancellationToken cancelToken) => HighPerfBitConverter.ToUInt16(await GetByteArrayAtAsync(offset, 2, cancelToken), 0);
        public async Task<int> GetIntAtAsync(ulong offset, CancellationToken cancelToken) => HighPerfBitConverter.ToInt32(await GetByteArrayAtAsync(offset, 4, cancelToken), 0);
        public async Task<uint> GetUIntAtAsync(ulong offset, CancellationToken cancelToken) => HighPerfBitConverter.ToUInt32(await GetByteArrayAtAsync(offset, 4, cancelToken), 0);
        public async Task<long> GetLongAtAsync(ulong offset, CancellationToken cancelToken) => HighPerfBitConverter.ToInt64(await GetByteArrayAtAsync(offset, 8, cancelToken), 0);
        public async Task<ulong> GetULongAtAsync(ulong offset, CancellationToken cancelToken) => HighPerfBitConverter.ToUInt64(await GetByteArrayAtAsync(offset, 8, cancelToken), 0);
        public async Task<float> GetFloatAtAsync(ulong offset, CancellationToken cancelToken) => BitConverter.ToSingle(await GetByteArrayAtAsync(offset, 4, cancelToken), 0);
        public async Task<double> GetDoubleAtAsync(ulong offset, CancellationToken cancelToken) => BitConverter.ToDouble(await GetByteArrayAtAsync(offset, 8, cancelToken), 0);

        public async Task<int> SetSByteAtAsync(ulong offset, sbyte value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, new byte[1] { (byte)value }, cancelToken);
        public async Task<int> SetByteAtAsync(ulong offset, byte value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, new byte[1] { value }, cancelToken);
        public async Task<int> SetShortAtAsync(ulong offset, short value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetUShortAtAsync(ulong offset, ushort value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetIntAtAsync(ulong offset, int value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetUIntAtAsync(ulong offset, uint value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetLongAtAsync(ulong offset, long value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetULongAtAsync(ulong offset, ulong value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetFloatAtAsync(ulong offset, float value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);
        public async Task<int> SetDoubleAtAsync(ulong offset, double value, CancellationToken cancelToken) => await SetByteArrayAtAsync(offset, BitConverter.GetBytes(value), cancelToken);

        public Task<HashSet<ulong>> ScanMemoryAsync(byte[] searchValue, CancellationToken cancelToken)
        {
            return Task.Factory.StartNew(() =>
            {
                HashSet<ulong> returnValue = new HashSet<ulong>();

                ulong procMinAddress = sysInfo.minimumApplicationAddress.ToUInt64();
                ulong procMaxAddress = sysInfo.maximumApplicationAddress.ToUInt64();

                MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
                IntPtr bytesRead = IntPtr.Zero;

                byte[] buffer = null;
                while (procMinAddress < procMaxAddress)
                {
                    VirtualQueryEx(processHandle, new UIntPtr(procMinAddress), out memBasicInfo, new UIntPtr(48));

                    bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE);
                    if (hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT))
                    {
                        buffer = new byte[memBasicInfo.RegionSize.ToUInt64()];
                        ReadProcessMemory(processHandle, memBasicInfo.BaseAddress.ToUInt64(), buffer, (int)memBasicInfo.RegionSize, out bytesRead);

                        foreach (int offset in FindIndexesOf(buffer, 0, searchValue))
                        {
                            returnValue.Add(memBasicInfo.BaseAddress.ToUInt64() + (ulong)offset);
                        }

                        buffer = null;
                        GC.Collect();
                    }

                    procMinAddress += memBasicInfo.RegionSize.ToUInt64();
                }

                return returnValue;
            }, cancelToken, taskOpts, taskSched);
        }

        public IEnumerable<byte[]> DumpMemory()
        {
            ulong procMinAddress = sysInfo.minimumApplicationAddress.ToUInt64();
            ulong procMaxAddress = sysInfo.maximumApplicationAddress.ToUInt64();

            MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
            IntPtr bytesRead = IntPtr.Zero;

            while (procMinAddress < procMaxAddress)
            {
                VirtualQueryEx(processHandle, new UIntPtr(procMinAddress), out memBasicInfo, new UIntPtr(48));

                bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE);
                if (hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT))
                {
                    byte[] buffer = new byte[memBasicInfo.RegionSize.ToUInt64()];
                    ReadProcessMemory(processHandle, memBasicInfo.BaseAddress.ToUInt64(), buffer, (int)memBasicInfo.RegionSize, out bytesRead);

                    yield return buffer;
                }

                procMinAddress += memBasicInfo.RegionSize.ToUInt64();
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
                CloseHandle(processHandle);

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
