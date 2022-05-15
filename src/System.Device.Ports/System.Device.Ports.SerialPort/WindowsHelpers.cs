// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.System.Diagnostics.Debug;
using Windows.Win32.Foundation;
using Windows.Win32.Devices.Communication;

#pragma warning disable CA1416 // Validate platform compatibility

namespace System.Device.Ports.SerialPort
{
    internal static class WindowsHelpers
    {
        // This was declarated here because the "CsWin32 code generator" generates a wrong signature
        [DllImport("Kernel32", ExactSpelling = true, EntryPoint = "FormatMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe uint FormatMessage(FORMAT_MESSAGE_OPTIONS dwFlags, [Optional] void* lpSource,
            uint dwMessageId, uint dwLanguageId, void* lpBuffer, uint nSize, [Optional] sbyte** Arguments);

        public static Exception GetExceptionForLastWin32Error()
        {
            var hr = Marshal.GetHRForLastWin32Error();
            return Marshal.GetExceptionForHR(hr)
                ?? new ComponentModel.Win32Exception(hr, String.Format(Strings.Win32Error, hr));
        }

        public static Exception GetExceptionForWin32Error(WIN32_ERROR errorCode)
        {
            Debug.Assert(errorCode != WIN32_ERROR.ERROR_SUCCESS, "Error method called with ERROR_SUCCESS");

            return errorCode switch
            {

                WIN32_ERROR.ERROR_FILE_NOT_FOUND => new FileNotFoundException(Strings.IO_FileNotFound),
                WIN32_ERROR.ERROR_PATH_NOT_FOUND => new DirectoryNotFoundException(Strings.IO_PathNotFound_NoPathName),
                WIN32_ERROR.ERROR_ACCESS_DENIED => new UnauthorizedAccessException(Strings.UnauthorizedAccess_IODenied_NoPathName),
                WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE => new PathTooLongException(Strings.IO_PathTooLong),
                WIN32_ERROR.ERROR_SHARING_VIOLATION => new IOException(
                    Strings.IO_SharingViolation_NoFileName, MakeHRFromErrorCode(errorCode)),
                WIN32_ERROR.ERROR_OPERATION_ABORTED => new OperationCanceledException(),

                WIN32_ERROR.ERROR_FILE_EXISTS or
                WIN32_ERROR.ERROR_INVALID_PARAMETER or
                WIN32_ERROR.ERROR_ALREADY_EXISTS or
                _ => new IOException(GetMessage(errorCode), MakeHRFromErrorCode(errorCode)),
            };
        }

        public static Exception GetExceptionForWin32Error(WIN32_ERROR errorCode, string? optionalArgument)
        {
            if (optionalArgument == null)
            {
                return GetExceptionForWin32Error(errorCode);
            }

            Debug.Assert(errorCode != WIN32_ERROR.ERROR_SUCCESS, "Error method called with ERROR_SUCCESS");

            return errorCode switch
            {
                WIN32_ERROR.ERROR_FILE_NOT_FOUND => new FileNotFoundException(
                    string.Format(Strings.IO_FileNotFound_FileName, optionalArgument), optionalArgument),
                WIN32_ERROR.ERROR_PATH_NOT_FOUND => new DirectoryNotFoundException(
                    string.Format(Strings.IO_PathNotFound_Path, optionalArgument)),
                WIN32_ERROR.ERROR_ACCESS_DENIED => new UnauthorizedAccessException(
                   string.Format(Strings.UnauthorizedAccess_IODenied_Path, optionalArgument)),
                WIN32_ERROR.ERROR_ALREADY_EXISTS => new IOException(
                    string.Format(Strings.IO_AlreadyExists_Name, optionalArgument), MakeHRFromErrorCode(errorCode)),
                WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE => new PathTooLongException(
                   string.Format(Strings.IO_PathTooLong_Path, optionalArgument)),
                WIN32_ERROR.ERROR_SHARING_VIOLATION => new IOException(
                    string.Format(Strings.IO_SharingViolation_File, optionalArgument), MakeHRFromErrorCode(errorCode)),
                WIN32_ERROR.ERROR_FILE_EXISTS => new IOException(
                    string.Format(Strings.IO_FileExists_Name, optionalArgument), MakeHRFromErrorCode(errorCode)),
                WIN32_ERROR.ERROR_OPERATION_ABORTED => new OperationCanceledException(),

                WIN32_ERROR.ERROR_INVALID_PARAMETER or _ => new IOException(
                    $"{GetMessage(errorCode)} : '{optionalArgument}'", MakeHRFromErrorCode(errorCode)),
            };
        }

        public static int MakeHRFromErrorCode(WIN32_ERROR errorCode) => MakeHRFromErrorCode((int)errorCode);

        public static int MakeHRFromErrorCode(int errorCode)
            => ((0xFFFF0000 & errorCode) != 0) ? errorCode : unchecked(((int)0x80070000) | errorCode);

        public static string GetMessage(int errorCode) => GetMessage(errorCode, IntPtr.Zero);
        public static string GetMessage(WIN32_ERROR errorCode) => GetMessage((int)errorCode, IntPtr.Zero);

        internal static unsafe string GetMessage(int errorCode, IntPtr moduleHandle)
        {
            var flags = FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_IGNORE_INSERTS |
                FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_FROM_SYSTEM |
                FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_ARGUMENT_ARRAY;

            if (moduleHandle != IntPtr.Zero)
            {
                flags |= FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_FROM_HMODULE;
            }

            // First try to format the message into the stack based buffer.  Most error messages willl fit.
            Span<char> stackBuffer = stackalloc char[256]; // arbitrary stack limit
            fixed (char* bufferPtr = stackBuffer)
            {
                var length = (int)FormatMessage(flags, moduleHandle.ToPointer(), unchecked((uint)errorCode), 0,
                    bufferPtr, (uint)stackBuffer.Length, null);
                if (length > 0)
                {
                    return GetAndTrimString(stackBuffer.Slice(0, length));
                }
            }

            // We got back an error.  If the error indicated that there wasn't enough room to store
            // the error message, then call FormatMessage again, but this time rather than passing in
            // a buffer, have the method allocate one, which we then need to free.
            if ((WIN32_ERROR)Marshal.GetLastWin32Error() == WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER)
            {
                void* nativeMsgPtr = default;
                try
                {
                    var length = (int)FormatMessage(flags | FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_ALLOCATE_BUFFER,
                        moduleHandle.ToPointer(), unchecked((uint)errorCode), 0, &nativeMsgPtr, 0, null);
                    if (length > 0)
                    {
                        return GetAndTrimString(new Span<char>((char*)nativeMsgPtr, length));
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(new IntPtr(nativeMsgPtr));
                }
            }

            // Couldn't get a message, so manufacture one.
            return $"Unknown error (0x{errorCode:x})";
        }

        private static string GetAndTrimString(Span<char> buffer)
        {
            int length = buffer.Length;
            while (length > 0 && buffer[length - 1] <= 32)
            {
                length--; // trim off spaces and non-printable ASCII chars at the end of the resource
            }

            return buffer.Slice(0, length).ToString();
        }

        internal static void SetFlag(this DCB dcb, int whichFlag, int setting)
        {
            uint mask;
            setting = setting << whichFlag;

            Debug.Assert(whichFlag >= DCBFlags.FBINARY && whichFlag <= DCBFlags.FDUMMY2, "SetDcbFlag needs to fit into enum!");

            if (whichFlag == DCBFlags.FDTRCONTROL || whichFlag == DCBFlags.FRTSCONTROL)
            {
                mask = 0x3;
            }
            else if (whichFlag == DCBFlags.FDUMMY2)
            {
                mask = 0x1FFFF;
            }
            else
            {
                mask = 0x1;
            }

            // clear the region
            dcb._bitfield &= ~(mask << whichFlag);

            // set the region
            dcb._bitfield |= ((uint)setting);
        }

        internal static int GetFlag(this DCB dcb, int whichFlag)
        {
            uint mask;

            Debug.Assert(whichFlag >= DCBFlags.FBINARY && whichFlag <= DCBFlags.FDUMMY2, "GetDcbFlag needs to fit into enum!");

            if (whichFlag == DCBFlags.FDTRCONTROL || whichFlag == DCBFlags.FRTSCONTROL)
            {
                mask = 0x3;
            }
            else if (whichFlag == DCBFlags.FDUMMY2)
            {
                mask = 0x1FFFF;
            }
            else
            {
                mask = 0x1;
            }

            uint result = dcb._bitfield & (mask << whichFlag);
            return (int)(result >> whichFlag);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool ClearCommError([In] IntPtr hFile,
            [Out, Optional] out CLEAR_COMM_ERROR_FLAGS lpErrors,
            [Out, Optional] out COMSTAT lpStat);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static unsafe extern bool WaitCommEvent(IntPtr hFile, COMM_EVENT_MASK* lpEvtMask, void* lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static unsafe extern bool GetOverlappedResult(IntPtr hFile,
            void* lpOverlapped,
            out uint lpNumberOfBytesTransferred,
            bool bWait);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int ReadFile(IntPtr handle, byte* bytes, uint numBytesToRead,
            byte* numBytesRead, System.Threading.NativeOverlapped* overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int WriteFile(IntPtr handle, byte* buffer,
            uint numBytesToWrite, byte* numBytesWritten, System.Threading.NativeOverlapped* lpOverlapped);
    }

    internal static class DCBFlags
    {
        // Since C# does not provide access to bitfields and the native DCB structure contains
        // a very necessary one, these are the positional offsets (from the right) of areas
        // of the 32-bit integer used in SerialStream's SetDcbFlag() and GetDcbFlag() methods.
        internal const int FBINARY = 0;
        internal const int FPARITY = 1;
        internal const int FOUTXCTSFLOW = 2;
        internal const int FOUTXDSRFLOW = 3;
        internal const int FDTRCONTROL = 4;
        internal const int FDSRSENSITIVITY = 6;
        internal const int FOUTX = 8;
        internal const int FINX = 9;
        internal const int FERRORCHAR = 10;
        internal const int FNULL = 11;
        internal const int FRTSCONTROL = 12;
        internal const int FABORTONOERROR = 14;
        internal const int FDUMMY2 = 15;
    }

    /*
        [StructLayout(LayoutKind.Sequential)]
        internal partial struct DCB
        {
            internal uint DCBlength;
            internal uint BaudRate;
            internal uint Bitfield;
            internal ushort WReserved;
            internal ushort XonLim;
            internal ushort XoffLim;
            internal byte ByteSize;
            internal byte Parity;
            internal byte StopBits;
            internal byte XonChar;
            internal byte XoffChar;
            internal byte ErrorChar;
            internal byte EofChar;
            internal byte EvtChar;
            internal ushort WReserved1;
        }
    */
}

#pragma warning restore CA1416 // Validate platform compatibility
