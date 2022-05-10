// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

    }
}
