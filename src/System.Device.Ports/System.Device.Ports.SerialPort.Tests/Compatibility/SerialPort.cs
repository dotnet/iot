// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.ComponentModel;
using System.IO;
using System.Text;

using SP = System.Device.Ports.SerialPort.SerialPort;
using Defaults = System.Device.Ports.SerialPort.SerialPortDefaults;

namespace System.Device.Ports.SerialPort.Tests
{
    /// <summary>
    /// A wrapper for the SerialPort class exposing the
    /// same members of System.IO.Ports.SerialPort
    /// </summary>
    internal class SerialPort : Component
    {
        private SP _serialPort;
        private SerialStream _serialStream;
        private StreamReader? _streamReader;
        private StreamWriter? _streamWriter;

        /// <summary>
        /// The InfiniteTimeout constant
        /// </summary>
        public const int InfiniteTimeout = -1;

        /// <summary>
        /// The default constructor
        /// </summary>
        public SerialPort()
            : this(String.Empty, Defaults.DefaultBaudRate, Defaults.DefaultParity, Defaults.DefaultDataBits, Defaults.DefaultStopBits)
        {
        }

        /// <summary>
        /// The constructor taking an IContainer instance
        /// </summary>
        /// <param name="container">The container instance</param>
        public SerialPort(IContainer container)
            : this()
        {
            // Required for Windows.Forms Class Composition Designer support
            container.Add(this);
        }

        /// <summary>
        /// Creates a new instance of the serial port given one or more communication parameters.
        /// </summary>
        /// <param name="portName">The name of the port</param>
        public SerialPort(string portName)
            : this(portName, Defaults.DefaultBaudRate, Defaults.DefaultParity, Defaults.DefaultDataBits, Defaults.DefaultStopBits)
        {
        }

        /// <summary>
        /// Creates a new instance of the serial port given one or more communication parameters.
        /// </summary>
        /// <param name="portName">The name of the port</param>
        /// <param name="baudRate">The baud rate</param>
        public SerialPort(string portName, int baudRate)
            : this(portName, baudRate, Defaults.DefaultParity, Defaults.DefaultDataBits, Defaults.DefaultStopBits)
        {
        }

        /// <summary>
        /// Creates a new instance of the serial port given one or more communication parameters.
        /// </summary>
        /// <param name="portName">The name of the port</param>
        /// <param name="baudRate">The baud rate</param>
        /// <param name="parity">The parity</param>
        public SerialPort(string portName, int baudRate, Parity parity)
            : this(portName, baudRate, parity, Defaults.DefaultDataBits, Defaults.DefaultStopBits)
        {
        }

        /// <summary>
        /// Creates a new instance of the serial port given one or more communication parameters.
        /// </summary>
        /// <param name="portName">The name of the port</param>
        /// <param name="baudRate">The baud rate</param>
        /// <param name="parity">The parity</param>
        /// <param name="dataBits">The data bits in the [5,9] interval</param>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits)
            : this(portName, baudRate, parity, dataBits, Defaults.DefaultStopBits)
        {
        }

        /// <summary>
        /// Creates a new instance of the serial port given one or more communication parameters.
        /// </summary>
        /// <param name="portName">The name of the port</param>
        /// <param name="baudRate">The baud rate</param>
        /// <param name="parity">The parity</param>
        /// <param name="dataBits">The data bits in the [5,9] interval</param>
        /// <param name="stopBits">The stop bits</param>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            _serialPort = SP.Create(baudRate, parity, dataBits, stopBits);
            if (!string.IsNullOrEmpty(portName))
            {
                _serialPort.PortName = portName;
            }

            _serialStream = new SerialStream(_serialPort);
        }

        /// <summary>
        /// Indicates that an error has occurred with a port represented by a SerialPort object.
        /// </summary>
        public event SerialErrorReceivedEventHandler ErrorReceived
        {
            add
            {
                _serialPort.ErrorReceived += value;
            }
            remove
            {
                _serialPort.ErrorReceived -= value;
            }
        }

        /// <summary>
        /// Indicates that a non-data signal event has occurred on the port represented by the SerialPort object.
        /// </summary>
        public event SerialPinChangedEventHandler PinChanged
        {
            add
            {
                _serialPort.PinChanged += value;
            }
            remove
            {
                _serialPort.PinChanged -= value;
            }
        }

        /// <summary>
        /// Indicates that data has been received through a port represented by the SerialPort object.
        /// </summary>
        public event SerialDataReceivedEventHandler DataReceived
        {
            add
            {
                _serialPort.DataReceived += value;
            }
            remove
            {
                _serialPort.DataReceived -= value;
            }
        }

        /// <summary>
        /// The System.IO.Stream representing the serial data
        /// </summary>
        public Stream BaseStream => _serialStream;

        /// <summary>
        /// The baud rate
        /// </summary>
        public int BaudRate
        {
            get => _serialPort.BaudRate;
            set => _serialPort.BaudRate = value;
        }

        /// <summary>
        /// Gets or sets the break signal state.
        /// </summary>
        public bool BreakState
        {
            get => _serialPort.BreakState;
            set => _serialPort.BreakState = value;
        }

        /// <summary>
        /// Gets the number of bytes of data in the send buffer.
        /// </summary>
        public int BytesToWrite
        {
            get => _serialPort.BytesToWrite;
        }

        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        public int BytesToRead
        {
            get => _serialPort.BytesToRead;
        }

        /// <summary>
        /// Gets the state of the Carrier Detect line for the port.
        /// </summary>
        public bool CDHolding
        {
            get => _serialPort.CDHolding;
        }

        /// <summary>
        /// Gets the state of the Clear-to-Send line.
        /// </summary>
        public bool CtsHolding
        {
            get => _serialPort.CtsHolding;
        }

        /// <summary>
        /// The data bits
        /// </summary>
        public int DataBits
        {
            get => _serialPort.DataBits;
            set => _serialPort.DataBits = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether null bytes are ignored when
        /// transmitted between the port and the receive buffer.
        /// </summary>
        public bool DiscardNull
        {
            get => _serialPort.DiscardNull;
            set => _serialPort.DiscardNull = value;
        }

        /// <summary>
        /// Gets the state of the Data Set Ready (DSR) signal.
        /// </summary>
        public bool DsrHolding
        {
            get => _serialPort.DsrHolding;
        }

        /// <summary>
        /// Gets or sets a value that enables the Data Terminal Ready (DTR) signal during serial communication.
        /// </summary>
        public bool DtrEnable
        {
            get => _serialPort.DtrEnable;
            set => _serialPort.DtrEnable = value;
        }

        /// <summary>
        /// The encoding used for string or char based data
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        /// <summary>
        /// Gets or sets the handshaking protocol for serial port transmission of data using a value from Handshake.
        /// </summary>
        public Handshake Handshake
        {
            get => _serialPort.Handshake;
            set => _serialPort.Handshake = value;
        }

        /// <summary>
        /// Gets a value indicating the open or closed status of the SerialPort object.
        /// </summary>
        public bool IsOpen => _serialPort.IsOpen;

        /// <summary>
        /// This property is obsolete as the StreamReader has its own concept of NewLine
        /// </summary>
        public string NewLine
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// The parity
        /// </summary>
        public Parity Parity
        {
            get => _serialPort.Parity;
            set => _serialPort.Parity = value;
        }

        /// <summary>
        /// Gets or sets the byte that replaces invalid bytes in a data stream when a parity error occurs.
        /// </summary>
        public byte ParityReplace
        {
            get => _serialPort.ParityReplace;
            set => _serialPort.ParityReplace = value;
        }

        /// <summary>
        /// Gets or sets the port name
        /// </summary>
        public string PortName
        {
            get => _serialPort.PortName;
            set => _serialPort.PortName = value;
        }

        /// <summary>
        /// Gets or sets the size of the SerialPort input buffer.
        /// </summary>
        public int ReadBufferSize
        {
            get => _serialPort.ReadBufferSize;
            set => _serialPort.ReadBufferSize = value;
        }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish.
        /// </summary>
        public int ReadTimeout
        {
            get => _serialPort.ReadTimeout;
            set => _serialPort.ReadTimeout = value;
        }

        /// <summary>
        /// Gets or sets the number of bytes in the internal input buffer before a DataReceived event occurs.
        /// </summary>
        public int ReceivedBytesThreshold
        {
            get => _serialPort.ReceivedBytesThreshold;
            set => _serialPort.ReceivedBytesThreshold = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Request to Send (RTS) signal is enabled during serial communication.
        /// </summary>
        public bool RtsEnable
        {
            get => _serialPort.RtsEnable;
            set => _serialPort.RtsEnable = value;
        }

        /// <summary>
        /// The stop bits
        /// </summary>
        public StopBits StopBits
        {
            get => _serialPort.StopBits;
            set => _serialPort.StopBits = value;
        }

        /// <summary>
        /// Gets or sets the size of the serial port output buffer.
        /// </summary>
        public int WriteBufferSize
        {
            get => _serialPort.WriteBufferSize;
            set => _serialPort.WriteBufferSize = value;
        }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a write operation does not finish.
        /// </summary>
        public int WriteTimeout
        {
            get => _serialPort.WriteTimeout;
            set => _serialPort.WriteTimeout = value;
        }

        /// <summary>
        /// Close the serial port and related stream
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// The Dispose method implementing the Dispose pattern
        /// </summary>
        /// <param name="disposing">true when called during the Dispose</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serialPort.Dispose();
                _serialStream.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Discards data from the serial driver's receive buffer.
        /// </summary>
        public void DiscardInBuffer() => _serialPort.DiscardInBuffer();

        /// <summary>
        /// Discards data from the serial driver's transmit buffer.
        /// </summary>
        public void DiscardOutBuffer() => _serialPort.DiscardOutBuffer();

        /// <summary>
        /// Opens a new serial port connection.
        /// </summary>
        public void Open() => _serialPort.Open();

        /// <summary>
        /// Reads a number of bytes from the SerialPort input buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The offset in buffer at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if count is greater than the number of bytes in the input buffer.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(byte[] buffer, int offset, int count)
            => Read(buffer, offset, count);

        /// <summary>
        /// Reads a single character from the serial port
        /// </summary>
        /// <returns></returns>
        public int ReadChar() => Reader.Read();

        /// <summary>
        /// Reads a number of characters from the SerialPort input buffer and writes those bytes into a character array at the specified offset.
        /// </summary>
        /// <param name="buffer">The char array to write the input to.</param>
        /// <param name="offset">The offset in buffer at which to write the characters.</param>
        /// <param name="count">The maximum number of characters to read. Fewer characters are read if count is greater than the number of characters in the input buffer.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(char[] buffer, int offset, int count)
            => Reader.Read(buffer, offset, count);

        /// <summary>
        /// Synchronously reads one byte from the SerialPort input buffer.
        /// </summary>
        /// <returns></returns>
        public int ReadByte() => _serialStream.ReadByte();

        /// <summary>
        /// Read the current available data as a string
        /// </summary>
        /// <returns></returns>
        public string ReadExisting()
        {
            var available = _serialPort.BytesToRead;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(available);
            try
            {
                int numRead = Read(buffer, 0, buffer.Length);
                var text = Encoding.GetString(buffer, 0, numRead);
                return text;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Read a line from the serial port
        /// </summary>
        /// <returns></returns>
        public string ReadLine() => Reader.ReadLine() ?? String.Empty;

        /// <summary>
        /// This API was not migrated and throws
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public string ReadTo(string value) => throw new NotImplementedException();

        /// <summary>
        /// Writes a string to output using the current Encoding
        /// </summary>
        /// <param name="text">The string to write</param>
        public void Write(string text)
            => Writer.Write(text);

        /// <summary>
        /// Write an array of characters over the serial port using the current Encoding
        /// </summary>
        /// <param name="buffer">The buffer of characters</param>
        /// <param name="offset">The offset to the starting character to write</param>
        /// <param name="count">The number of characters to write</param>
        public void Write(char[] buffer, int offset, int count)
            => Writer.Write(buffer, offset, count);

        /// <summary>
        /// Writes a specified section of a byte buffer to output.
        /// </summary>
        /// <param name="buffer">The buffer to write</param>
        /// <param name="offset">The offset to the starting byte to write</param>
        /// <param name="count">The number of bytes to write</param>
        public void Write(byte[] buffer, int offset, int count)
            => Write(buffer, offset, count);

        /// <summary>
        /// Write a string over the serial port
        /// </summary>
        /// <param name="text">The string to write</param>
        public void WriteLine(string text) => Writer.WriteLine(text);

        private StreamReader Reader => _streamReader ??= new StreamReader(_serialStream, Encoding);
        private StreamWriter Writer => _streamWriter ??= new StreamWriter(_serialStream, Encoding);
    }

}
