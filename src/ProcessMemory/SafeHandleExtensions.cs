using System;
using Microsoft.Win32.SafeHandles;
using Windows.Win32.Foundation;

namespace ProcessMemory
{
    public static class SafeHandleExtensions
    {
        // Built-in SafeHandle implementations.
        public static SafeFileHandle ToSafeFileHandle(this HANDLE handle, bool ownsHandle = true) => new SafeFileHandle(handle, ownsHandle);
        public static SafeFileHandle ToSafeFileHandle(this IntPtr handle, bool ownsHandle = true) => new SafeFileHandle(handle, ownsHandle);
        // Microsoft.Win32.SafeHandles.SafeMemoryMappedFileHandle is not constructable.
        // Microsoft.Win32.SafeHandles.SafeMemoryMappedViewHandle is not constructable.
        public static SafePipeHandle ToSafePipeHandle(this HANDLE handle, bool ownsHandle = true) => new SafePipeHandle(handle, ownsHandle);
        public static SafePipeHandle ToSafePipeHandle(this IntPtr handle, bool ownsHandle = true) => new SafePipeHandle(handle, ownsHandle);
        public static SafeProcessHandle ToSafeProcessHandle(this HANDLE handle, bool ownsHandle = true) => new SafeProcessHandle(handle, ownsHandle);
        public static SafeProcessHandle ToSafeProcessHandle(this IntPtr handle, bool ownsHandle = true) => new SafeProcessHandle(handle, ownsHandle);
        public static SafeWaitHandle ToSafeWaitHandle(this HANDLE handle, bool ownsHandle = true) => new SafeWaitHandle(handle, ownsHandle);
        public static SafeWaitHandle ToSafeWaitHandle(this IntPtr handle, bool ownsHandle = true) => new SafeWaitHandle(handle, ownsHandle);
        // Microsoft.Win32.SafeHandles.SafeX509ChainHandle is not constructable.

        // Custom SafeHandle implementations.
        public static GenericSafeHandle ToGenericSafeHandle(this HANDLE handle, bool ownsHandle = true, Func<HANDLE, bool>? releaseAction = default) => new GenericSafeHandle(handle, ownsHandle, releaseAction);
    }
}
