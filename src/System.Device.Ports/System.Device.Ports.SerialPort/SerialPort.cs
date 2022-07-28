// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
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
        public static SerialPort Create(int baudRate = SerialPortDefaults.DefaultBaudRate,
            Parity parity = SerialPortDefaults.DefaultParity,
            int dataBits = SerialPortDefaults.DefaultDataBits,
            StopBits stopBits = SerialPortDefaults.DefaultStopBits)
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
            IsOpen = false;

            if (IsOpen)
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

            OpenPort();
            InitializeBuffers(_readBufferSize, _writeBufferSize);
            IsOpen = true;
        }

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
                try
                {
                    ClosePort(false);
                }
                finally
                {
                    IsOpen = false;
                }
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

        internal void TriggerErrors(uint errors)
        {
            if ((errors & (uint)SerialError.TXFull) != 0)
            {
                ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(SerialError.TXFull));
            }

            if ((errors & (uint)SerialError.RXOver) != 0)
            {
                ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(SerialError.RXOver));
            }

            if ((errors & (uint)SerialError.Overrun) != 0)
            {
                ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(SerialError.Overrun));
            }

            if ((errors & (uint)SerialError.RXParity) != 0)
            {
                ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(SerialError.RXParity));
            }

            if ((errors & (uint)SerialError.Frame) != 0)
            {
                ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(SerialError.Frame));
            }
        }

        internal void TriggerReceiveEvents(uint nativeEvents)
        {
            if ((nativeEvents & (uint)SerialData.Chars) != 0)
            {
                DataReceived?.Invoke(this, new SerialDataReceivedEventArgs(SerialData.Chars));
            }

            if ((nativeEvents & (uint)SerialData.Eof) != 0)
            {
                DataReceived?.Invoke(this, new SerialDataReceivedEventArgs(SerialData.Eof));
            }
        }

        internal void TriggerPinEvents(uint nativeEvents)
        {
            if ((nativeEvents & (uint)SerialPinChange.CtsChanged) != 0)
            {
                PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPinChange.CtsChanged));
            }

            if ((nativeEvents & (uint)SerialPinChange.DsrChanged) != 0)
            {
                PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPinChange.DsrChanged));
            }

            if ((nativeEvents & (uint)SerialPinChange.CDChanged) != 0)
            {
                PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPinChange.CDChanged));
            }

            if ((nativeEvents & (uint)SerialPinChange.Ring) != 0)
            {
                PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPinChange.Ring));
            }

            if ((nativeEvents & (uint)SerialPinChange.Break) != 0)
            {
                PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPinChange.Break));
            }
        }

        /// <summary>
        /// Flush the content of the serial port write buffer
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="array">todo</param>
        /// <param name="offset">todo</param>
        /// <param name="count">todo</param>
        public abstract void Write(byte[] array, int offset, int count);

        /// <summary>
        /// Reads a number of bytes from the SerialPort input buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The offset in buffer at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if count is greater than the number of bytes in the input buffer.</param>
        /// <returns>The number of bytes read.</returns>
        public abstract int Read(byte[] buffer, int offset, int count);
    }
}
