// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using Microsoft.Win32.SafeHandles;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Devices;
using Windows.Win32.Devices.Communication;
using Windows.Win32.Storage.FileSystem;

#pragma warning disable CA1416 // Validate platform compatibility

namespace System.Device.Ports.SerialPort
{
    internal partial class WindowsSerialPort : SerialPort
    {
        private const string DefaultPortName = "COM1";

        private readonly COMM_EVENT_MASK _allComEvents =
            COMM_EVENT_MASK.EV_RING |
            COMM_EVENT_MASK.EV_ERR |
            COMM_EVENT_MASK.EV_BREAK |
            COMM_EVENT_MASK.EV_RLSD |
            COMM_EVENT_MASK.EV_DSR |
            COMM_EVENT_MASK.EV_CTS |
            /*COMM_EVENT_MASK.EV_TXEMPTY |*/
            COMM_EVENT_MASK.EV_RXFLAG |
            COMM_EVENT_MASK.EV_RXCHAR;  // equivalent to 0x1FB

        private WindowsEventLoop? _eventLoop;
        private Task _waitForComEventTask;
        private ThreadPoolBoundHandle? _threadPoolBound;
        private SafeHandle? _portHandle;
        private COMMPROP _commProp;
        private COMSTAT _comStat;
        private DCB _dcb;
        private COMMTIMEOUTS _commTimeouts;

        public WindowsSerialPort()
        {
            _portName = DefaultPortName;
            _waitForComEventTask = Task.CompletedTask;
        }

        protected internal override void OpenPort()
        {
            _comStat = default;

            if (_portName == null ||
                !_portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase) ||
                !uint.TryParse(_portName.AsSpan(3), out uint portNumber))
            {
                throw new ArgumentException(Strings.WinPortName_wrong, nameof(PortName));
            }

            string devicePortName = @"\\?\COM" + portNumber.ToString(CultureInfo.InvariantCulture);
            SafeFileHandle handle = PInvoke.CreateFile(
                devicePortName,
                FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
                FILE_SHARE_MODE.FILE_SHARE_NONE,    // comm devices must be opened w/exclusive-access
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING, // comm devices must use OPEN_EXISTING
                FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_OVERLAPPED,
                null);

            if (handle.IsInvalid)
            {
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }

            try
            {
                uint fileType = PInvoke.GetFileType(handle);
                if ((fileType != PInvoke.FILE_TYPE_CHAR) && (fileType != PInvoke.FILE_TYPE_UNKNOWN))
                {
                    throw new ArgumentException(string.Format(Strings.Arg_InvalidSerialPort, PortName), nameof(PortName));
                }

                _portHandle = handle;
                _commProp = default;

                // GetCommModemStatus is called because it fails when it is not a legitimate serial device
                if (!PInvoke.GetCommProperties(_portHandle, ref _commProp)
                    || !PInvoke.GetCommModemStatus(_portHandle, out MODEM_STATUS_FLAGS _))
                {
                    // If the portName they have passed in is a FILE_TYPE_CHAR but not a serial port,
                    // for example "LPT1", this API will fail.  For this reason we handle the error message specially.
                    var errorCode = (WIN32_ERROR)Marshal.GetLastWin32Error();
                    if ((errorCode == WIN32_ERROR.ERROR_INVALID_PARAMETER) || (errorCode == WIN32_ERROR.ERROR_INVALID_HANDLE))
                    {
                        throw new ArgumentException(Strings.Arg_InvalidSerialPortExtended, nameof(PortName));
                    }
                    else
                    {
                        throw WindowsHelpers.GetExceptionForWin32Error(errorCode, string.Empty);
                    }
                }

                if (_commProp.dwMaxBaud != 0 && BaudRate > _commProp.dwMaxBaud)
                {
                    throw new ArgumentOutOfRangeException(nameof(BaudRate), string.Format(Strings.Max_Baud, _commProp.dwMaxBaud));
                }

                _dcb = default;
                InitializeDCB();
                // after the initialization rts in the DCB is false
                _rtsEnable = _dcb.GetFlag(DCBFlags.FRTSCONTROL) == PInvoke.RTS_CONTROL_ENABLE;
                SetRtsEnable(_rtsEnable);
                SetReadTimeout(ReadTimeout);

                _threadPoolBound = ThreadPoolBoundHandle.BindHandle(_portHandle);

                // monitor all events except TXEMPTY
                PInvoke.SetCommMask(_portHandle, _allComEvents);
                StartBackgroundLoop();
            }
            catch
            {
                _portHandle?.Close();
                _portHandle = null;
                _threadPoolBound?.Dispose();
                throw;
            }
        }

        private void StartBackgroundLoop()
        {
            if (_portHandle == null || _threadPoolBound == null)
            {
                return;
            }

            _eventLoop = new WindowsEventLoop(this, _portHandle, _threadPoolBound);
            _waitForComEventTask = _eventLoop.StartEventLoop();
        }

        private void StopEventLoop() => _eventLoop?.ShutdownEventLoop();

        private unsafe void InitializeDCB()
        {
            if (!PInvoke.GetCommState(_portHandle, out _dcb))
            {
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }

            _dcb.DCBlength = (uint)sizeof(DCB);
            _dcb.BaudRate = (uint)BaudRate;
            _dcb.ByteSize = (byte)DataBits;
            _dcb.Parity = (byte)Parity;
            _dcb.StopBits = StopBits switch
            {
                StopBits.One => (byte)PInvoke.ONESTOPBIT,
                StopBits.OnePointFive => (byte)PInvoke.ONE5STOPBITS,
                StopBits.Two => (byte)PInvoke.TWOSTOPBITS,
                _ => throw new ArgumentException(Strings.StopBits_Invalid),
            };

            _dcb.SetFlag(DCBFlags.FPARITY, ((Parity == Parity.None) ? 0 : 1));
            _dcb.SetFlag(DCBFlags.FBINARY, 1); // always true for communications resources
            _dcb.SetFlag(DCBFlags.FOUTXCTSFLOW, ((Handshake == Handshake.RequestToSend ||
                Handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0));

            // _dcb.SetFlag(DCBFlags.FOUTXDSRFLOW, (dsrTimeout != 0L) ? 1 : 0);
            _dcb.SetFlag(DCBFlags.FOUTXDSRFLOW, 0); // dsrTimeout is always set to 0.
            _dcb.SetFlag(DCBFlags.FDTRCONTROL, (int)PInvoke.DTR_CONTROL_DISABLE);
            _dcb.SetFlag(DCBFlags.FDSRSENSITIVITY, 0); // this should remain off
            _dcb.SetFlag(DCBFlags.FINX, (Handshake == Handshake.XOnXOff || Handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
            _dcb.SetFlag(DCBFlags.FOUTX, (Handshake == Handshake.XOnXOff || Handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);

            // if no parity, we have no error character (i.e. ErrorChar = '\0' or null character)
            if (Parity != Parity.None)
            {
                _dcb.SetFlag(DCBFlags.FERRORCHAR, (ParityReplace != '\0') ? 1 : 0);
                _dcb.ErrorChar = (CHAR)ParityReplace;
            }
            else
            {
                _dcb.SetFlag(DCBFlags.FERRORCHAR, 0);
                _dcb.ErrorChar = new CHAR((byte)'\0');
            }

            // this method only runs once in the constructor, so we only have the default value to use.
            // Later the user may change this via the NullDiscard property.
            _dcb.SetFlag(DCBFlags.FNULL, DiscardNull ? 1 : 0);

            // SerialStream does not handle the fAbortOnError behaviour, so we must make sure it's not enabled
            _dcb.SetFlag(DCBFlags.FABORTONOERROR, 0);

            // Setting RTS control, which is RTS_CONTROL_HANDSHAKE if RTS / RTS-XOnXOff handshaking
            // used, RTS_ENABLE (RTS pin used during operation) if rtsEnable true but XOnXoff / No handshaking
            // used, and disabled otherwise.
            if ((Handshake == Handshake.RequestToSend ||
                Handshake == Handshake.RequestToSendXOnXOff))
            {
                _dcb.SetFlag(DCBFlags.FRTSCONTROL, (int)PInvoke.RTS_CONTROL_HANDSHAKE);
            }
            else if (_dcb.GetFlag(DCBFlags.FRTSCONTROL) == (int)PInvoke.RTS_CONTROL_HANDSHAKE)
            {
                _dcb.SetFlag(DCBFlags.FRTSCONTROL, (int)PInvoke.RTS_CONTROL_DISABLE);
            }

            _dcb.XonChar = new CHAR(DefaultXONChar);
            _dcb.XoffChar = new CHAR(DefaultXOFFChar);

            // minimum number of bytes allowed in each buffer before flow control activated
            // heuristically, this has been set at 1/4 of the buffer size
            _dcb.XonLim = _dcb.XoffLim = (ushort)(_commProp.dwCurrentRxQueue / 4);

            _dcb.EofChar = new CHAR(EOFChar);
            _dcb.EvtChar = new CHAR(EOFChar);   // This value is used WaitCommEvent event

            // set DCB structure
            if (PInvoke.SetCommState(_portHandle, _dcb) == false)
            {
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        /// <summary>
        /// Stop the native events
        /// </summary>
        /// <returns>True when the SerialPort could be successfully accessed</returns>
        private bool StopEvents(out bool isSerialPortAccessible)
        {
            PInvoke.SetCommMask(_portHandle, 0);
            if (PInvoke.EscapeCommFunction(_portHandle, ESCAPE_COMM_FUNCTION.CLRDTR))
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

        protected internal override void ClosePort(bool disposing)
        {
            if (_portHandle == null || _portHandle.IsInvalid)
            {
                return;
            }

            try
            {
                StopEventLoop();
                Thread.MemoryBarrier();

                var success = StopEvents(out bool isPortAccessible);
                if (disposing && !success)
                {
                    throw WindowsHelpers.GetExceptionForLastWin32Error();
                }

                if (isPortAccessible && !_portHandle.IsClosed)
                {
                    Flush();
                }

                _eventLoop?.WaitCommEventWaitHandle.Set();

                if (isPortAccessible)
                {
                    DiscardInBuffer();
                    DiscardOutBuffer();
                }

                if (disposing && _eventLoop != null)
                {
                    _waitForComEventTask.GetAwaiter().GetResult();
                    _eventLoop?.WaitCommEventWaitHandle.Close();
                }
            }
            catch (Exception err)
            {
                if (disposing)
                {
                    Debug.WriteLine($"Exception caught in {nameof(ClosePort)}." +
                        $" It will {(disposing ? string.Empty : "not ")} be rethrown: {err.ToString()}");
                    throw;
                }
            }
            finally
            {
                if (disposing)
                {
                    lock (this)
                    {
                        _portHandle.Close();
                        _portHandle = null;
                        _threadPoolBound?.Dispose();
                        _threadPoolBound = null;
                    }
                }
                else
                {
                    _portHandle.Close();
                    _portHandle = null;
                    _threadPoolBound?.Dispose();
                    _threadPoolBound = null;
                }
            }
        }

        private void Flush()
        {
            if (_portHandle == null)
            {
                throw new InvalidOperationException(Strings.Port_not_open);
            }

            PInvoke.FlushFileBuffers(_portHandle);
        }

        protected internal override void SetBaudRate(int value)
        {
            if (_commProp.dwMaxBaud > 0 && value > _commProp.dwMaxBaud)
            {
                throw new ArgumentException(nameof(BaudRate),
                    string.Format(Strings.ArgumentOutOfRange_Bounds_Lower_Upper, 0, _commProp.dwMaxBaud));
            }

            var current = _dcb.BaudRate;
            _dcb.BaudRate = (uint)value;
            if (!PInvoke.SetCommState(_portHandle, _dcb))
            {
                _dcb.BaudRate = current;
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        protected internal override void SetParity(Parity value)
        {
            if (_dcb.Parity == (byte)value)
            {
                return;
            }

            var currentParity = _dcb.Parity;
            var currentErrorChar = _dcb.ErrorChar;
            var currentFlagParity = _dcb.GetFlag(DCBFlags.FPARITY);
            var currentFlagErrorChar = _dcb.GetFlag(DCBFlags.FERRORCHAR);

            _dcb.Parity = (byte)value;
            var parityFlag = value == Parity.None ? 0 : 1;
            _dcb.SetFlag(DCBFlags.FPARITY, parityFlag);
            if (parityFlag == 1)
            {
                _dcb.ErrorChar = new CHAR(ParityReplace);
                _dcb.SetFlag(DCBFlags.FERRORCHAR, ParityReplace != '\0' ? 1 : 0);
            }
            else
            {
                _dcb.ErrorChar = new CHAR((byte)'\0');
                _dcb.SetFlag(DCBFlags.FERRORCHAR, 0);
            }

            if (!PInvoke.SetCommState(_portHandle, _dcb))
            {
                _dcb.Parity = currentParity;
                _dcb.ErrorChar = currentErrorChar;
                _dcb.SetFlag(DCBFlags.FPARITY, currentFlagParity);
                _dcb.SetFlag(DCBFlags.FERRORCHAR, currentFlagErrorChar);
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        protected internal override void SetDataBits(int value)
        {
            if (_dcb.ByteSize == value)
            {
                return;
            }

            var currentByteSize = _dcb.ByteSize;
            _dcb.ByteSize = (byte)value;

            if (!PInvoke.SetCommState(_portHandle, _dcb))
            {
                _dcb.ByteSize = currentByteSize;
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        protected internal override void SetStopBits(StopBits value)
        {
            byte nativeValue = value switch
            {
                StopBits.One => (byte)PInvoke.ONESTOPBIT,
                StopBits.OnePointFive => (byte)PInvoke.ONE5STOPBITS,
                StopBits.Two => (byte)PInvoke.TWOSTOPBITS,
                _ => throw new ArgumentOutOfRangeException(nameof(StopBits), Strings.ArgumentOutOfRange_Enum),
            };

            byte currentValue = _dcb.StopBits;
            if (nativeValue == currentValue)
            {
                return;
            }

            _dcb.StopBits = nativeValue;
            if (PInvoke.SetCommState(_portHandle, _dcb))
            {
                _dcb.StopBits = currentValue;
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        protected internal override void SetBreakState(bool value)
        {
            BOOL result = value
                ? PInvoke.SetCommBreak(_portHandle)
                : PInvoke.ClearCommBreak(_portHandle);

            if (!result)
            {
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        private bool ClearCommError()
        {
            if (_portHandle == null || _portHandle.IsInvalid)
            {
                return false;
            }

            if (!WindowsHelpers.ClearCommError(_portHandle.DangerousGetHandle(),
                out CLEAR_COMM_ERROR_FLAGS _, out _comStat))
            {
                WindowsHelpers.GetExceptionForLastWin32Error();
            }

            return true;
        }

        protected internal unsafe override int GetBytesToRead()
        {
            if (!ClearCommError())
            {
                return 0;
            }

            return (int)_comStat.cbInQue;
        }

        protected internal override int GetBytesToWrite()
        {
            if (!ClearCommError())
            {
                return 0;
            }

            return (int)_comStat.cbOutQue;
        }

        private MODEM_STATUS_FLAGS GetPinStatus()
        {
            if (_portHandle == null || _portHandle.IsInvalid)
            {
                throw new InvalidOperationException(Strings.Port_not_open);
            }

            if (!PInvoke.GetCommModemStatus(_portHandle, out MODEM_STATUS_FLAGS pinStatus))
            {
                WindowsHelpers.GetExceptionForLastWin32Error();
            }

            return pinStatus;
        }

        protected internal override bool GetCDHolding()
        {
            var pinStatus = GetPinStatus();
            return (MODEM_STATUS_FLAGS.MS_RLSD_ON & pinStatus) != 0;
        }

        protected internal override bool GetCtsHolding()
        {
            var pinStatus = GetPinStatus();
            return (MODEM_STATUS_FLAGS.MS_CTS_ON & pinStatus) != 0;
        }

        protected internal override void SetDiscardNull(bool value)
        {
            var currentFlag = _dcb.GetFlag(DCBFlags.FNULL);
            var currentValue = currentFlag == 1;
            if (currentValue == value)
            {
                return;
            }

            _dcb.SetFlag(DCBFlags.FNULL, value ? 1 : 0);
            if (PInvoke.SetCommState(_portHandle, _dcb))
            {
                _dcb.SetFlag(DCBFlags.FNULL, currentFlag);
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        protected internal override bool GetDsrHolding()
        {
            var pinStatus = GetPinStatus();
            return (MODEM_STATUS_FLAGS.MS_DSR_ON & pinStatus) != 0;
        }

        protected internal override bool GetDtrEnable()
        {
            var dtrControl = _dcb.GetFlag(DCBFlags.FDTRCONTROL);
            return dtrControl == PInvoke.DTR_CONTROL_ENABLE;
        }

        protected internal override void SetDtrEnable(bool value)
        {
            var current = _dcb.GetFlag(DCBFlags.FDTRCONTROL);
            var newValue = (int)(value ? PInvoke.DTR_CONTROL_ENABLE : PInvoke.DTR_CONTROL_DISABLE);
            _dcb.SetFlag(DCBFlags.FDTRCONTROL, newValue);
            if (PInvoke.SetCommState(_portHandle, _dcb))
            {
                _dcb.SetFlag(DCBFlags.FDTRCONTROL, current);
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }

            var function = value ? ESCAPE_COMM_FUNCTION.SETDTR : ESCAPE_COMM_FUNCTION.CLRDTR;
            if (!PInvoke.EscapeCommFunction(_portHandle, function))
            {
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        protected internal override void SetHandshake(Handshake value)
        {
            var currentInOutX = _dcb.GetFlag(DCBFlags.FINX);
            var currentOutXCtsFlow = _dcb.GetFlag(DCBFlags.FOUTXCTSFLOW);
            var currentRtsControl = _dcb.GetFlag(DCBFlags.FRTSCONTROL);

            var inOutX = (value == Handshake.XOnXOff || value == Handshake.RequestToSendXOnXOff) ? 1 : 0;
            var outXCtsFlow = (value == Handshake.RequestToSend || value == Handshake.RequestToSendXOnXOff) ? 1 : 0;
            _dcb.SetFlag(DCBFlags.FINX, inOutX);
            _dcb.SetFlag(DCBFlags.FOUTX, inOutX);
            _dcb.SetFlag(DCBFlags.FOUTXCTSFLOW, outXCtsFlow);

            uint flow;
            if (value == Handshake.RequestToSend || value == Handshake.RequestToSendXOnXOff)
            {
                flow = PInvoke.RTS_CONTROL_HANDSHAKE;
            }
            else if (RtsEnable)
            {
                flow = PInvoke.RTS_CONTROL_ENABLE;
            }
            else
            {
                flow = PInvoke.RTS_CONTROL_DISABLE;
            }

            _dcb.SetFlag(DCBFlags.FRTSCONTROL, (int)flow);
            if (PInvoke.SetCommState(_portHandle, _dcb))
            {
                _dcb.SetFlag(DCBFlags.FINX, currentInOutX);
                _dcb.SetFlag(DCBFlags.FOUTX, currentInOutX);
                _dcb.SetFlag(DCBFlags.FOUTXCTSFLOW, currentOutXCtsFlow);
                _dcb.SetFlag(DCBFlags.FRTSCONTROL, currentRtsControl);
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        protected internal override void SetParityReplace(byte value)
        {
            var currentErrorChar = _dcb.ErrorChar;
            var currentFlagErrorChar = _dcb.GetFlag(DCBFlags.FERRORCHAR);

            if (_dcb.GetFlag(DCBFlags.FPARITY) == 1)
            {
                _dcb.SetFlag(DCBFlags.FERRORCHAR, value != '\0' ? 1 : 0);
                _dcb.ErrorChar = new CHAR(value);
            }
            else
            {
                _dcb.SetFlag(DCBFlags.FERRORCHAR, 0);
                _dcb.ErrorChar = new CHAR((byte)'\0');
            }

            if (PInvoke.SetCommState(_portHandle, _dcb))
            {
                _dcb.ErrorChar = currentErrorChar;
                _dcb.SetFlag(DCBFlags.FERRORCHAR, currentFlagErrorChar);
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        protected internal override void SetReadTimeout(int value)
        {
            var oldReadConstant = _commTimeouts.ReadTotalTimeoutConstant;
            var oldReadInterval = _commTimeouts.ReadIntervalTimeout;
            var oldReadMultipler = _commTimeouts.ReadTotalTimeoutMultiplier;

            if (value == 0)
            {
                _commTimeouts.ReadTotalTimeoutConstant = 0;
                _commTimeouts.ReadTotalTimeoutMultiplier = 0;
                _commTimeouts.ReadIntervalTimeout = PInvoke.MAXDWORD;
            }
            else if (value == SerialPort.InfiniteTimeout)
            {
                // we must use InfiniteCommTimeouts, see related comment
                _commTimeouts.ReadTotalTimeoutConstant = unchecked((uint)SerialPort.InfiniteCommTimeouts);
                _commTimeouts.ReadTotalTimeoutMultiplier = PInvoke.MAXDWORD;
                _commTimeouts.ReadIntervalTimeout = PInvoke.MAXDWORD;
            }
            else
            {
                _commTimeouts.ReadTotalTimeoutConstant = (uint)value;
                _commTimeouts.ReadTotalTimeoutMultiplier = PInvoke.MAXDWORD;
                _commTimeouts.ReadIntervalTimeout = PInvoke.MAXDWORD;
            }

            _commTimeouts.WriteTotalTimeoutMultiplier = 0;
            _commTimeouts.WriteTotalTimeoutConstant = (uint)(WriteTimeout == SerialPort.InfiniteTimeout ? 0 : WriteTimeout);

            if (PInvoke.SetCommTimeouts(_portHandle, _commTimeouts) == false)
            {
                _commTimeouts.ReadTotalTimeoutConstant = oldReadConstant;
                _commTimeouts.ReadTotalTimeoutMultiplier = oldReadMultipler;
                _commTimeouts.ReadIntervalTimeout = oldReadInterval;
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        protected internal override bool GetRtsEnable()
        {
            var flag = _dcb.GetFlag(DCBFlags.FRTSCONTROL);
            return flag == (int)PInvoke.RTS_CONTROL_ENABLE;
        }

        protected internal override void SetRtsEnable(bool value, bool setField = false)
        {
            var currentValue = _dcb.GetFlag(DCBFlags.FRTSCONTROL);
            if (value)
            {
                _dcb.SetFlag(DCBFlags.FRTSCONTROL, (int)PInvoke.RTS_CONTROL_ENABLE);
            }
            else
            {
                _dcb.SetFlag(DCBFlags.FRTSCONTROL, (int)PInvoke.RTS_CONTROL_DISABLE);
            }

            if (!PInvoke.SetCommState(_portHandle, _dcb))
            {
                // if fails, restore old value
                _dcb.SetFlag(DCBFlags.FRTSCONTROL, currentValue);
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }

            var escapeFunction = value ? ESCAPE_COMM_FUNCTION.SETRTS : ESCAPE_COMM_FUNCTION.CLRRTS;
            if (!PInvoke.EscapeCommFunction(_portHandle, escapeFunction))
            {
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }

            if (setField)
            {
                _rtsEnable = value;
            }
        }

        protected internal override void SetWriteBufferSize(int value)
        {
            /* There is no specific API to call in Windows */
        }

        protected internal override void SetWriteTimeout(int value)
        {
            var currentWriteTimeout = _commTimeouts.WriteTotalTimeoutConstant;
            _commTimeouts.WriteTotalTimeoutConstant = (uint)(value == InfiniteTimeout ? 0 : value);

            if (PInvoke.SetCommTimeouts(_portHandle, _commTimeouts) == false)
            {
                _commTimeouts.WriteTotalTimeoutConstant = currentWriteTimeout;
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        public override void DiscardInBuffer()
        {
            if (PInvoke.PurgeComm(_portHandle, PURGE_COMM_FLAGS.PURGE_RXCLEAR | PURGE_COMM_FLAGS.PURGE_RXABORT) == false)
            {
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        public override void DiscardOutBuffer()
        {
            if (PInvoke.PurgeComm(_portHandle, PURGE_COMM_FLAGS.PURGE_TXCLEAR | PURGE_COMM_FLAGS.PURGE_TXABORT) == false)
            {
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected internal override void InitializeBuffers(int readBufferSize, int writeBufferSize)
        {
            if (!PInvoke.SetupComm(_portHandle, (uint)readBufferSize, (uint)writeBufferSize))
            {
                throw WindowsHelpers.GetExceptionForLastWin32Error();
            }
        }
    }
}

#pragma warning restore CA1416 // Validate platform compatibility
