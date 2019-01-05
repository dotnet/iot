// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.Mcp23xxx
{
    // TODO: https://github.com/dotnet/iot/issues/125
    // Implement IGpioControllerProvider.
    // The thinking is to implement this interface where it can provide a GpioController
    // that looks like a regular controller, but controls the bindings I/O via I2C/SPI drivers.
    //
    // For example...
    // var connectionSettings = new SpiConnectionSettings(0, 0);
    // var spiDevice = new UnixSpiDevice(connectionSettings);
    // var mcp23Sxx = new Mcp23Sxx(0, spiDevice);
    // GpioController mcp23SxxController = mcp23Sxx.GetDefaultGpioController();
    // 
    // Now when you call the GpioController methods, the master controller will send the respective SPI commands
    // to the binding behind the scenes to update its I/O.
    //
    // The code below would interact with the MCP23XXX related registers (IODIR, GPIO, etc.)
    // to configure the pin as an input and read the the value.
    // mcp23SxxController.SetPinMode(1, PinMode.Input);
    // PinValue pin1Value = mcp23SxxController.Read(1);

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

        public abstract byte Read(Register.Address registerAddress, Port port = Port.PortA, Bank bank = Bank.Bank1);
        public abstract byte[] Read(Register.Address startingRegisterAddress, byte byteCount, Port port = Port.PortA, Bank bank = Bank.Bank1);
        public abstract void Write(Register.Address registerAddress, byte data, Port port = Port.PortA, Bank bank = Bank.Bank1);
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
    }
}
