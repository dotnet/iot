// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// Error Messages
    /// </summary>
    internal class SRMessages
    {
        /// <summary>Unable to read beyond the end of the stream.</summary>
        internal static string @IO_EOF_ReadBeyondEOF => GetResourceString("IO_EOF_ReadBeyondEOF", @"Unable to read beyond the end of the stream.");

        /// <summary>The BaseStream is only available when the port is open.</summary>
        internal static string @BaseStream_Invalid_Not_Open => GetResourceString("BaseStream_Invalid_Not_Open", @"The BaseStream is only available when the port is open.");

        /// <summary>The PortName cannot be empty.</summary>
        internal static string @PortNameEmpty_String => GetResourceString("PortNameEmpty_String", @"The PortName cannot be empty.");

        /// <summary>The port is closed.</summary>
        internal static string @Port_not_open => GetResourceString("Port_not_open", @"The port is closed.");

        /// <summary>The port is already open.</summary>
        internal static string @Port_already_open => GetResourceString("Port_already_open", @"The port is already open.");

        /// <summary>'{0}' cannot be set while the port is open.</summary>
        internal static string @Cant_be_set_when_open => GetResourceString("Cant_be_set_when_open", @"'{0}' cannot be set while the port is open.");

        /// <summary>The maximum baud rate for the device is {0}.</summary>
        internal static string @Max_Baud => GetResourceString("Max_Baud", @"The maximum baud rate for the device is {0}.");

        /// <summary>The port is in the break state and cannot be written to.</summary>
        internal static string @In_Break_State => GetResourceString("In_Break_State", @"The port is in the break state and cannot be written to.");

        /// <summary>The write timed out.</summary>
        internal static string @Write_timed_out => GetResourceString("Write_timed_out", @"The write timed out.");

        /// <summary>RtsEnable cannot be accessed if Handshake is set to RequestToSend or RequestToSendXOnXOff.</summary>
        internal static string @CantSetRtsWithHandshaking => GetResourceString("CantSetRtsWithHandshaking", @"RtsEnable cannot be accessed if Handshake is set to RequestToSend or RequestToSendXOnXOff.");

        /// <summary>SerialPort does not support encoding '{0}'.  The supported encodings include ASCIIEncoding, UTF8Encoding, UnicodeEncoding, UTF32Encoding, and most single or double byte code pages.  For a complete list please see the documentation.</summary>
        internal static string @NotSupportedEncoding => GetResourceString("NotSupportedEncoding", @"SerialPort does not support encoding '{0}'.  The supported encodings include ASCIIEncoding, UTF8Encoding, UnicodeEncoding, UTF32Encoding, and most single or double byte code pages.  For a complete list please see the documentation.");

        /// <summary>The given port name ({0}) does not resolve to a valid serial port.</summary>
        internal static string @Arg_InvalidSerialPort => GetResourceString("Arg_InvalidSerialPort", @"The given port name ({0}) does not resolve to a valid serial port.");

        /// <summary>The given port name is invalid.  It may be a valid port, but not a serial port.</summary>
        internal static string @Arg_InvalidSerialPortExtended => GetResourceString("Arg_InvalidSerialPortExtended", @"The given port name is invalid.  It may be a valid port, but not a serial port.");

        /// <summary>Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.</summary>
        internal static string @Argument_InvalidOffLen => GetResourceString("Argument_InvalidOffLen", @"Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

        /// <summary>Argument must be between {0} and {1}.</summary>
        internal static string @ArgumentOutOfRange_Bounds_Lower_Upper => GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", @"Argument must be between {0} and {1}.");

        /// <summary>Enum value was out of legal range.</summary>
        internal static string @ArgumentOutOfRange_Enum => GetResourceString("ArgumentOutOfRange_Enum", @"Enum value was out of legal range.");

        /// <summary>Non-negative number required.</summary>
        internal static string @ArgumentOutOfRange_NeedNonNegNumRequired => GetResourceString("ArgumentOutOfRange_NeedNonNegNumRequired", @"Non-negative number required.");

        /// <summary>Positive number required.</summary>
        internal static string @ArgumentOutOfRange_NeedPosNum => GetResourceString("ArgumentOutOfRange_NeedPosNum", @"Positive number required.");

        /// <summary>The timeout must be greater than or equal to -1.</summary>
        internal static string @ArgumentOutOfRange_Timeout => GetResourceString("ArgumentOutOfRange_Timeout", @"The timeout must be greater than or equal to -1.");

        /// <summary>The timeout must be either a positive number or -1.</summary>
        internal static string @ArgumentOutOfRange_WriteTimeout => GetResourceString("ArgumentOutOfRange_WriteTimeout", @"The timeout must be either a positive number or -1.");

        /// <summary>Probable I/O race condition detected while copying memory. The I/O package is not thread safe by default. In multithreaded applications, a stream must be accessed in a thread-safe way, such as a thread-safe wrapper returned by TextReader's or TextWriter's  ...</summary>
        internal static string @IndexOutOfRange_IORaceCondition => GetResourceString("IndexOutOfRange_IORaceCondition", @"Probable I/O race condition detected while copying memory. The I/O package is not thread safe by default. In multithreaded applications, a stream must be accessed in a thread-safe way, such as a thread-safe wrapper returned by TextReader's or TextWriter's Synchronized methods. This also applies to classes like StreamWriter and StreamReader.");

        /// <summary>The I/O operation has been aborted because of either a thread exit or an application request.</summary>
        internal static string @IO_OperationAborted => GetResourceString("IO_OperationAborted", @"The I/O operation has been aborted because of either a thread exit or an application request.");

        /// <summary>Stream does not support seeking.</summary>
        internal static string @NotSupported_UnseekableStream => GetResourceString("NotSupported_UnseekableStream", @"Stream does not support seeking.");

        /// <summary>Cannot access a closed stream.</summary>
        internal static string @ObjectDisposed_StreamClosed => GetResourceString("ObjectDisposed_StreamClosed", @"Cannot access a closed stream.");

        /// <summary>Argument {0} cannot be null or zero-length.</summary>
        internal static string @InvalidNullEmptyArgument => GetResourceString("InvalidNullEmptyArgument", @"Argument {0} cannot be null or zero-length.");

        /// <summary>IAsyncResult object did not come from the corresponding async method on this type.</summary>
        internal static string @Arg_WrongAsyncResult => GetResourceString("Arg_WrongAsyncResult", @"IAsyncResult object did not come from the corresponding async method on this type.");

        /// <summary>EndRead can only be called once for each asynchronous operation.</summary>
        internal static string @InvalidOperation_EndReadCalledMultiple => GetResourceString("InvalidOperation_EndReadCalledMultiple", @"EndRead can only be called once for each asynchronous operation.");

        /// <summary>EndWrite can only be called once for each asynchronous operation.</summary>
        internal static string @InvalidOperation_EndWriteCalledMultiple => GetResourceString("InvalidOperation_EndWriteCalledMultiple", @"EndWrite can only be called once for each asynchronous operation.");

        /// <summary>Access to the port '{0}' is denied.</summary>
        internal static string @UnauthorizedAccess_IODenied_Port => GetResourceString("UnauthorizedAccess_IODenied_Port", @"Access to the port '{0}' is denied.");

        /// <summary>System.IO.Ports is currently only supported on Windows.</summary>
        internal static string @PlatformNotSupported_IOPorts => GetResourceString("PlatformNotSupported_IOPorts", @"System.IO.Ports is currently only supported on Windows.");

        /// <summary>Enumeration of serial port names is not supported on the current platform.</summary>
        internal static string @PlatformNotSupported_SerialPort_GetPortNames => GetResourceString("PlatformNotSupported_SerialPort_GetPortNames", @"Enumeration of serial port names is not supported on the current platform.");

        /// <summary>The specified file name or path is too long, or a component of the specified path is too long.</summary>
        internal static string @IO_PathTooLong => GetResourceString("IO_PathTooLong", @"The specified file name or path is too long, or a component of the specified path is too long.");

        /// <summary>Could not find a part of the path.</summary>
        internal static string @IO_PathNotFound_NoPathName => GetResourceString("IO_PathNotFound_NoPathName", @"Could not find a part of the path.");

        /// <summary>Could not find a part of the path '{0}'.</summary>
        internal static string @IO_PathNotFound_Path => GetResourceString("IO_PathNotFound_Path", @"Could not find a part of the path '{0}'.");

        /// <summary>Unable to find the specified file.</summary>
        internal static string @IO_FileNotFound => GetResourceString("IO_FileNotFound", @"Unable to find the specified file.");

        /// <summary>Could not find file '{0}'.</summary>
        internal static string @IO_FileNotFound_FileName => GetResourceString("IO_FileNotFound_FileName", @"Could not find file '{0}'.");

        /// <summary>Access to the path is denied.</summary>
        internal static string @UnauthorizedAccess_IODenied_NoPathName => GetResourceString("UnauthorizedAccess_IODenied_NoPathName", @"Access to the path is denied.");

        /// <summary>Access to the path '{0}' is denied.</summary>
        internal static string @UnauthorizedAccess_IODenied_Path => GetResourceString("UnauthorizedAccess_IODenied_Path", @"Access to the path '{0}' is denied.");

        /// <summary>The path '{0}' is too long, or a component of the specified path is too long.</summary>
        internal static string @IO_PathTooLong_Path => GetResourceString("IO_PathTooLong_Path", @"The path '{0}' is too long, or a component of the specified path is too long.");

        /// <summary>The process cannot access the file '{0}' because it is being used by another process.</summary>
        internal static string @IO_SharingViolation_File => GetResourceString("IO_SharingViolation_File", @"The process cannot access the file '{0}' because it is being used by another process.");

        /// <summary>The process cannot access the file because it is being used by another process.</summary>
        internal static string @IO_SharingViolation_NoFileName => GetResourceString("IO_SharingViolation_NoFileName", @"The process cannot access the file because it is being used by another process.");

        /// <summary>Specified file length was too large for the file system.</summary>
        internal static string @ArgumentOutOfRange_FileLengthTooBig => GetResourceString("ArgumentOutOfRange_FileLengthTooBig", @"Specified file length was too large for the file system.");

        /// <summary>The file '{0}' already exists.</summary>
        internal static string @IO_FileExists_Name => GetResourceString("IO_FileExists_Name", @"The file '{0}' already exists.");

        /// <summary>Cannot create '{0}' because a file or directory with the same name already exists.</summary>
        internal static string @IO_AlreadyExists_Name => GetResourceString("IO_AlreadyExists_Name", @"Cannot create '{0}' because a file or directory with the same name already exists.");

        private static string GetResourceString(string messageId, string defaultMessage)
            => defaultMessage;

    }
}
