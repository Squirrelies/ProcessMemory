using static ProcessMemory.PInvoke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

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

        public string GetMemoryProtectFlags(long offset)
        {
            try
            {
                MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
                VirtualQueryEx(ProcessHandle, new IntPtr(offset), out memBasicInfo, new IntPtr(48));
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
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
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return string.Format("[GetMemoryProtectFlags EXCEPTION: {0}]", ex.ToString());
            }
        }

        public byte[] GetByteArrayAt(long offset, int size)
        {
            byte[] returnValue = new byte[size];

            //if (verify)
            //{
            //    MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
            //    VirtualQueryEx(ProcessHandle, new IntPtr(offset), out memBasicInfo, new IntPtr(48));
            //    bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_WRITECOPY); // WRITECOPY also works?
            //    if (!(hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT)))
            //        return returnValue;
            //}

            string debuggy = GetMemoryProtectFlags(offset);

            IntPtr bytesRead = IntPtr.Zero;
            if (!ReadProcessMemory(ProcessHandle, offset, returnValue, size, out bytesRead))
            {
                int win32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(win32Error, string.Format("{0}: {1}: {2}", ((Win32Error)win32Error).ToString(), bytesRead.ToInt32(), GetMemoryProtectFlags(offset)));
            }

            return returnValue;
        }

        public int SetByteArrayAt(long offset, byte[] data)
        {
            MEMORY_BASIC_INFORMATION64 memBasicInfo = new MEMORY_BASIC_INFORMATION64();
            IntPtr bytesWritten = IntPtr.Zero;

            VirtualQueryEx(ProcessHandle, new IntPtr(offset), out memBasicInfo, new IntPtr(48));

            bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE);
            if (hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT))
            {
                if (!WriteProcessMemory(ProcessHandle, offset, data, data.Length, out bytesWritten))
                {
                    int win32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(win32Error, ((Win32Error)win32Error).ToString());
                }
            }

            return bytesWritten.ToInt32();
        }

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
