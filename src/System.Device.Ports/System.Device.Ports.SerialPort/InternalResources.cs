// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Device.Ports.SerialPort.Resources;
using System.IO;

using Marshal = System.Runtime.InteropServices.Marshal;

namespace System.Device.Ports.SerialPort
{
    internal static partial class InternalResources
    {
        // Beginning of static Error methods
        internal static void EndOfFile()
        {
            throw new EndOfStreamException(Strings.IO_EOF_ReadBeyondEOF);
        }

        internal static string GetMessage(int errorCode)
        {
            return new Win32Exception(errorCode).Message;
        }

        internal static Exception FileNotOpenException()
        {
            return new ObjectDisposedException(null, Strings.Port_not_open);
        }

        internal static void FileNotOpen()
        {
            throw FileNotOpenException();
        }

        internal static void WrongAsyncResult()
        {
            throw new ArgumentException(Strings.Arg_WrongAsyncResult);
        }

        internal static void EndReadCalledTwice()
        {
            // Should ideally be InvalidOperationExc but we can't maintain parity with Stream and SerialStream without some work
            throw new ArgumentException(Strings.InvalidOperation_EndReadCalledMultiple);
        }

        internal static void EndWriteCalledTwice()
        {
            // Should ideally be InvalidOperationExc but we can't maintain parity with Stream and SerialStream without some work
            throw new ArgumentException(Strings.InvalidOperation_EndWriteCalledMultiple);
        }
    }
}
