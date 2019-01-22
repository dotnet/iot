// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.Mcp23xxx
{
    public abstract class Mcp23xxx : IDisposable
    {
        protected int DeviceAddress { get; }
        private GpioController _masterGpioController;
        private readonly int? _reset;
        private readonly int? _interruptA;
        private readonly int? _interruptB;

        /// <summary>
        /// A general purpose parallel I/O expansion for I2C or SPI applications.
        /// </summary>
        /// <param name="deviceAddress">The device address for the connection on the I2C or SPI bus.</param>
        /// <param name="reset">Output pin number that is connected to the hardware reset.</param>
        /// <param name="interruptA">Input pin number that is connected to the interrupt for Port A (INTA).</param>
        /// <param name="interruptB">Input pin number that is connected to the interrupt for Port B (INTB).</param>
        public Mcp23xxx(int deviceAddress, int? reset = null, int? interruptA = null, int? interruptB = null)
        {
            ValidateDeviceAddress(deviceAddress);

            DeviceAddress = deviceAddress;
            _reset = reset;
            _interruptA = interruptA;
            _interruptB = interruptB;

            InitializeMasterGpioController();
        }

        internal static void ValidateBitNumber(int bitNumber)
        {
            if (bitNumber < 0 || bitNumber > 7)
            {
                throw new IndexOutOfRangeException("Invalid bit index.");
            }
        }

        internal static void ClearBit(ref byte data, int bitNumber)
        {
            ValidateBitNumber(bitNumber);
            data &= (byte)~(1 << bitNumber);
        }

        internal void SetBit(ref byte data, int bitNumber)
        {
            ValidateBitNumber(bitNumber);
            data |= (byte)(1 << bitNumber);
        }

        internal static bool GetBit(byte data, int bitNumber)
        {
            ValidateBitNumber(bitNumber);
            return ((data >> bitNumber) & 1) == 1;
        }

        private void ValidateDeviceAddress(int deviceAddress)
        {
            if (deviceAddress < 0x20 || deviceAddress > 0x27)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceAddress), deviceAddress, "The Mcp23xxx address must be a value of 32 (0x20) - 39 (0x27).");
            }
        }

        private void InitializeMasterGpioController()
        {
            // Only need master controller if there are external pins provided.
            if (_reset != null || _interruptA != null || _interruptB != null)
            {
                _masterGpioController = new GpioController();

                if (_interruptA != null)
                {
                    _masterGpioController.OpenPin((int)_interruptA, PinMode.Input);
                }

                if (_interruptB != null)
                {
                    _masterGpioController.OpenPin((int)_interruptB, PinMode.Input);
                }

                if (_reset != null)
                {
                    _masterGpioController.OpenPin((int)_reset, PinMode.Output);
                    Disable();
                }
            }
        }

        public abstract int PinCount { get; }

        public abstract byte[] Read(Register.Address startingRegisterAddress, byte byteCount, Port port = Port.PortA, Bank bank = Bank.Bank1);
        public abstract void Write(Register.Address startingRegisterAddress, byte[] data, Port port = Port.PortA, Bank bank = Bank.Bank1);

        public virtual void Dispose()
        {
            if (_masterGpioController != null)
            {
                _masterGpioController.Dispose();
                _masterGpioController = null;
            }
        }

        public void Disable()
        {
            if (_masterGpioController == null)
            {
                throw new Exception("Master controller has not been initialized.");
            }

            if (_reset == null)
            {
                throw new Exception("Reset pin has not been initialized.");
            }

            _masterGpioController.Write((int)_reset, PinValue.Low);
        }

        public void Enable()
        {
            if (_masterGpioController == null)
            {
                throw new Exception("Master controller has not been initialized.");
            }

            if (_reset == null)
            {
                throw new Exception("Reset pin has not been initialized.");
            }

            _masterGpioController.Write((int)_reset, PinValue.High);
        }

        public byte Read(Register.Address registerAddress, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            byte[] data = Read(registerAddress, 1, port, bank);
            return data[0];
        }

        public bool ReadBit(Register.Address registerAddress, int bitNumber, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            ValidateBitNumber(bitNumber);
            byte data = Read(registerAddress, port, bank);
            return GetBit(data, bitNumber);
        }

        /// <summary>
        /// Read the pin value of interrupt for Port A (INTA).
        /// </summary>
        /// <returns>Pin value of interrupt for Port A (INTA).</returns>
        public PinValue ReadInterruptA()
        {
            if (_masterGpioController == null)
            {
                throw new Exception("Master controller has not been initialized.");
            }

            if (_reset == null)
            {
                throw new Exception("INTA pin has not been initialized.");
            }

            return _masterGpioController.Read((int)_interruptA);
        }

        /// <summary>
        /// Read the pin value of interrupt for Port B (INTB).
        /// </summary>
        /// <returns>Pin value of interrupt for Port B (INTB).</returns>
        public PinValue ReadInterruptB()
        {
            if (_masterGpioController == null)
            {
                throw new Exception("Master controller has not been initialized.");
            }

            if (_reset == null)
            {
                throw new Exception("INTB pin has not been initialized.");
            }

            return _masterGpioController.Read((int)_interruptB);
        }

        public void Write(Register.Address registerAddress, byte data, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            Write(registerAddress, new byte[] { data }, port, bank);
        }

        public void WriteBit(Register.Address registerAddress, int bitNumber, bool bit, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            ValidateBitNumber(bitNumber);
            byte data = Read(registerAddress, port, bank);

            if (bit)
            {
                SetBit(ref data, bitNumber);
            }
            else
            {
                ClearBit(ref data, bitNumber);
            }

            Write(registerAddress, data, port, bank);
        }
    }
}
