using System;
using System.Collections.Generic;
using static Windows.Win32.PInvoke;
using Windows.Win32.System.Threading;
using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;
using ProcessMemory.Types;

namespace ProcessMemory
{
    public unsafe class ProcessMemoryHandler : IDisposable
    {
        private static readonly nuint memoryBasicInformationSize = (nuint)sizeof(MEMORY_BASIC_INFORMATION);

        public HANDLE ProcessHandle { get; private set; }

        public uint ProcessExitCode
        {
            get
            {
                uint lpExitCode = 0;
                GetExitCodeProcess(ProcessHandle, &lpExitCode);
                return lpExitCode;
            }
        }

        public bool ProcessRunning => ProcessExitCode == 259U;

        public ProcessMemoryHandler(uint pid, bool readOnly = true)
        {
            ProcessHandle = OpenProcess((readOnly) ? PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION | PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ : PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION | PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ | PROCESS_ACCESS_RIGHTS.PROCESS_VM_WRITE, false, pid);
        }

        private string GetMemoryProtectFlags(void* offset)
        {
            try
            {
                MEMORY_BASIC_INFORMATION memBasicInfo = new MEMORY_BASIC_INFORMATION();
                VirtualQueryEx(ProcessHandle, offset, &memBasicInfo, memoryBasicInformationSize);

                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendLine("[MEMORY_BASIC_INFORMATION]");
                sb.AppendFormat("BaseAddress: {0}\r\n", (nint)memBasicInfo.BaseAddress);
                sb.AppendFormat("AllocationBase: {0}\r\n", (nint)memBasicInfo.AllocationBase);
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

        public Span<byte> GetSpanByteAt(void* offset, nuint size)
        {
            Span<byte> returnValue = new byte[size];
            nuint d;
            fixed (byte* bp = returnValue)
                ReadProcessMemory(ProcessHandle, offset, bp, size, &d);
            return returnValue;
        }
        public Span<byte> GetSpanByteAt(nuint* offset, nuint size) => GetSpanByteAt(offset, size);

        public byte[] GetByteArrayAt(void* offset, nuint size) => GetSpanByteAt(offset, size).ToArray();
        public byte[] GetByteArrayAt(nuint* offset, nuint size) => GetByteArrayAt(offset, size);

        public nuint SetSpanByteAt(void* offset, Span<byte> input)
        {
            nuint lpBytesWritten = 0;
            fixed (byte* inputPtr = input)
                return WriteProcessMemory(ProcessHandle, offset, inputPtr, (nuint)input.Length, &lpBytesWritten) ? lpBytesWritten : 0;
        }
        public nuint SetSpanByteAt(nuint* offset, byte[] input) => SetSpanByteAt(offset, input);

        public nuint SetByteArrayAt(void* offset, byte[] input) => SetSpanByteAt(offset, input);
        public nuint SetByteArrayAt(nuint* offset, byte[] input) => SetByteArrayAt(offset, input);

        public bool TryGetByteArrayAt(void* offset, nuint size, void* result) => ReadProcessMemory(ProcessHandle, offset, result, size, null);
        public bool TryGetByteArrayAt(nuint* offset, nuint size, void* result) => TryGetByteArrayAt(offset, size, result);

        public bool TrySetByteArrayAt(void* offset, nuint size, void* result) => WriteProcessMemory(ProcessHandle, offset, result, size, null);
        public bool TrySetByteArrayAt(nuint* offset, nuint size, void* result) => TrySetByteArrayAt(offset, size, result);

        public T GetAt<T>(void* offset) where T : unmanaged
        {
            T* rv = stackalloc T[1];
            if (ReadProcessMemory(ProcessHandle, offset, rv, (nuint)sizeof(T), null))
                return *rv;
            else
                return default;
        }

        public T GetAt<T>(nuint* offset) where T : unmanaged => GetAt<T>(offset);

        public nuint SetAt<T>(void* offset, T value) where T : unmanaged
        {
            nuint lpBytesWritten = 0;
            return WriteProcessMemory(ProcessHandle, offset, &value, (nuint)sizeof(T), &lpBytesWritten) ? lpBytesWritten : 0;
        }

        public nuint SetAt<T>(nuint* offset, T value) where T : unmanaged => SetAt<T>(offset, value);

        public bool TryGetAt<T>(void* offset, ref T value) where T : unmanaged
        {
            fixed (T* pointer = &value)
            {
                return ReadProcessMemory(ProcessHandle, offset, pointer, (nuint)sizeof(T), null);
            }
        }

        public bool TryGetAt<T>(nuint* offset, ref T value) where T : unmanaged
        {
            fixed (T* pointer = &value)
            {
                return ReadProcessMemory(ProcessHandle, offset, pointer, (nuint)sizeof(T), null);
            }
        }

        public bool TrySetAt<T>(void* offset, ref T value) where T : unmanaged
        {
            fixed (T* pointer = &value)
            {
                return WriteProcessMemory(ProcessHandle, offset, pointer, (nuint)sizeof(T), null);
            }
        }

        public bool TrySetAt<T>(nuint* offset, ref T value) where T : unmanaged
        {
            fixed (T* pointer = &value)
            {
                return WriteProcessMemory(ProcessHandle, offset, pointer, (nuint)sizeof(T), null);
            }
        }

        public nint GetNIntAt(void* offset) => GetAt<nint>(offset);
        public nuint GetNUIntAt(void* offset) => GetAt<nuint>(offset);
        public sbyte GetSByteAt(void* offset) => GetAt<sbyte>(offset);
        public byte GetByteAt(void* offset) => GetAt<byte>(offset);
        public short GetShortAt(void* offset) => GetAt<short>(offset);
        public ushort GetUShortAt(void* offset) => GetAt<ushort>(offset);
        public Int24 GetInt24At(void* offset) => GetAt<Int24>(offset);
        public UInt24 GetUInt24At(void* offset) => GetAt<UInt24>(offset);
        public int GetIntAt(void* offset) => GetAt<int>(offset);
        public uint GetUIntAt(void* offset) => GetAt<uint>(offset);
        public long GetLongAt(void* offset) => GetAt<long>(offset);
        public ulong GetULongAt(void* offset) => GetAt<ulong>(offset);
        public float GetFloatAt(void* offset) => GetAt<float>(offset);
        public double GetDoubleAt(void* offset) => GetAt<double>(offset);
        public string GetASCIIStringAt(void* offset, nuint size) => GetSpanByteAt(offset, size).FromASCIIBytes();
        public string GetUnicodeStringAt(void* offset, nuint size) => GetSpanByteAt(offset, size).FromUnicodeBytes();

        public nuint SetNIntAt(void* offset, nint value) => SetAt(offset, value);
        public nuint SetNUIntAt(void* offset, nuint value) => SetAt(offset, value);
        public nuint SetSByteAt(void* offset, sbyte value) => SetAt(offset, value);
        public nuint SetByteAt(void* offset, byte value) => SetAt(offset, value);
        public nuint SetShortAt(void* offset, short value) => SetAt(offset, value);
        public nuint SetUShortAt(void* offset, ushort value) => SetAt(offset, value);
        public nuint SetInt24At(void* offset, Int24 value) => SetAt(offset, value);
        public nuint SetUInt24At(void* offset, UInt24 value) => SetAt(offset, value);
        public nuint SetIntAt(void* offset, int value) => SetAt(offset, value);
        public nuint SetUIntAt(void* offset, uint value) => SetAt(offset, value);
        public nuint SetLongAt(void* offset, long value) => SetAt(offset, value);
        public nuint SetULongAt(void* offset, ulong value) => SetAt(offset, value);
        public nuint SetFloatAt(void* offset, float value) => SetAt(offset, value);
        public nuint SetDoubleAt(void* offset, double value) => SetAt(offset, value);

        public bool TryGetNIntAt(void* address, ref nint result) => TryGetAt(address, ref result);
        public bool TryGetNUIntAt(void* address, ref nuint result) => TryGetAt(address, ref result);
        public bool TryGetSByteAt(void* address, ref sbyte result) => TryGetAt(address, ref result);
        public bool TryGetByteAt(void* address, ref byte result) => TryGetAt(address, ref result);
        public bool TryGetShortAt(void* address, ref short result) => TryGetAt(address, ref result);
        public bool TryGetUShortAt(void* address, ref ushort result) => TryGetAt(address, ref result);
        public bool TryGetInt24At(void* address, ref Int24 result) => TryGetAt(address, ref result);
        public bool TryGetUInt24At(void* address, ref UInt24 result) => TryGetAt(address, ref result);
        public bool TryGetIntAt(void* address, ref int result) => TryGetAt(address, ref result);
        public bool TryGetUIntAt(void* address, ref uint result) => TryGetAt(address, ref result);
        public bool TryGetLongAt(void* address, ref long result) => TryGetAt(address, ref result);
        public bool TryGetULongAt(void* address, ref ulong result) => TryGetAt(address, ref result);
        public bool TryGetFloatAt(void* address, ref float result) => TryGetAt(address, ref result);
        public bool TryGetDoubleAt(void* address, ref double result) => TryGetAt(address, ref result);

        public bool TryGetSByteAt(void* address, sbyte* result) => TryGetAt(address, ref *result);
        public bool TryGetByteAt(void* address, byte* result) => TryGetAt(address, ref *result);
        public bool TryGetShortAt(void* address, short* result) => TryGetAt(address, ref *result);
        public bool TryGetUShortAt(void* address, ushort* result) => TryGetAt(address, ref *result);
        public bool TryGetInt24At(void* address, Int24* result) => TryGetAt(address, ref *result);
        public bool TryGetUInt24At(void* address, UInt24* result) => TryGetAt(address, ref *result);
        public bool TryGetIntAt(void* address, int* result) => TryGetAt(address, ref *result);
        public bool TryGetUIntAt(void* address, uint* result) => TryGetAt(address, ref *result);
        public bool TryGetLongAt(void* address, long* result) => TryGetAt(address, ref *result);
        public bool TryGetULongAt(void* address, ulong* result) => TryGetAt(address, ref *result);
        public bool TryGetFloatAt(void* address, float* result) => TryGetAt(address, ref *result);
        public bool TryGetDoubleAt(void* address, double* result) => TryGetAt(address, ref *result);
        public unsafe bool TryGetASCIIStringAt(void* address, nuint size, ref string result)
        {
            Span<byte> stringSpan = new byte[size];
            fixed (byte* bp = stringSpan)
                if (TryGetByteArrayAt(address, size, bp))
                {
                    result = stringSpan.FromASCIIBytes();
                    return true;
                }
            return false;
        }
        public unsafe bool TryGetUnicodeStringAt(void* address, nuint size, ref string result)
        {
            Span<byte> stringSpan = new byte[size];
            fixed (byte* bp = stringSpan)
                if (TryGetByteArrayAt(address, size, bp))
                {
                    result = stringSpan.FromUnicodeBytes();
                    return true;
                }
            return false;
        }

        public bool TryGetNIntAt(nuint* address, nint* result) => TryGetAt(address, ref *result);
        public bool TryGetNUIntAt(nuint* address, nuint* result) => TryGetAt(address, ref *result);
        public bool TryGetSByteAt(nuint* address, sbyte* result) => TryGetAt(address, ref *result);
        public bool TryGetByteAt(nuint* address, byte* result) => TryGetAt(address, ref *result);
        public bool TryGetShortAt(nuint* address, short* result) => TryGetAt(address, ref *result);
        public bool TryGetUShortAt(nuint* address, ushort* result) => TryGetAt(address, ref *result);
        public bool TryGetInt24At(nuint* address, Int24* result) => TryGetAt(address, ref *result);
        public bool TryGetUInt24At(nuint* address, UInt24* result) => TryGetAt(address, ref *result);
        public bool TryGetIntAt(nuint* address, int* result) => TryGetAt(address, ref *result);
        public bool TryGetUIntAt(nuint* address, uint* result) => TryGetAt(address, ref *result);
        public bool TryGetLongAt(nuint* address, long* result) => TryGetAt(address, ref *result);
        public bool TryGetULongAt(nuint* address, ulong* result) => TryGetAt(address, ref *result);
        public bool TryGetFloatAt(nuint* address, float* result) => TryGetAt(address, ref *result);
        public bool TryGetDoubleAt(nuint* address, double* result) => TryGetAt(address, ref *result);
        public bool TryGetASCIIStringAt(nuint* address, nuint size, ref string result) => TryGetASCIIStringAt(address, size, ref result);
        public bool TryGetUnicodeStringAt(nuint* address, nuint size, ref string result) => TryGetUnicodeStringAt(address, size, ref result);

        public static nint FindIndexOf(byte[] array, nint start, byte[] sequence)
        {
            nint end = array.Length - sequence.Length; // past here no match is possible
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
                    for (nint offset = 1; offset < sequence.Length; ++offset)
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

        public static IEnumerable<nint> FindIndexesOf(byte[] array, nint start, byte[] sequence)
        {
            nint end = array.Length - sequence.Length;
            nint indexOf;
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
