using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace NDistribUnit.Common.Common.PInvoke
{
    /// <summary>
    /// Wrappers around native functions
    /// </summary>
    public static class Win32
    {
        /// <summary>
        /// The flags used in mask and flags parameter in <see cref="SetHandleInformation"/>
        /// </summary>
        [Flags]
        public enum SetInformationHandleFlags
        {
            /// <summary>
            /// If this flag is set, a child process created with the bInheritHandles parameter of CreateProcess set to TRUE will inherit the object handle.
            /// </summary>

            HANDLE_FLAG_INHERIT = 0x1,

            /// <summary>
            /// If this flag is set, calling the CloseHandle function will not close the object handle.
            /// </summary>
            HANDLE_FLAG_PROTECT_FROM_CLOSE = 0x2
        }
        

        /// <summary>
        /// Sets the handle information.
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetHandleInformation(IntPtr handle, SetInformationHandleFlags mask, SetInformationHandleFlags flags);
    }
}

// ReSharper restore InconsistentNaming