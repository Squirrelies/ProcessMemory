using ProcessMemory.Common.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ProcessMemory.Common.PInvoke;

namespace ProcessMemory.Common
{
    public unsafe abstract class ProcessMemoryHandler : IDisposable
    {
        protected static TaskCreationOptions taskOpts = TaskCreationOptions.DenyChildAttach;
        protected static TaskScheduler taskSched = TaskScheduler.Default;

        public IntPtr ProcessHandle = IntPtr.Zero;
        protected SYSTEM_INFO sysInfo;

        protected ProcessMemoryHandler(int pid, bool readOnly = true)
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

        public abstract byte[] GetByteArrayAt(IntPtr offset, int size);
        public abstract int SetByteArrayAt(IntPtr offset, byte[] data);

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

        public abstract bool TryGetByteArrayAt(IntPtr offset, int size, IntPtr result);

        public bool TryGetSByteAt(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 1, result);
        public bool TryGetByteAt(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 1, result);
        public bool TryGetShortAt(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 2, result);
        public bool TryGetUShortAt(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 2, result);
        public bool TryGetInt24At(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 3, result);
        public bool TryGetUInt24At(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 3, result);
        public bool TryGetIntAt(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 4, result);
        public bool TryGetUIntAt(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 4, result);
        public bool TryGetLongAt(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 8, result);
        public bool TryGetULongAt(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 8, result);
        public bool TryGetFloatAt(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 4, result);
        public bool TryGetDoubleAt(IntPtr offset, IntPtr result) => TryGetByteArrayAt(offset, 8, result);

        public abstract bool TryGetByteArrayAt(IntPtr offset, int size, void* result);

        public bool TryGetSByteAt(IntPtr offset, sbyte* result) => TryGetByteArrayAt(offset, 1, result);
        public bool TryGetByteAt(IntPtr offset, byte* result) => TryGetByteArrayAt(offset, 1, result);
        public bool TryGetShortAt(IntPtr offset, short* result) => TryGetByteArrayAt(offset, 2, result);
        public bool TryGetUShortAt(IntPtr offset, ushort* result) => TryGetByteArrayAt(offset, 2, result);
        public bool TryGetInt24At(IntPtr offset, byte* result) => TryGetByteArrayAt(offset, 3, result);
        public bool TryGetUInt24At(IntPtr offset, byte* result) => TryGetByteArrayAt(offset, 3, result);
        public bool TryGetIntAt(IntPtr offset, int* result) => TryGetByteArrayAt(offset, 4, result);
        public bool TryGetUIntAt(IntPtr offset, uint* result) => TryGetByteArrayAt(offset, 4, result);
        public bool TryGetLongAt(IntPtr offset, long* result) => TryGetByteArrayAt(offset, 8, result);
        public bool TryGetULongAt(IntPtr offset, ulong* result) => TryGetByteArrayAt(offset, 8, result);
        public bool TryGetFloatAt(IntPtr offset, float* result) => TryGetByteArrayAt(offset, 4, result);
        public bool TryGetDoubleAt(IntPtr offset, double* result) => TryGetByteArrayAt(offset, 8, result);

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
