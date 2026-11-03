// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;

namespace Iot.Device.Ft200X.Samples
{
    /// <summary>
    /// Processes FIFO request frames for a simulated temperature sensor with ID, temperature, and configuration registers.
    /// </summary>
    internal sealed class TemperatureSensor : IFifoSensor
    {
        /// <summary>
        /// The size in bytes of each request and response frame.
        /// </summary>
        public const int FrameLength = 4;

        private const byte ReadCommand = 0;
        private const byte WriteCommand = 1;
        private const byte IdRegister = 0;
        private const byte TemperatureRegister = 1;
        private const byte ConfigurationRegister = 2;
        private const byte InvalidCommand = 1;
        private const byte UnknownRegister = 2;
        private const byte ReadOnlyRegister = 3;
        private const byte InvalidValue = 4;
        private const byte MeasurementDisabled = 5;
        private const ushort SensorId = 0x2001;
        private const short TemperatureHundredthsCelsius = 2350;

        private readonly byte[] _request = new byte[FrameLength];
        private readonly byte[] _response = new byte[FrameLength];
        private int _received;
        private bool _enabled = true;

        /// <inheritdoc/>
        public ReadOnlyMemory<byte> ProcessByte(byte value)
        {
            return TryProcessByte(value, _response) ? _response : ReadOnlyMemory<byte>.Empty;
        }

        /// <summary>
        /// Accumulates a request byte and produces a response when a complete frame is available.
        /// </summary>
        /// <param name="value">The next request byte from the USB receive FIFO.</param>
        /// <param name="response">The four-byte destination for status, register, and a big-endian 16-bit value.</param>
        /// <returns>
        /// <see langword="true"/> when a response is ready, including protocol error responses;
        /// otherwise <see langword="false"/>, leaving the destination unchanged.
        /// </returns>
        /// <exception cref="ArgumentException">The response buffer is not exactly four bytes long.</exception>
        public bool TryProcessByte(byte value, Span<byte> response)
        {
            if (response.Length != FrameLength)
            {
                throw new ArgumentException("The sensor response must be four bytes long.", nameof(response));
            }

            // USB reads may split a request or combine several I2C writes.
            _request[_received++] = value;
            if (_received < FrameLength)
            {
                return false;
            }

            _received = 0;
            response.Clear();
            byte command = _request[0];
            byte register = _request[1];
            ushort requestedValue = BinaryPrimitives.ReadUInt16BigEndian(_request.AsSpan(2));
            response[1] = register;
            if (command != ReadCommand && command != WriteCommand)
            {
                response[0] = InvalidCommand;
                return true;
            }

            if (register > ConfigurationRegister)
            {
                response[0] = UnknownRegister;
                return true;
            }

            if (command == WriteCommand)
            {
                if (register != ConfigurationRegister)
                {
                    response[0] = ReadOnlyRegister;
                    return true;
                }

                if (requestedValue > 1)
                {
                    response[0] = InvalidValue;
                    return true;
                }

                _enabled = requestedValue == 1;
            }
            else if (requestedValue != 0)
            {
                response[0] = InvalidValue;
                return true;
            }

            switch (register)
            {
                case IdRegister:
                    BinaryPrimitives.WriteUInt16BigEndian(response.Slice(2), SensorId);
                    break;
                case TemperatureRegister:
                    if (_enabled)
                    {
                        BinaryPrimitives.WriteInt16BigEndian(response.Slice(2), TemperatureHundredthsCelsius);
                    }
                    else
                    {
                        response[0] = MeasurementDisabled;
                    }

                    break;
                case ConfigurationRegister:
                    response[3] = _enabled ? (byte)1 : (byte)0;
                    break;
            }

            return true;
        }
    }
}
