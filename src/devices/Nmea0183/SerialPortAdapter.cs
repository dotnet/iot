// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.IO.Ports;

namespace Iot.Device.Seatalk1
{
    /// <summary>
    /// Default <see cref="ISerialPort"/> implementation backed by <see cref="SerialPort"/>.
    /// </summary>
    public sealed class SerialPortAdapter : ISerialPort
    {
        private readonly SerialPort _serialPort;

        /// <summary>
        /// Creates a new adapter instance.
        /// </summary>
        /// <param name="portName">Serial port name, e.g. COM5 or /dev/ttyAMA2.</param>
        public SerialPortAdapter(string portName)
        {
            _serialPort = new SerialPort(portName);
        }

        /// <inheritdoc />
        public string PortName => _serialPort.PortName;

        /// <inheritdoc />
        public int BaudRate
        {
            get => _serialPort.BaudRate;
            set => _serialPort.BaudRate = value;
        }

        /// <inheritdoc />
        public Parity Parity
        {
            get => _serialPort.Parity;
            set => _serialPort.Parity = value;
        }

        /// <inheritdoc />
        public StopBits StopBits
        {
            get => _serialPort.StopBits;
            set => _serialPort.StopBits = value;
        }

        /// <inheritdoc />
        public int DataBits
        {
            get => _serialPort.DataBits;
            set => _serialPort.DataBits = value;
        }

        /// <inheritdoc />
        public Stream BaseStream => _serialPort.BaseStream;

        /// <inheritdoc />
        public void Open() => _serialPort.Open();

        /// <inheritdoc />
        public void Close() => _serialPort.Close();

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);

        /// <inheritdoc />
        public void Dispose() => _serialPort.Dispose();

        /// <inheritdoc/>
        public int ReadByte()
        {
            return _serialPort.ReadByte();
        }
    }
}
