// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Ports.SerialPort.Windows;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Devices.Communication;

#pragma warning disable CA1416 // Validate platform compatibility

namespace System.Device.Ports.SerialPort.Windows2
{
    internal class WindowsInterop
    {
        /// <summary>
        /// Returns a DCB structure set-up with the given parameters
        /// </summary>
        /// <param name="portHandle">The opened serial port</param>
        /// <param name="baudRate">The speed of the serial port</param>
        /// <param name="parity">The parity used in the handshake</param>
        /// <param name="dataBits">The number of data bits</param>
        /// <param name="stopBits">The number of stop bits</param>
        /// <param name="handshake">The handshake for the serial port</param>
        /// <param name="parityReplace">The byte meant to replace parity errors</param>
        /// <param name="discardNull">True to discard nulls</param>
        /// <param name="inputBufferSize">This can be obtained from CommProp.dwCurrentRxQueue</param>
        /// <returns>A DCB structure</returns>
        /// <exception cref="ArgumentException"></exception>
        internal static unsafe DCB InitializeDCB(SafeHandle portHandle,
            int baudRate, Parity parity, int dataBits, StopBits stopBits,
            Handshake handshake, byte parityReplace, bool discardNull,
            uint inputBufferSize)
        {
            DCB dcb;
            if (!PInvoke.GetCommState(portHandle, out dcb))
            {
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }

            dcb.DCBlength = (uint)sizeof(DCB);
            dcb.BaudRate = (uint)baudRate;
            dcb.Parity = (byte)parity;
            dcb.ByteSize = (byte)dataBits;
            dcb.StopBits = stopBits switch
            {
                StopBits.One => (byte)PInvoke.ONESTOPBIT,
                StopBits.OnePointFive => (byte)PInvoke.ONE5STOPBITS,
                StopBits.Two => (byte)PInvoke.TWOSTOPBITS,
                _ => throw new ArgumentException(Strings.StopBits_Invalid),
            };

            dcb.SetFlag(DCBFlags.FPARITY, ((parity == Parity.None) ? 0 : 1));
            dcb.SetFlag(DCBFlags.FBINARY, 1); // always true for communications resources
            dcb.SetFlag(DCBFlags.FOUTXCTSFLOW,
                (handshake == Handshake.RequestToSend ||
                handshake == Handshake.RequestToSendXOnXOff)
                ? 1 : 0);

            // _dcb.SetFlag(DCBFlags.FOUTXDSRFLOW, (dsrTimeout != 0L) ? 1 : 0);
            dcb.SetFlag(DCBFlags.FOUTXDSRFLOW, 0); // dsrTimeout is always set to 0.
            dcb.SetFlag(DCBFlags.FDTRCONTROL, (int)PInvoke.DTR_CONTROL_DISABLE);
            dcb.SetFlag(DCBFlags.FDSRSENSITIVITY, 0); // this should remain off
            dcb.SetFlag(DCBFlags.FINX, (handshake == Handshake.XOnXOff || handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
            dcb.SetFlag(DCBFlags.FOUTX, (handshake == Handshake.XOnXOff || handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);

            // if no parity, we have no error character (i.e. ErrorChar = '\0' or null character)
            if (parity != Parity.None)
            {
                dcb.SetFlag(DCBFlags.FERRORCHAR, (parityReplace != '\0') ? 1 : 0);
                dcb.ErrorChar = (CHAR)parityReplace;
            }
            else
            {
                dcb.SetFlag(DCBFlags.FERRORCHAR, 0);
                dcb.ErrorChar = new CHAR((byte)'\0');
            }

            // this method only runs once in the constructor, so we only have the default value to use.
            // Later the user may change this via the NullDiscard property.
            dcb.SetFlag(DCBFlags.FNULL, discardNull ? 1 : 0);

            // SerialStream does not handle the fAbortOnError behaviour, so we must make sure it's not enabled
            dcb.SetFlag(DCBFlags.FABORTONOERROR, 0);

            // Setting RTS control, which is RTS_CONTROL_HANDSHAKE if RTS / RTS-XOnXOff handshaking
            // used, RTS_ENABLE (RTS pin used during operation) if rtsEnable true but XOnXoff / No handshaking
            // used, and disabled otherwise.
            if (handshake == Handshake.RequestToSend || handshake == Handshake.RequestToSendXOnXOff)
            {
                dcb.SetFlag(DCBFlags.FRTSCONTROL, (int)PInvoke.RTS_CONTROL_HANDSHAKE);
            }
            else if (dcb.GetFlag(DCBFlags.FRTSCONTROL) == (int)PInvoke.RTS_CONTROL_HANDSHAKE)
            {
                dcb.SetFlag(DCBFlags.FRTSCONTROL, (int)PInvoke.RTS_CONTROL_DISABLE);
            }

            dcb.XonChar = new CHAR((byte)SerialPortCharacters.DefaultXONChar);
            dcb.XoffChar = new CHAR((byte)SerialPortCharacters.DefaultXOFFChar);

            // minimum number of bytes allowed in each buffer before flow control activated
            // heuristically, this has been set at 1/4 of the buffer size
            dcb.XonLim = dcb.XoffLim = (ushort)(inputBufferSize / 4);

            dcb.EofChar = new CHAR((byte)SerialPortCharacters.EOFChar);
            dcb.EvtChar = new CHAR((byte)SerialPortCharacters.EOFChar);   // This value is used WaitCommEvent event

            // set DCB structure
            if (PInvoke.SetCommState(portHandle, dcb) == false)
            {
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }

            return dcb;
        }

        internal static bool StopEvents(SafeHandle portHandle, out bool isSerialPortAccessible)
        {
            PInvoke.SetCommMask(portHandle, 0);
            if (PInvoke.EscapeCommFunction(portHandle, ESCAPE_COMM_FUNCTION.CLRDTR))
            {
                // return on success
                isSerialPortAccessible = true;
                return true;
            }

            isSerialPortAccessible = false;
            int hr = Marshal.GetLastWin32Error();
            Debug.Fail($"Unexpected error code from EscapeCommFunction in {nameof(StopEvents)}  Error code: 0x{(uint)hr:x}");

            // access denied can happen if USB is yanked out. If that happens, we
            // want to at least allow finalize to succeed and clean up everything
            // we can. To achieve this, we need to avoid further attempts to access
            // the SerialPort.  A customer also reported seeing ERROR_BAD_COMMAND here.
            // Do not throw an exception on the finalizer thread - that's just rude,
            // since apps can't catch it and we may tear down the app.
            if ((hr == (uint)WIN32_ERROR.ERROR_ACCESS_DENIED ||
                hr == (uint)WIN32_ERROR.ERROR_BAD_COMMAND ||
                hr == (uint)WIN32_ERROR.ERROR_DEVICE_REMOVED))
            {
                // throwing is not necessary when these errors occur
                return true;
            }

            return false;
        }

    }
}

#pragma warning restore CA1416 // Validate platform compatibility
