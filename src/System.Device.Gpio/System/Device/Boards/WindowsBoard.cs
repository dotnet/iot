using System;
using System.Collections.Generic;
using System.Device.Analog;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;
using System.Text;

namespace System.Device.Boards
{
    public class WindowsBoard : Board
    {
        public WindowsBoard(PinNumberingScheme defaultNumberingScheme)
            : base(defaultNumberingScheme)
        {
            if (defaultNumberingScheme != PinNumberingScheme.Board)
            {
                throw new NotSupportedException("This board only supports logical pin numbering");
            }
        }

        public override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        public override int ConvertLogicalNumberingSchemeToPinNumber(int pinNumber)
        {
            return pinNumber;
        }

        public override GpioController CreateGpioController(PinNumberingScheme pinNumberingScheme)
        {
            return new GpioController(DefaultPinNumberingScheme, new Windows10Driver(this), this);
        }

        public override I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings)
        {
            return new Windows10I2cDevice(connectionSettings, this);
        }

        public override SpiDevice CreateSpiDevice(SpiConnectionSettings settings)
        {
            return new Windows10SpiDevice(settings, this);
        }

        public override PwmChannel CreatePwmChannel(int chip, int channel, int frequency = 400, double dutyCyclePercentage = 0.5)
        {
            throw new NotImplementedException();
        }

        public override AnalogController CreateAnalogController(int chip)
        {
            throw new NotImplementedException();
        }
    }
}
