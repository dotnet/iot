// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using Iot.Device.Seatalk1;

namespace Iot.Device.Nmea0183.Tests
{
    /// <summary>
    /// In-memory serial port fake for tests using <see cref="Seatalk1Parser"/>.
    /// </summary>
    internal sealed class FakeSerialPort : ISerialPort
    {
        private readonly MemoryStream _inputStream;
        private readonly List<byte> _writtenBytes;

        /// <summary>
        /// Initializes a new fake serial port.
        /// </summary>
        /// <param name="input">Input bytes returned by <see cref="BaseStream"/> reads.</param>
        /// <param name="portName">Optional port name.</param>
        public FakeSerialPort(byte[]? input = null, string portName = "FAKE")
        {
            _inputStream = input is null ? new MemoryStream() : new MemoryStream(input);
            _writtenBytes = new List<byte>();
            PortName = portName;
            Parity = Parity.Even;
            StopBits = StopBits.One;
            DataBits = 8;
            BaudRate = 4800;
        }

        /// <summary>
        /// Gets all bytes written through <see cref="Write(byte[], int, int)"/>.
        /// </summary>
        public IReadOnlyList<byte> WrittenBytes => _writtenBytes;

        /// <summary>
        /// True when <see cref="Open"/> was called and not yet closed.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <inheritdoc />
        public string PortName { get; }

        /// <inheritdoc />
        public int BaudRate { get; set; }

        /// <inheritdoc />
        public Parity Parity { get; set; }

        /// <inheritdoc />
        public StopBits StopBits { get; set; }

        /// <inheritdoc />
        public int DataBits { get; set; }

        /// <inheritdoc />
        public Stream BaseStream => _inputStream;

        /// <inheritdoc />
        public void Open()
        {
            IsOpen = true;
        }

        /// <inheritdoc />
        public void Close()
        {
            IsOpen = false;
        }

        /// <inheritdoc />
        void ISerialPort.Write(byte[] buffer, int offset, int count)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            for (int i = 0; i < count; i++)
            {
                _writtenBytes.Add(buffer[offset + i]);
            }
        }

        public void SimulateReceive(byte[] buffer)
        {
            _inputStream.Write(buffer, 0, buffer.Length);
        }

        public int ReadByte()
        {
            return _inputStream.ReadByte();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _inputStream.Dispose();
        }
    }
}
