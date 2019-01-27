// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Pca95x4
{
    /// <summary>
    /// A general purpose parallel I/O expansion for I2C applications.
    /// </summary>
    public class Pca95x4 : IDisposable
    {
        private readonly I2cDevice _i2cDevice;
        private GpioController _masterGpioController;
        private readonly int? _interrupt;

        /// <summary>
        /// Initializes new instance of Pca95x4.
        /// A general purpose parallel I/O expansion for I2C applications.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="interrupt">The input pin number that is connected to the interrupt (INT).</param>
        public Pca95x4(I2cDevice i2cDevice, int? interrupt = null)
        {
            _i2cDevice = i2cDevice;
            _interrupt = interrupt;

            InitializeMasterGpioController();
        }

        private static void ValidateBitNumber(int bitNumber)
        {
            if (bitNumber < 0 || bitNumber > 7)
            {
                throw new IndexOutOfRangeException($"Invalid bit number {bitNumber}.");
            }
        }

        private static void ClearBit(ref byte data, int bitNumber)
        {
            ValidateBitNumber(bitNumber);
            data &= (byte)~(1 << bitNumber);
        }

        private void SetBit(ref byte data, int bitNumber)
        {
            ValidateBitNumber(bitNumber);
            data |= (byte)(1 << bitNumber);
        }

        private static bool GetBit(byte data, int bitNumber)
        {
            ValidateBitNumber(bitNumber);
            return ((data >> bitNumber) & 1) == 1;
        }

        private void InitializeMasterGpioController()
        {
            // Only need master controller if there is external pin provided.
            if (_interrupt != null)
            {
                _masterGpioController = new GpioController();
                _masterGpioController.OpenPin((int)_interrupt, PinMode.Input);
            }
        }

        /// <summary>
        /// Reads a byte from a register.
        /// </summary>
        /// <param name="register">The register to read.</param>
        /// <returns>The data read from the register.</returns>
        public byte Read(Register register)
        {
            byte[] data = Read(register, 1);
            return data[0];
        }

        /// <summary>
        /// Reads a number of bytes from a register. 
        /// All data returned is from the selected register as Pca95x4 does not auto-increment addressing.
        /// </summary>
        /// <param name="register">The register to read.</param>
        /// <param name="byteCount"></param>
        /// <returns>The data read from the register.</returns>
        public byte[] Read(Register register, byte byteCount)
        {
            Write(register, new byte[] { }); // Set address to register first.

            byte[] readBuffer = new byte[byteCount];
            _i2cDevice.Read(readBuffer);
            return readBuffer;
        }

        /// <summary>
        /// Reads a bit from a register.
        /// </summary>
        /// <param name="register">The register to read.</param>
        /// <param name="bitNumber">The register bit number to read.</param>
        /// <returns>The value of the register bit read.</returns>
        public bool ReadBit(Register register, int bitNumber)
        {
            byte data = Read(register);
            return GetBit(data, bitNumber);
        }

        /// <summary>
        /// Reads the pin value of the interrupt (INT).
        /// </summary>
        /// <returns>The pin value of the interrupt (INT).</returns>
        public PinValue ReadInterrupt()
        {
            if (_masterGpioController == null)
            {
                throw new Exception("Master controller has not been initialized.");
            }

            if (_interrupt == null)
            {
                throw new Exception("INT pin has not been initialized.");
            }

            return _masterGpioController.Read((int)_interrupt);
        }

        /// <summary>
        /// Writes a byte to a register.
        /// </summary>
        /// <param name="register">The register to write.</param>
        /// <param name="data">The data to write to the register.</param>
        public void Write(Register register, byte data)
        {
            Write(register, new byte[] { data });
        }

        /// <summary>
        /// Writes a number of bytes to a register.
        /// All data will be written to selected register as Pca95x4 does not auto-increment addressing.
        /// </summary>
        /// <param name="register">The register to write.</param>
        /// <param name="data">The data to write to the register.</param>
        public void Write(Register register, byte[] data)
        {
            byte[] writeBuffer = new byte[data.Length + 1]; // Include Register Address.
            writeBuffer[0] = (byte)register;
            data.CopyTo(writeBuffer, 1);
            _i2cDevice.Write(writeBuffer);
        }

        /// <summary>
        /// Writes to a register bit.
        /// </summary>
        /// <param name="register">The register to write.</param>
        /// <param name="bitNumber">The register bit number to write.</param>
        /// <param name="bit">The value to write to register bit.</param>
        public void WriteBit(Register register, int bitNumber, bool bit)
        {
            byte data = Read(register);

            if (bit)
            {
                SetBit(ref data, bitNumber);
            }
            else
            {
                ClearBit(ref data, bitNumber);
            }

            Write(register, data);
        }

        /// <summary>
        /// Inverts Input Register polarity.
        /// </summary>
        /// <param name="invert">Determines if Inputer Register polarity is inverted.</param>
        public void InvertInputRegisterPolarity(bool invert)
        {
            byte data = invert ? (byte)0xFF : (byte)0x00;
            Write(Register.PolarityInversion, data);
        }

        /// <summary>
        /// Inverts Input Register bit polarity.
        /// </summary>
        /// <param name="bitNumber">The Input Register bit number to invert.</param>
        /// <param name="invert">Determines if the Input Register bit polarity is inverted.</param>
        public void InvertInputRegisterBitPolarity(int bitNumber, bool invert)
        {
            WriteBit(Register.PolarityInversion, bitNumber, invert);
        }

        public void Dispose()
        {
            _i2cDevice?.Dispose();
        }
    }
}
