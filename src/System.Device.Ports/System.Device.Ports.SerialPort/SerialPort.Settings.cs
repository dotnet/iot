// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
Handshake
ParityReplace
BreakState
DiscardNull
ReadTimeout
WriteTimeout

DtrEnable
RtsEnable

CDHolding
CtsHolding
DsrHolding

*/

using System.Text;

namespace System.Device.Ports.SerialPort
{
    public partial class SerialPort
    {
        /// <summary>
        /// Indicates that no time-out should occur.
        /// </summary>
        public const int InfiniteTimeout = -1;

        /// <summary>
        /// Indicates that no time-out should occur.
        /// This value is used for the SetCommTimeouts
        /// which does not accept -1
        /// </summary>
        protected const int InfiniteCommTimeouts = -2;

        private const int MaxDataBitsNoParity = 9;
        private const int MinDataBits = 5;

        internal const int DefaultBaudRate = 9600;
        internal const Parity DefaultParity = Parity.None;
        internal const int DefaultDataBits = 8;
        internal const StopBits DefaultStopBits = StopBits.One;
        internal const Handshake DefaultHandshake = Handshake.None;
        internal const bool DefaultDtrEnable = false;
        internal const bool DefaultRtsEnable = false;
        internal const bool DefaultDiscardNull = false;
        internal const byte DefaultParityReplace = (byte)SerialPortCharacters.QuestionMark;
        internal const int DefaultReadBufferSize = 4096;
        internal const int DefaultWriteBufferSize = 2048;
        /*private const int DefaultBufferSize = 1024;*/

        private const int DefaultReceivedBytesThreshold = 1;
        private const int DefaultReadTimeout = InfiniteTimeout;
        private const int DefaultWriteTimeout = InfiniteTimeout;

        private int _baudRate;
        private Parity _parity;
        private int _dataBits;
        private StopBits _stopBits;
        private bool _breakState;
        private bool _discardNull = DefaultDiscardNull;
        private bool _dtrEnable = DefaultDtrEnable;
        private Handshake _handshake = DefaultHandshake;
        private byte _parityReplace = DefaultParityReplace;

        /// <summary>
        /// The name of the serial port whose default value is platform dependent
        /// and set to a proper default name in the derived platform-specific classes
        /// </summary>
        private string _portName = string.Empty;

        /// <summary>
        /// The field caching the value for the RTS enable flag
        /// </summary>
        protected bool _rtsEnable = DefaultRtsEnable;

        private int _readBufferSize = DefaultReadBufferSize;
        private int _readTimeout = DefaultReadTimeout;
        private int _receivedBytesThreshold = DefaultReceivedBytesThreshold;
        private int _writeBufferSize = DefaultWriteBufferSize;
        private int _writeTimeout = DefaultWriteTimeout;

        /// <summary>
        /// The baud rate
        /// </summary>
        public int BaudRate
        {
            get => _baudRate;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(BaudRate), Strings.ArgumentOutOfRange_NeedPosNum);
                }

                if (value == _baudRate)
                {
                    return;
                }

                if (IsOpen)
                {
                    SetBaudRate(value);
                }

                _baudRate = value;
            }
        }

        /// <summary>
        /// Set the baud rate
        /// </summary>
        /// <param name="value">The baud rate to set</param>
        protected internal abstract void SetBaudRate(int value);

        /// <summary>
        /// The parity
        /// </summary>
        public Parity Parity
        {
            get => _parity;
            set
            {
                if (value < Parity.None || value > Parity.Space)
                {
                    throw new ArgumentOutOfRangeException(nameof(Parity), Strings.ArgumentOutOfRange_Enum);
                }

                if (value == _parity)
                {
                    return;
                }

                if (IsOpen)
                {
                    SetParity(value);
                }

                _parity = value;
            }
        }

        /// <summary>
        /// Set the communication parity
        /// </summary>
        /// <param name="value">The parity value to set</param>
        protected internal abstract void SetParity(Parity value);

        /// <summary>
        /// The data bits
        /// </summary>
        public int DataBits
        {
            get => _dataBits;
            set
            {
                // 9 data bit is only supported by toggling the parity bit
                if (value < MinDataBits || value > MaxDataBitsNoParity ||
                    (value == MaxDataBitsNoParity && Parity != Parity.None))
                {
                    throw new ArgumentOutOfRangeException(nameof(DataBits), Strings.InvalidDataBits);
                }

                if (value == _dataBits)
                {
                    return;
                }

                if (IsOpen)
                {
                    SetDataBits(value);
                }

                _dataBits = value;
            }
        }

        /// <summary>
        /// Set the communication data bits
        /// </summary>
        /// <param name="value">The data bits value to set</param>
        protected internal abstract void SetDataBits(int value);

        /// <summary>
        /// The stop bits
        /// </summary>
        public StopBits StopBits
        {
            get => _stopBits;
            set
            {
                if (value < StopBits.One || value > StopBits.OnePointFive)
                {
                    throw new ArgumentOutOfRangeException(nameof(StopBits), Strings.ArgumentOutOfRange_Enum);
                }

                if (value == _stopBits)
                {
                    return;
                }

                if (IsOpen)
                {
                    SetStopBits(value);
                }

                _stopBits = value;
            }
        }

        /// <summary>
        /// Set the communication stop bits
        /// </summary>
        /// <param name="value">The stop bits value to set</param>
        protected internal abstract void SetStopBits(StopBits value);

        /// <summary>
        /// Gets or sets the break signal state.
        /// </summary>
        public bool BreakState
        {
            get => _breakState;
            set
            {
                // In the old implementation SetBreakState is set every time using platform specific code
                // for this reason, this code is commented but I left it here because the old behavior
                // is still to be confirmed.
                /*
                if (value == _breakState)
                {
                    return;
                }
                */

                // todo: throw if not open, get and set
                SetBreakState(value);
                _breakState = value;
            }
        }

        /// <summary>
        /// Sets the break signal state.
        /// </summary>
        /// <param name="value">true if the port is in a break state; otherwise, false.</param>
        protected internal abstract void SetBreakState(bool value);

        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        public int BytesToRead
        {
            get
            {
                if (!IsOpen)
                {
                    throw new InvalidOperationException(Strings.Port_not_open);
                }

                // todo: throw if not open, get and set
                return GetBytesToRead();
            }
        }

        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        protected internal abstract int GetBytesToRead();

        /// <summary>
        /// Gets the number of bytes of data in the send buffer.
        /// </summary>
        public int BytesToWrite
        {
            get
            {
                if (!IsOpen)
                {
                    throw new InvalidOperationException(Strings.Port_not_open);
                }

                // todo: throw if not open, get and set
                return GetBytesToWrite();
            }
        }

        /// <summary>
        /// Gets the number of bytes of data in the send buffer.
        /// </summary>
        protected internal abstract int GetBytesToWrite();

        /// <summary>
        /// Gets the state of the Carrier Detect line for the port.
        /// </summary>
        public bool CDHolding
        {
            get
            {
                if (!IsOpen)
                {
                    throw new InvalidOperationException(Strings.Port_not_open);
                }

                // todo: throw if not open, get and set
                return GetCDHolding();
            }
        }

        /// <summary>
        /// Gets the state of the Carrier Detect line for the port.
        /// </summary>
        protected internal abstract bool GetCDHolding();

        /// <summary>
        /// Gets the state of the Clear-to-Send line.
        /// </summary>
        public bool CtsHolding
        {
            get
            {
                if (!IsOpen)
                {
                    throw new InvalidOperationException(Strings.Port_not_open);
                }

                // todo: throw if not open, get and set
                return GetCtsHolding();
            }
        }

        /// <summary>
        /// Gets the state of the Clear-to-Send line.
        /// </summary>
        protected internal abstract bool GetCtsHolding();

        /// <summary>
        /// Gets or sets a value indicating whether null bytes are ignored when
        /// transmitted between the port and the receive buffer.
        /// </summary>
        public bool DiscardNull
        {
            get => _discardNull;
            set
            {
                if (IsOpen)
                {
                    SetDiscardNull(value);
                }

                _discardNull = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether null bytes are ignored when
        /// transmitted between the port and the receive buffer.
        /// </summary>
        /// <param name="value">true if null bytes are ignored; otherwise false. The default is false.</param>
        protected internal abstract void SetDiscardNull(bool value);

        /// <summary>
        /// Gets the state of the Data Set Ready (DSR) signal.
        /// </summary>
        public bool DsrHolding
        {
            get
            {
                if (!IsOpen)
                {
                    throw new InvalidOperationException(Strings.Port_not_open);
                }

                // todo: throw if not open
                return GetDsrHolding();
            }
        }

        /// <summary>
        /// Gets the state of the Data Set Ready (DSR) signal.
        /// </summary>
        protected internal abstract bool GetDsrHolding();

        /// <summary>
        /// Gets or sets a value that enables the Data Terminal Ready (DTR) signal during serial communication.
        /// </summary>
        public bool DtrEnable
        {
            get
            {
                if (IsOpen)
                {
                    _dtrEnable = GetDtrEnable();
                }

                return _dtrEnable;
            }
            set
            {
                if (!IsOpen)
                {
                    throw new InvalidOperationException(Strings.Port_not_open);
                }

                SetDtrEnable(value);
                _dtrEnable = value;
            }
        }

        /// <summary>
        /// Gets a value that enables the Data Terminal Ready (DTR) signal during serial communication.
        /// </summary>
        protected internal abstract bool GetDtrEnable();

        /// <summary>
        /// Sets a value that enables the Data Terminal Ready (DTR) signal during serial communication.
        /// </summary>
        /// <param name="value">true to enable Data Terminal Ready (DTR); otherwise, false. The default is false.</param>
        protected internal abstract void SetDtrEnable(bool value);

        /// <summary>
        /// Gets or sets the handshaking protocol for serial port transmission of data using a value from Handshake.
        /// </summary>
        public Handshake Handshake
        {
            get => _handshake;
            set
            {
                if (value < Handshake.None || value > Handshake.RequestToSendXOnXOff)
                {
                    throw new ArgumentOutOfRangeException(nameof(Handshake), Strings.ArgumentOutOfRange_Enum);
                }

                if (value == _handshake)
                {
                    return;
                }

                if (IsOpen)
                {
                    SetHandshake(value);
                }

                _handshake = value;
            }
        }

        /// <summary>
        /// Gets or sets the handshaking protocol for serial port transmission of data using a value from Handshake.
        /// </summary>
        /// <param name="value">One of the Handshake values. The default is None.</param>
        protected internal abstract void SetHandshake(Handshake value);

        /// <summary>
        /// Gets a value indicating the open or closed status of the SerialPort object.
        /// </summary>
        public bool IsOpen { get; protected set; }

        /// <summary>
        /// Gets or sets the byte that replaces invalid bytes in a data stream when a parity error occurs.
        /// </summary>
        public byte ParityReplace
        {
            get => _parityReplace;
            set
            {
                if (value == _parityReplace)
                {
                    return;
                }

                if (IsOpen)
                {
                    SetParityReplace(value);
                }

                _parityReplace = value;
            }
        }

        /// <summary>
        /// Gets or sets the byte that replaces invalid bytes in a data stream when a parity error occurs.
        /// </summary>
        /// <param name="value">A byte that replaces invalid bytes.</param>
        protected internal abstract void SetParityReplace(byte value);

        /// <summary>
        /// Gets or sets the port name
        /// </summary>
        public string PortName
        {
            get => _portName;
            set
            {
                if (value == _portName)
                {
                    return;
                }

                if (_portName == null)
                {
                    throw new ArgumentNullException(nameof(PortName));
                }

                if (value.Length == 0)
                {
                    throw new ArgumentException(string.Format(Strings.EmptyString, nameof(PortName)));
                }

                if (IsOpen)
                {
                    throw new InvalidOperationException(string.Format(Strings.Cant_be_set_when_open, nameof(PortName)));
                }

                _portName = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the SerialPort input buffer.
        /// </summary>
        public int ReadBufferSize
        {
            get
            {
                return _readBufferSize;
            }
            set
            {
                if (value == _readBufferSize)
                {
                    return;
                }

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(ReadBufferSize));
                }

                if (IsOpen)
                {
                    throw new InvalidOperationException(string.Format(Strings.Cant_be_set_when_open, nameof(ReadBufferSize)));
                }

                InitializeBuffers(value, WriteBufferSize);
                _readBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish.
        /// </summary>
        public int ReadTimeout
        {
            get
            {
                return _readTimeout;
            }
            set
            {
                if (value == _readTimeout)
                {
                    return;
                }

                if (value < 0 && value != InfiniteTimeout)
                {
                    throw new ArgumentOutOfRangeException(nameof(ReadTimeout), Strings.ArgumentOutOfRange_Timeout);
                }

                if (IsOpen)
                {
                    SetReadTimeout(value);
                }

                _readTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish.
        /// </summary>
        /// <param name="value">The number of milliseconds before a time-out occurs when a read operation does not finish.</param>
        protected internal abstract void SetReadTimeout(int value);

        /// <summary>
        /// Gets or sets the number of bytes in the internal input buffer before a DataReceived event occurs.
        /// </summary>
        public int ReceivedBytesThreshold
        {
            get
            {
                return _receivedBytesThreshold;
            }
            set
            {
                if (value == _receivedBytesThreshold)
                {
                    return;
                }

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(ReceivedBytesThreshold), Strings.ArgumentOutOfRange_NeedPosNum);
                }

                if (IsOpen)
                {
                    OnReceivedBytesThresholdChanged();
                }

                _receivedBytesThreshold = value;
            }
        }

        // TODO
        private void OnReceivedBytesThresholdChanged()
        {
            // fake the call to our event handler in case the threshold has been set lower
            // than how many bytes we currently have.
            SerialDataReceivedEventArgs args = new SerialDataReceivedEventArgs(SerialData.Chars);
            /*CatchReceivedEvents(this, args);*/
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Request to Send (RTS) signal is enabled during serial communication.
        /// </summary>
        public bool RtsEnable
        {
            get
            {
                if (IsOpen)
                {
                    _rtsEnable = GetRtsEnable();
                }

                return _rtsEnable;
            }
            set
            {
                if (value == _rtsEnable)
                {
                    return;
                }

                if (Handshake == Handshake.RequestToSend || Handshake == Handshake.RequestToSendXOnXOff)
                {
                    throw new InvalidOperationException(Strings.CantSetRtsWithHandshaking);
                }

                if (IsOpen)
                {
                    SetRtsEnable(value, true);
                }
                else
                {
                    _rtsEnable = value;
                }
            }
        }

        /// <summary>
        /// Read the RtsEnable flag from the native APIs
        /// </summary>
        /// <returns></returns>
        protected internal abstract bool GetRtsEnable();

        /// <summary>
        /// Gets or sets a value indicating whether the Request to Send (RTS) signal is enabled during serial communication.
        /// </summary>
        /// <param name="value">true to enable Request to Transmit (RTS); otherwise, false. The default is false.</param>
        /// <param name="setField">true to set the underlying field</param>
        protected internal abstract void SetRtsEnable(bool value, bool setField);

        /// <summary>
        /// Gets or sets the size of the serial port output buffer.
        /// </summary>
        public int WriteBufferSize
        {
            get
            {
                return _writeBufferSize;
            }
            set
            {
                if (value == _writeBufferSize)
                {
                    return;
                }

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(WriteBufferSize));
                }

                if (IsOpen)
                {
                    throw new InvalidOperationException(string.Format(Strings.Cant_be_set_when_open, nameof(WriteBufferSize)));
                }

                InitializeBuffers(ReadBufferSize, value);
                _writeBufferSize = value;
            }
        }

        /// <summary>
        /// Initialize the platform-specific buffers for reading and writing operations
        /// </summary>
        /// <param name="readBufferSize">The size of the read buffer.</param>
        /// <param name="writeBufferSize">The size of the write buffer.</param>
        protected internal abstract void InitializeBuffers(int readBufferSize, int writeBufferSize);

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a write operation does not finish.
        /// </summary>
        public int WriteTimeout
        {
            get => _writeTimeout;
            set
            {
                if (value == _writeTimeout)
                {
                    return;
                }

                if (value <= 0 && value != InfiniteTimeout)
                {
                    throw new ArgumentOutOfRangeException(nameof(WriteTimeout), Strings.ArgumentOutOfRange_WriteTimeout);
                }

                if (IsOpen)
                {
                    SetWriteTimeout(value);
                }

                _writeTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a write operation does not finish.
        /// </summary>
        /// <param name="value">The number of milliseconds before a time-out occurs. The default is InfiniteTimeout.</param>
        protected internal abstract void SetWriteTimeout(int value);
    }
}
