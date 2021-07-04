//using System;

//namespace ProcessMemory
//{
//    /// <summary>
//    /// Actually does the same code as BitConverter except it doesn't check endianness, bother with contracts or emit CLS compliance. It just does the bit shifting.
//    /// </summary>
//    public static class HighPerfBitConverter
//    {
//        public static unsafe short ToInt16(byte[] recordData, int offset)
//        {
//            if (recordData.Length >= offset + 2)
//                fixed (byte* b = &recordData[offset])
//                    return (short)((*b) | (*(b + 1) << 8));
//            else
//                throw new IndexOutOfRangeException();
//        }

//        public static ushort ToUInt16(byte[] recordData, int offset) => (ushort)ToInt16(recordData, offset);

//        public static unsafe int ToInt32(byte[] recordData, int offset)
//        {
//            if (recordData.Length >= offset + 4)
//                fixed (byte* b = &recordData[offset])
//                    return (*b) | (*(b + 1) << 8) | (*(b + 2) << 16) | (*(b + 3) << 24);
//            else
//                throw new IndexOutOfRangeException();
//        }

//        public static uint ToUInt32(byte[] recordData, int offset) => (uint)ToInt32(recordData, offset);

//        public static unsafe long ToInt64(byte[] recordData, int offset)
//        {
//            if (recordData.Length >= offset + 8)
//            {
//                fixed (byte* b = &recordData[offset])
//                {
//                    int i1 = (*b) | (*(b + 1) << 8) | (*(b + 2) << 16) | (*(b + 3) << 24);
//                    int i2 = (*(b + 4)) | (*(b + 5) << 8) | (*(b + 6) << 16) | (*(b + 7) << 24);
//                    return (uint)i1 | ((long)i2 << 32);
//                }
//            }
//            else
//                throw new IndexOutOfRangeException();
//        }

//        public static ulong ToUInt64(byte[] recordData, int offset) => (ulong)ToInt64(recordData, offset);
//    }
//}
