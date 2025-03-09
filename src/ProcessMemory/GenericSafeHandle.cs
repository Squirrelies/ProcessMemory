using System;
using Microsoft.Win32.SafeHandles;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace ProcessMemory
{
    /// <summary>
    /// A generic <seealso cref="SafeHandle"/> implementation.
    /// </summary>
    public sealed class GenericSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// A <seealso cref="System.Func{HANDLE, bool}"/> to perform when it is time to release this <seealso cref="HANDLE"/>.
        /// </summary>
        private readonly Func<HANDLE, bool> releaseAction;

        /// <summary>
        /// Whether the <seealso cref="HANDLE"/> is invalid.
        /// </summary>
        public override bool IsInvalid => handle == IntPtr.Zero || handle == new IntPtr(-1);

        /// <summary>
        /// A generic <seealso cref="SafeHandle"/> implementation.
        /// </summary>
        /// <param name="handle">A <seealso cref="HANDLE"/> to wrap into a <seealso cref="SafeHandle"/>.</param>
        /// <param name="ownsHandle">Whether we own this <seealso cref="HANDLE"/>.</param>
        /// <param name="releaseAction">A <seealso cref="System.Func{HANDLE, bool}"/> to release the <seealso cref="HANDLE"/> when it goes out of scope. By default, if left undefined, we call <seealso cref="CloseHandle"/>.</param>
        public GenericSafeHandle(HANDLE handle, bool ownsHandle = true, Func<HANDLE, bool>? releaseAction = default) : base(ownsHandle)
        {
            SetHandle(handle);
            this.releaseAction = releaseAction ?? (handle => CloseHandle(handle).Value != 0);
        }

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
                return releaseAction((HANDLE)handle);

            return true;
        }
    }
}
