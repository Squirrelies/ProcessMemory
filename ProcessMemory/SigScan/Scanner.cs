using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;

namespace ProcessMemory.SigScan
{
    public class Scanner
    {
        private readonly Process process;
        private readonly IntPtr startAddress;
        private readonly int size;
        private byte[] dumpedRegion;

        public Scanner(Process process, IntPtr startAddress, int size)
        {
            this.process = process;
            this.startAddress = startAddress;
            this.size = size;
        }

        private (byte[] bytes, bool[] mask) ParseSignature(string signature)
        {
            byte[] bytes = new byte[signature.Length / 2];
            bool[] mask = new bool[signature.Length / 2];
            for (int i = 0; i < signature.Length; i += 2)
            {
                string byteString = signature.Substring(i, 2);
                if (byteString == "??")
                {
                    mask[i / 2] = false;
                    bytes[i / 2] = 0x00;
                }
                else
                {
                    mask[i / 2] = true;
                    bytes[i / 2] = Convert.ToByte(byteString, 16);
                }
            }
            return (bytes, mask);
        }

        private bool DumpMemory()
        {
            if (this.process == null || this.process.HasExited || this.startAddress == IntPtr.Zero || this.size == 0)
                return false;

            try
            {
                this.dumpedRegion = new byte[this.size];
                IntPtr bytesRead = IntPtr.Zero;

                bool success = PInvoke.ReadProcessMemory(this.process.Handle, this.startAddress, this.dumpedRegion, this.size, out bytesRead);

                if (!success || (int)bytesRead != this.size)
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return false;
            }
        }

        // <summary>
        // Find a pattern in the dumped memory region.
        // Uses only a list of all known bytes.
        // </summary>
        public List<IntPtr> FindPatterns(byte[] signature)
        {
            List<IntPtr> results = [];

            if (signature == null || signature.Length == 0)
                return results;

            if (!this.DumpMemory())
                return results;

            for (int i = 0; i < this.dumpedRegion.Length - signature.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < signature.Length; j++)
                {
                    if (signature[j] != dumpedRegion[i + j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    results.Add(new IntPtr(startAddress.ToInt64() + i));
            }

            return results;
        }

        // <summary>
        // Find a pattern in the dumped memory region.
        // Uses a string signature with wildcards.
        // </summary>
        public List<IntPtr> FindPatterns(string signature)
        {
            List<IntPtr> results = [];

            if (string.IsNullOrEmpty(signature))
                return results;

            if (!this.DumpMemory())
                return results;

            var parsedSig = ParseSignature(signature);
            byte[] signatureBytes = parsedSig.bytes;
            bool[] mask = parsedSig.mask;

            for (int i = 0; i < this.dumpedRegion.Length - signatureBytes.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < signatureBytes.Length; j++)
                {
                    if (!mask[j] && signatureBytes[j] != dumpedRegion[i + j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    results.Add(new IntPtr(startAddress.ToInt64() + i));
            }

            return results;
        }
    }
}
