// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// The class that allows to exchange data across a standard Serial Port
    /// </summary>
    public abstract partial class SerialPort
    {
        /// <summary>
        /// Indicates that data has been received through a port represented by the SerialPort object.
        /// </summary>
        public event SerialDataReceivedEventHandler? DataReceived;

        /// <summary>
        /// Indicates that an error has occurred with a port represented by a SerialPort object.
        /// </summary>
        public event SerialErrorReceivedEventHandler? ErrorReceived;

        /// <summary>
        /// Indicates that a non-data signal event has occurred on the port represented by the SerialPort object.
        /// </summary>
        public event SerialPinChangedEventHandler? PinChanged;

        /// <summary>
        /// Creates a new instance of the serial port given one or more communication parameters.
        /// </summary>
        /// <param name="baudRate">The baud rate</param>
        /// <param name="parity">The parity</param>
        /// <param name="dataBits">The data bits in the [5,9] interval</param>
        /// <param name="stopBits">The stop bits</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static SerialPort Create(int baudRate = DefaultBaudRate,
            Parity parity = DefaultParity,
            int dataBits = DefaultDataBits,
            StopBits stopBits = DefaultStopBits)
        {
            SerialPort instance;
            if (OperatingSystem.IsWindows())
            {
                instance = new WindowsSerialPort();
            }
            else if (OperatingSystem.IsLinux())
            {
                instance = new LinuxSerialPort();
            }
            else
            {
                throw new Exception(string.Format(Strings.NotSupported_OperatingSystem, Environment.OSVersion));
            }

            instance.BaudRate = baudRate;
            instance.Parity = parity;
            instance.DataBits = dataBits;
            instance.StopBits = stopBits;
            return instance;
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        internal SerialPort()
        {
        }

        /// <summary>
        /// Finalize the serial port object
        /// </summary>
        ~SerialPort()
        {
            Dispose(false);
        }

        internal void NoCall()
        {
            _isOpen = false;

            if (_isOpen)
            {
                DataReceived?.Invoke(this, new SerialDataReceivedEventArgs(new SerialData()));
                ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(SerialError.Frame));
                PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPinChange.Ring));
            }
        }

        /// <summary>
        /// Opens a new serial port connection.
        /// </summary>
        public void Open()
        {
            if (IsOpen)
            {
                throw new InvalidOperationException(Strings.Port_already_open);
            }

            InitializeBuffers(_readBufferSize, _writeBufferSize);
            OpenPort();
        }

        /// <summary>
        /// Initialize the platform-specific buffers for reading and writing operations
        /// </summary>
        /// <param name="readBufferSize">The size of the read buffer.</param>
        /// <param name="writeBufferSize">The size of the write buffer.</param>
        protected internal abstract void InitializeBuffers(int readBufferSize, int writeBufferSize);

        /// <summary>
        /// Open the port using platform-specific code
        /// </summary>
        protected internal abstract void OpenPort();

        /// <summary>
        /// Closes the port connection, sets the IsOpen property to false, and disposes of the internal Stream object.
        /// This method does not call Dispose because we may want to re-open the port without creating a new instance
        /// of the Serial Port class.
        /// </summary>
        public void Close()
        {
            if (IsOpen)
            {
                ClosePort(false);
            }
        }

        /// <summary>
        /// Close the port using platform-specific code
        /// </summary>
        protected internal abstract void ClosePort(bool disposing);

        /// <summary>
        /// Releases the unmanaged resources used by the SerialPort and optionally releases the managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /*
        /// <summary>
        /// Releases the unmanaged resources used by the SerialPort and optionally releases the managed resources.
        /// </summary>
        /// <returns></returns>
        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
        */

        /// <summary>
        /// Releases the unmanaged resources used by the SerialPort and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True when called from the Dispose method. False when called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            ClosePort(disposing);
            if (disposing)
            {
            }
        }

        /// <summary>
        /// Discards data from the serial driver's receive buffer.
        /// </summary>
        public abstract void DiscardInBuffer();

        /// <summary>
        /// Discards data from the serial driver's transmit buffer.
        /// </summary>
        public abstract void DiscardOutBuffer();

        internal void TriggerErrors(int errors)
        {
            if ((errors & (int)SerialError.TXFull) != 0)
            {
                ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(SerialError.TXFull));
            }

            if ((errors & (int)SerialError.RXOver) != 0)
            {
                ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(SerialError.RXOver));
            }

            if ((errors & (int)SerialError.Overrun) != 0)
            {
                ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(SerialError.Overrun));
            }

            if ((errors & (int)SerialError.RXParity) != 0)
            {
                ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(SerialError.RXParity));
            }

            if ((errors & (int)SerialError.Frame) != 0)
            {
                ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(SerialError.Frame));
            }
        }

        internal void TriggerReceiveEvents(int nativeEvents)
        {
            if ((nativeEvents & (int)SerialData.Chars) != 0)
            {
                DataReceived?.Invoke(this, new SerialDataReceivedEventArgs(SerialData.Chars));
            }

            if ((nativeEvents & (int)SerialData.Eof) != 0)
            {
                DataReceived?.Invoke(this, new SerialDataReceivedEventArgs(SerialData.Eof));
            }
        }

        internal void TriggerPinEvents(int nativeEvents)
        {
            if ((nativeEvents & (int)SerialPinChange.CtsChanged) != 0)
            {
                PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPinChange.CtsChanged));
            }

            if ((nativeEvents & (int)SerialPinChange.DsrChanged) != 0)
            {
                PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPinChange.DsrChanged));
            }

            if ((nativeEvents & (int)SerialPinChange.CDChanged) != 0)
            {
                PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPinChange.CDChanged));
            }

            if ((nativeEvents & (int)SerialPinChange.Ring) != 0)
            {
                PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPinChange.Ring));
            }

            if ((nativeEvents & (int)SerialPinChange.Break) != 0)
            {
                PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPinChange.Break));
            }
        }

        /// <summary>
        /// Reads a number of bytes from the SerialPort input buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The offset in buffer at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if count is greater than the number of bytes in the input buffer.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a number of characters from the SerialPort input buffer and writes them into an array of characters at a given offset.
        /// </summary>
        /// <param name="buffer">The character array to write the input to.</param>
        /// <param name="offset">The offset in buffer at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if count is greater than the number of bytes in the input buffer.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(char[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Synchronously reads one byte from the SerialPort input buffer.
        /// </summary>
        /// <returns>The byte, cast to an Int32, or -1 if the end of the stream has been read.</returns>
        public int ReadByte()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Synchronously reads one character from the SerialPort input buffer.
        /// </summary>
        /// <returns>The character that was read.</returns>
        public int ReadChar()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads all immediately available bytes, based on the encoding,
        /// in both the stream and the input buffer of the SerialPort object.
        /// </summary>
        /// <returns>The contents of the stream and the input buffer of the SerialPort object.</returns>
        public string ReadExisting()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads up to the NewLine value in the input buffer.
        /// </summary>
        /// <returns>The contents of the input buffer up to the first occurrence of a NewLine value.</returns>
        public string ReadLine()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a string up to the specified value in the input buffer.
        /// </summary>
        /// <param name="value">A value that indicates where the read operation stops.</param>
        /// <returns>The contents of the input buffer up to the specified value.</returns>
        public string ReadTo(string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the specified string to the serial port.
        /// </summary>
        /// <param name="text">The string for output.</param>
        public void Write(string text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the specified string and the NewLine value to the output buffer.
        /// </summary>
        /// <param name="text">The string to write to the output buffer.</param>
        public void WriteLine(string text)
        {
            throw new NotImplementedException();
        }

    }
}
