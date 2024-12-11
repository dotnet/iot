// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Spi;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ArduinoCsCompiler.Runtime;
using Iot.Device.Arduino;
using Iot.Device.Board;

namespace ArduinoCsCompiler
{
    /// <summary>
    /// This is the arduino board driver when running on the Arduino/ESP32. It is pretty simple, because
    /// it represents the board from it's own perspective.
    /// </summary>
    [ArduinoReplacement(typeof(ArduinoBoard), true, IncludingPrivates = true)]
    public class ArduinoNativeBoard : Board
    {
        public ArduinoNativeBoard()
        {
        }

        public static bool TryConnectToNetworkedBoard(IPAddress boardAddress, int port,
            [NotNullWhen(true)] out ArduinoBoard board)
        {
            board = null!;
            return false;
        }

        public static bool TryConnectToNetworkedBoard(IPAddress boardAddress, int port, bool useAutoReconnect,
            [NotNullWhen(true)] out ArduinoBoard? board)
        {
            board = null!;
            return false;
        }

        public static bool TryFindBoard(IEnumerable<string> comPorts, IEnumerable<int> baudRates,
            [NotNullWhen(true)] out ArduinoBoard? board)
        {
            var nativeboard = new ArduinoNativeBoard();
            // BEWARE: This only works because of the replacement in the runtime.
            board = MiniUnsafe.As<ArduinoBoard>(nativeboard);
            return true;
        }

        protected override I2cBusManager CreateI2cBusCore(int busNumber, int[]? pins)
        {
            return new I2cBusManager(this, busNumber, pins, new ArduinoNativeI2cBus(this, busNumber));
        }

        public override int GetDefaultI2cBusNumber()
        {
            return 0;
        }

        public override GpioController CreateGpioController()
        {
            return new GpioController(new ArduinoNativeGpioDriver());
        }

        protected override SpiDevice CreateSimpleSpiDevice(SpiConnectionSettings settings, int[] pins)
        {
            throw new NotSupportedException("SPI support not implemented");
        }

        protected override PwmChannel CreateSimplePwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage)
        {
            throw new NotSupportedException("PWM support not implemented");
        }

        public override int GetDefaultPinAssignmentForPwm(int chip, int channel)
        {
            if (chip != 0)
            {
                throw new NotSupportedException($"No PWM channel {chip}");
            }

            return channel;
        }

        public override int[] GetDefaultPinAssignmentForI2c(int busId)
        {
            if (busId != 0)
            {
                throw new NotSupportedException("Only bus number 0 is currently supported");
            }

            int pins = GetDefaultPinAssignmentForI2cInternal();
            return new int[2]
            {
                pins >> 8, pins & 0xFF
            };
        }

        public override int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings)
        {
            throw new NotImplementedException();
        }

        protected override void ActivatePinMode(int pinNumber, PinUsage usage)
        {
            base.ActivatePinMode(pinNumber, usage);
            ActivatePinModeInternal(pinNumber, usage);
        }

        [ArduinoImplementation("ArduinoNativeBoardActivatePinModeInternal")]
        private void ActivatePinModeInternal(int pinNumber, PinUsage usage)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// I2C pins, returned from hardware
        /// </summary>
        /// <returns>bits 0-7 Pin1, and 8-15 Pin2</returns>
        [ArduinoImplementation("ArduinoNativeBoardGetDefaultPinAssignmentForI2cInternal")]
        private int GetDefaultPinAssignmentForI2cInternal()
        {
            throw new NotImplementedException();
        }
    }
}
