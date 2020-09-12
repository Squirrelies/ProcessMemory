using ProcessMemory.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static ProcessMemory.Common.PInvoke;
using static ProcessMemory.x86.PInvoke;
using MEMORY_BASIC_INFORMATION = ProcessMemory.x86.PInvoke.MEMORY_BASIC_INFORMATION32;

namespace ProcessMemory.x86
{
    public unsafe class ProcessMemoryHandler : ProcessMemory.Common.ProcessMemoryHandler, IDisposable
    {
        private static readonly IntPtr memoryBasicInformationSize = new IntPtr(sizeof(MEMORY_BASIC_INFORMATION));

        public ProcessMemoryHandler(int pid, bool readOnly = true) : base(pid, readOnly) { }

        private string GetMemoryProtectFlags(IntPtr offset)
        {
            try
            {
                MEMORY_BASIC_INFORMATION memBasicInfo = new MEMORY_BASIC_INFORMATION();
                VirtualQueryEx(ProcessHandle, offset, out memBasicInfo, memoryBasicInformationSize);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("[MEMORY_BASIC_INFORMATION]");
                sb.AppendFormat("BaseAddress: {0}\r\n", memBasicInfo.BaseAddress);
                sb.AppendFormat("AllocationBase: {0}\r\n", memBasicInfo.AllocationBase);
                sb.AppendFormat("AllocationProtect: {0}\r\n", memBasicInfo.AllocationProtect);
                sb.AppendFormat("RegionSize: {0}\r\n", memBasicInfo.RegionSize);
                sb.AppendFormat("State: {0}\r\n", memBasicInfo.State);
                sb.AppendFormat("Protect: {0}\r\n", memBasicInfo.Protect);
                sb.AppendFormat("Type: {0}\r\n", memBasicInfo.Type);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return string.Format("[GetMemoryProtectFlags EXCEPTION: {0}]", ex.ToString());
            }
        }

        public override byte[] GetByteArrayAt(IntPtr offset, int size)
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

        public override int SetByteArrayAt(IntPtr offset, byte[] data)
        {
            IntPtr bytesWritten = IntPtr.Zero;
            if (!WriteProcessMemory(ProcessHandle, offset, data, data.Length, out bytesWritten))
            {
                int win32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(win32Error, ((Win32Error)win32Error).ToString());
            }

            return bytesWritten.ToInt32();
        }

        public override bool TryGetByteArrayAt(IntPtr offset, int size, IntPtr result)
        {
            IntPtr bytesRead = IntPtr.Zero;
            return ReadProcessMemory(ProcessHandle, offset, result, size, out bytesRead);
        }

        public override bool TryGetByteArrayAt(IntPtr offset, int size, void* result)
        {
            IntPtr bytesRead = IntPtr.Zero;
            return ReadProcessMemory(ProcessHandle, offset, result, size, out bytesRead);
        }

        public Task<HashSet<long>> ScanMemoryAsync(byte[] searchValue, CancellationToken cancelToken)
        {
            return Task.Factory.StartNew(() =>
            {
                HashSet<long> returnValue = new HashSet<long>();

                long procMinAddress = sysInfo.minimumApplicationAddress.ToInt64();
                long procMaxAddress = sysInfo.maximumApplicationAddress.ToInt64();

                MEMORY_BASIC_INFORMATION memBasicInfo = new MEMORY_BASIC_INFORMATION();
                IntPtr bytesRead = IntPtr.Zero;

                byte[] buffer = null;
                while (procMinAddress < procMaxAddress)
                {
                    VirtualQueryEx(ProcessHandle, new IntPtr(procMinAddress), out memBasicInfo, memoryBasicInformationSize);

                    bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE);
                    if (hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT))
                    {
                        buffer = new byte[memBasicInfo.RegionSize.ToInt64()];
                        ReadProcessMemory(ProcessHandle, memBasicInfo.BaseAddress, buffer, (int)memBasicInfo.RegionSize, out bytesRead);

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

            MEMORY_BASIC_INFORMATION memBasicInfo = new MEMORY_BASIC_INFORMATION();
            IntPtr bytesRead = IntPtr.Zero;

            while (procMinAddress < procMaxAddress)
            {
                VirtualQueryEx(ProcessHandle, new IntPtr(procMinAddress), out memBasicInfo, memoryBasicInformationSize);

                bool hasAnyRead = memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READONLY) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_READWRITE) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READ) || memBasicInfo.Protect.HasFlag(AllocationProtect.PAGE_EXECUTE_READWRITE);
                if (hasAnyRead && memBasicInfo.State.HasFlag(MemoryFlags.MEM_COMMIT))
                {
                    byte[] buffer = new byte[memBasicInfo.RegionSize.ToInt64()];
                    ReadProcessMemory(ProcessHandle, memBasicInfo.BaseAddress, buffer, (int)memBasicInfo.RegionSize, out bytesRead);

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
    }
}
