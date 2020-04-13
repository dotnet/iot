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
    public class RaspberryPiBoard : UnixBoard
    {
        public RaspberryPiBoard(PinNumberingScheme defaultNumberingScheme, bool useLibgpiod = true)
            : base(defaultNumberingScheme, useLibgpiod)
        {
        }

        public override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber switch
            {
                3 => 2,
                5 => 3,
                7 => 4,
                8 => 14,
                10 => 15,
                11 => 17,
                12 => 18,
                13 => 27,
                15 => 22,
                16 => 23,
                18 => 24,
                19 => 10,
                21 => 9,
                22 => 25,
                23 => 11,
                24 => 8,
                26 => 7,
                27 => 0,
                28 => 1,
                29 => 5,
                31 => 6,
                32 => 12,
                33 => 13,
                35 => 19,
                36 => 16,
                37 => 26,
                38 => 20,
                40 => 21,
                _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
            };
        }

        public override int ConvertLogicalNumberingSchemeToPinNumber(int pinNumber)
        {
            return pinNumber switch
            {
                2 => 3,
                3 => 5,
                4 => 7,
                14 => 8,
                15 => 10,
                17 => 11,
                18 => 12,
                27 => 13,
                22 => 15,
                23 => 16,
                24 => 18,
                10 => 19,
                9 => 21,
                25 => 22,
                11 => 23,
                8 => 24,
                7 => 26,
                0 => 27,
                1 => 28,
                5 => 29,
                6 => 31,
                12 => 23,
                13 => 33,
                19 => 35,
                16 => 36,
                26 => 37,
                20 => 38,
                21 => 40,
                _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
            };
        }

        protected override GpioDriver CreateGpioDriver()
        {
            return new RaspberryPi3Driver(this);
        }

        /// <summary>
        /// Creates an I2C device object for the given bus and device id.
        /// See the Raspberry Pi manual for possible bus-to-pin assignments.
        /// </summary>
        /// <param name="connectionSettings">I2C connection settings</param>
        /// <returns>An <see cref="I2cDevice"/> instance</returns>
        public override I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings)
        {
            int scl = -1;
            int sda = -1;
            switch (connectionSettings.BusId)
            {
                case 0:
                {
                    // Bus 0 is the one on logical pins 0 and 1. According to the docs, it should not
                    // be used by application software and instead is reserved for HATs, but who does really care?
                    scl = 1;
                    sda = 0;
                    break;
                }

                case 1:
                {
                    // This is the bus commonly used by application software.
                    sda = 2;
                    scl = 3;
                    break;
                }

                default:
                {
                    // Lets assume here the user knows what he's doing. Otherwise, it will just fail later (or he'll not get any
                    // reply from the device
                    sda = connectionSettings.SdaPin;
                    scl = connectionSettings.SclPin;
                    break;
                }
            }

            if (scl == -1 || sda == -1)
            {
                throw new ArgumentException("For I2C buses other than 0 and 1, the SDA and SCL pins must be explicitly specified", nameof(connectionSettings));
            }

            connectionSettings = new I2cConnectionSettings(connectionSettings.BusId, connectionSettings.DeviceAddress, sda, scl);
            return new UnixI2cDevice(connectionSettings, this);
        }

        protected override void ActivatePinMode(int pinNumber, PinUsage usage)
        {
            // TODO: Set extended pin modes (ALT0-ALT5)
            base.ActivatePinMode(pinNumber, usage);
        }
    }
}
