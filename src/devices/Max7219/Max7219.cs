// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device
{
    public class Max7219 : IDisposable
    {
        private SpiDevice _spiDevice;

        /// <summary>
        /// Number of digits Register per Module
        /// </summary>
        const int NumDigits = 8;

        /// <summary>
        /// Internal buffer used to write to the devices.
        /// </summary>
        private readonly byte[] _writeBuffer;

        /// <summary>
        /// A Buffer that contains the values of the digits registers per device
        /// </summary>
        private readonly byte[,] _buffer;

        /// <summary>
        /// Number of cascaded devices
        /// </summary>
        /// <value></value>
        public int CascadedDevices { get; private set; }

        /// <summary>
        /// The Rotation to be applied (when modules are assembled rotated way)
        /// </summary>
        public RotationType Rotation { get; set; }

        /// <summary>
        /// Creates a Max7219 Device given a <see paramref="spiDevice"> to communicate over and the
        /// number of devices that are cascaded.
        /// </summary>
        public Max7219(SpiDevice spiDevice, int cascadedDevices = 1, RotationType rotation = RotationType.None)
        {
            _spiDevice = spiDevice;
            CascadedDevices = cascadedDevices;
            Rotation = rotation;
            _buffer = new byte[CascadedDevices, NumDigits];
            _writeBuffer = new byte[2 * CascadedDevices];
        }


        /// <summary>
        /// Standard initialization routine.
        /// </summary>
        public void Init()
        {
            SetRegister(Register.SCANLIMIT, 7); //show all 8 digits
            SetRegister(Register.DECODEMODE, 0); // use matrix (not digits)
            SetRegister(Register.DISPLAYTEST, 0); // no display test
            SetRegister(Register.SHUTDOWN, 1); // not shutdown mode
            Brightness(5); //intensity, range: 0..15
            ClearAll();
        }


        /// <summary>
        /// Sends data to a specific register replicated for all cascaded devices
        /// </summary>
        public void SetRegister(Register register, byte data)
        {
            var i = 0;
            for (byte deviceId = 0; deviceId < CascadedDevices; deviceId++)
            {
                _writeBuffer[i++] = (byte)register;
                _writeBuffer[i++] = data;
            }
            Write(_writeBuffer);
        }

        /// <summary>
        /// Write data do the spi device.
        /// </summary>
        /// <remarks>
        /// The size of the data should be 2 * cascaded devices.
        /// </remarks>
        private void Write(Span<byte> data)
        {
            _spiDevice.Write(data);
        }

        /// <summary>
        /// Sets the brightness of all cascaded devices to the same intensity level. 
        /// </summary>
        /// <param name="intensity">intensity level ranging from 0..15. </param>
        public void Brightness(int intensity)
        {
            if (intensity < 0 || intensity > 15)
                throw new ArgumentOutOfRangeException(nameof(intensity), $"Invalid intensity for Brightness {intensity}");
            SetRegister(Register.INTENSITY, (byte)intensity);
        }

        /// <summary>
        /// Gets or Sets the value to the digit value for a given device
        /// and digit position
        /// </summary>
        public byte this[int deviceId, int digit]
        {
            get
            {
                ValidatePosition(deviceId, digit);
                return _buffer[deviceId, digit];
            }
            set
            {
                ValidatePosition(deviceId, digit);
                _buffer[deviceId, digit] = value;
            }
        }

        private void ValidatePosition(int deviceId, int digit)
        {
            if (deviceId < 0 || deviceId >= CascadedDevices)
                throw new ArgumentOutOfRangeException("deviceId", $"Invalid device Id: {deviceId}");
            if (digit < 0 || digit >= NumDigits)
                throw new ArgumentOutOfRangeException("digit", $"Invalid digit: {digit}");
        }

        /// <summary>
        /// Writes all the Values to the devices.
        /// </summary>
        public void Flush()
        {
            WriteBuffer(_buffer);
        }

        /// <summary>
        /// Writes a two dimensional buffer containing all the values to the devices.
        /// </summary>
        public void WriteBuffer(byte[,] buffer)
        {
            switch (Rotation)
            {
                case RotationType.None: WriteBufferWithoutRotation(buffer); break;
                case RotationType.Half: WriteBufferRotateHalf(buffer); break;
                case RotationType.Right: WriteBufferRotateRight(buffer); break;
                case RotationType.Left: WriteBufferRotateLeft(buffer); break;
            }
        }

        /// <summary>
        /// Writes a two dimensional buffer containing all the values to the devices without roation
        /// </summary>
        private void WriteBufferWithoutRotation(byte[,] buffer)
        {
            ValidateBuffer(buffer);
            for (int digit = 0; digit < NumDigits; digit++)
            {
                var i = 0;
                for (byte deviceId = 0; deviceId < CascadedDevices; deviceId++)
                {
                    _writeBuffer[i++] = (byte)((int)Register.DIGIT0 + digit);
                    _writeBuffer[i++] = buffer[deviceId, digit];
                }
                Write(_writeBuffer);
            }
        }

        /// <summary>
        /// Writes a two dimensional buffer containing all the values to the devices 
        /// rotating values by 180 degree.
        /// </summary>
        private void WriteBufferRotateHalf(byte[,] buffer)
        {
            ValidateBuffer(buffer);
            for (int digit = 0; digit < NumDigits; digit++)
            {
                var i = 0;
                for (byte deviceId = 0; deviceId < CascadedDevices; deviceId++)
                {
                    _writeBuffer[i++] = (byte)((int)Register.DIGIT0 + digit);
                    byte b = buffer[deviceId, 7 - digit];
                    //reverse bits in byte
                    b = (byte)((b * 0x0202020202 & 0x010884422010) % 1023);
                    _writeBuffer[i++] = b;

                }
                Write(_writeBuffer);
            }
        }

        /// <summary>
        /// Writes a two dimensional buffer containing all the values to the devices 
        /// rotating values to the right.
        /// </summary>
        private void WriteBufferRotateRight(byte[,] buffer)
        {
            ValidateBuffer(buffer);
            for (int digit = 0; digit < NumDigits; digit++)
            {
                var mask = 0x01 << digit;
                var i = 0;
                for (byte deviceId = 0; deviceId < CascadedDevices; deviceId++)
                {
                    _writeBuffer[i++] = (byte)((int)Register.DIGIT0 + (7 - digit));
                    byte value = 0;
                    byte targetBit = 0x80;
                    for (int bitDigit = 0; bitDigit < NumDigits; bitDigit++, targetBit >>= 1)
                    {
                        if ((buffer[deviceId, bitDigit] & mask) != 0)
                            value |= targetBit;
                    }
                    _writeBuffer[i++] = value;
                }
                Write(_writeBuffer);
            }
        }

        /// <summary>
        /// Writes a two dimensional buffer containing all the values to the devices 
        /// rotating values to the left.
        /// </summary>
        private void WriteBufferRotateLeft(byte[,] buffer)
        {
            ValidateBuffer(buffer);
            for (int digit = 0; digit < NumDigits; digit++)
            {
                var mask = 0x80 >> digit;
                var i = 0;
                for (byte deviceId = 0; deviceId < CascadedDevices; deviceId++)
                {
                    _writeBuffer[i++] = (byte)((int)Register.DIGIT0 + (7 - digit));
                    byte value = 0;
                    byte targetBit = 0x01;
                    for (int bitDigit = 0; bitDigit < NumDigits; bitDigit++, targetBit <<= 1)
                    {
                        if ((buffer[deviceId, bitDigit] & mask) != 0)
                            value |= targetBit;
                    }
                    _writeBuffer[i++] = value;
                }
                Write(_writeBuffer);
            }
        }

        /// <summary>
        /// Validates the buffer dimensions.
        /// </summary>
        /// <param name="buffer"></param>
        private void ValidateBuffer(byte[,] buffer)
        {
            if (buffer.Rank != 2)
                throw new ArgumentException(nameof(buffer), "buffer must be two dimensional.");
            if (buffer.GetUpperBound(0) != CascadedDevices - 1)
                throw new ArgumentException(nameof(buffer), $"buffer upper bound ({buffer.GetUpperBound(0)}) for dimension 0 must be {CascadedDevices - 1}.");
            if (buffer.GetUpperBound(1) != NumDigits - 1)
                throw new ArgumentException(nameof(buffer), $"buffer upper bound ({buffer.GetUpperBound(1)}) for dimension 1 must be {NumDigits - 1}.");
        }

        /// <summary>
        /// Clears the buffer from the given start to end (exclusive) and flushes
        /// </summary>
        public void Clear(int start, int end)
        {
            if (end < 0 || end > CascadedDevices)
                throw new ArgumentOutOfRangeException(nameof(end));
            if (start < 0 || start >= end)
                throw new ArgumentOutOfRangeException(nameof(end));
            for (int deviceId = start; deviceId < end; deviceId++)
            {
                for (int digit = 0; digit < NumDigits; digit++)
                {
                    this[deviceId, digit] = 0;
                }
            }
            Flush();
        }

        /// <summary>
        /// Clears the buffer from the given start to end and flushes
        /// </summary>
        public void ClearAll()
        {
            Clear(0, CascadedDevices);
        }

        public void Dispose()
        {
            if (_spiDevice != null)
            {
                _spiDevice.Dispose();
                _spiDevice = null;
            }
        }

        public enum Register : byte
        {
            NOOP = 0x0,
            DIGIT0 = 0x1,
            DIGIT1 = 0x2,
            DIGIT2 = 0x3,
            DIGIT3 = 0x4,
            DIGIT4 = 0x5,
            DIGIT5 = 0x6,
            DIGIT6 = 0x7,
            DIGIT7 = 0x8,
            DECODEMODE = 0x9,
            INTENSITY = 0xA,
            SCANLIMIT = 0xB,
            SHUTDOWN = 0xC,
            DISPLAYTEST = 0xF
        }

        public enum RotationType
        {
            None,
            Right, // Rotate 90 degrees to the right
            Half, // Roate 180 degrees
            Left, // Rotate 90 degrees to the left
        }
    }
}
