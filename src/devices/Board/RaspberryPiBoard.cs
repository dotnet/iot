// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;
using System.Text;

namespace Iot.Device.Board
{
    /// <summary>
    /// Raspberry Pi specific board implementation.
    /// Contains all the knowledge about which pins can be used for what purpose.
    /// </summary>
    public class RaspberryPiBoard : GenericBoard
    {
        private ManagedGpioController? _managedGpioController;

        /// <summary>
        /// Creates an instance of a Rasperry Pi board.
        /// </summary>
        public RaspberryPiBoard(PinNumberingScheme defaultNumberingScheme)
            : base(defaultNumberingScheme)
        {
            // TODO: Ideally detect board type, so that invalid combinations can be prevented (i.e. I2C bus 2 on Raspi 3)
            PinCount = 28;
        }

        /// <summary>
        /// Number of pins of the board
        /// </summary>
        public int PinCount
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        protected override GpioDriver? TryCreateBestGpioDriver()
        {
            return new RaspberryPi3Driver();
        }

        /// <summary>
        /// Initializes this instance
        /// </summary>
        /// <exception cref="NotSupportedException">The current hardware could not be identified as a valid Raspberry Pi type</exception>
        public override void Initialize()
        {
            // Needs to be a raspi 3 driver here (either unix or windows)
            GpioDriver? driver = TryCreateBestGpioDriver();
            if (driver == null)
            {
                throw new NotSupportedException("Could not initialize the RaspberryPi GPIO driver");
            }

            _managedGpioController = new ManagedGpioController(this, DefaultPinNumberingScheme, driver);
            PinCount = _managedGpioController.PinCount;
            base.Initialize();
        }

        /// <summary>
        /// Convert a pin number from one scheme to another
        /// </summary>
        /// <param name="pinNumber">Pin number</param>
        /// <param name="inputScheme">Input scheme</param>
        /// <param name="outputScheme">Output scheme</param>
        /// <returns>Pin number in the output scheme</returns>
        public override int ConvertPinNumber(int pinNumber, PinNumberingScheme inputScheme, PinNumberingScheme outputScheme)
        {
            if (inputScheme == outputScheme)
            {
                return pinNumber;
            }

            if (inputScheme == PinNumberingScheme.Board && outputScheme == PinNumberingScheme.Logical)
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
            else if (inputScheme == PinNumberingScheme.Logical && outputScheme == PinNumberingScheme.Board)
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
                    12 => 32,
                    13 => 33,
                    19 => 35,
                    16 => 36,
                    26 => 37,
                    20 => 38,
                    21 => 40,
                    _ => throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber))
                };
            }

            throw new NotSupportedException("Unsupported numbering scheme combination");
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForI2c(int busId)
        {
            int scl;
            int sda;
            switch (busId)
            {
                case 0:
                {
                    // Bus 0 is the one on logical pins 0 and 1. According to the docs, it should not
                    // be used by application software and instead is reserved for HATs, but if you don't have one, it is free for other purposes
                    sda = 0;
                    scl = 1;
                    break;
                }

                case 1:
                {
                    // This is the bus commonly used by application software.
                    sda = 2;
                    scl = 3;
                    break;
                }

                case 2:
                {
                    throw new NotSupportedException("I2C Bus number 2 doesn't exist");
                }

                case 3:
                {
                    sda = 4;
                    scl = 5;
                    break;
                }

                case 4:
                {
                    sda = 6;
                    scl = 7;
                    break;
                }

                case 5:
                {
                    sda = 10;
                    scl = 11;
                    break;
                }

                case 6:
                    sda = 22;
                    scl = 23;
                    break;

                default:
                    throw new NotSupportedException($"I2C bus {busId} does not exist.");
            }

            return new int[]
            {
                // Return in the default scheme of the board
                ConvertPinNumber(sda, PinNumberingScheme.Logical, DefaultPinNumberingScheme),
                ConvertPinNumber(scl, PinNumberingScheme.Logical, DefaultPinNumberingScheme)
            };
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings)
        {
            int cs = connectionSettings.ChipSelectLine;
            // If hardware CS is used, the CS selection must be 0 or 1, since only that is supported
            // (except for bus 1, which has 3 pre-defined CS lines)
            if ((cs >= 2 || cs < -1) && !((connectionSettings.BusId == 1) && cs == 2))
            {
                throw new ArgumentOutOfRangeException(nameof(connectionSettings), "Chip select line must be 0 or 1");
            }

            List<int> pins = new List<int>();
            switch (connectionSettings.BusId)
            {
                case 0:
                    pins.Add(9);
                    pins.Add(10);
                    pins.Add(11);
                    if (cs == 0)
                    {
                        pins.Add(8);
                    }
                    else if (cs == 1)
                    {
                        pins.Add(7);
                    }

                    break;
                case 1:
                    pins.Add(19);
                    pins.Add(20);
                    pins.Add(21);
                    if (cs == 0)
                    {
                        pins.Add(18);
                    }
                    else if (cs == 1)
                    {
                        pins.Add(17);
                    }
                    else if (cs == 2)
                    {
                        pins.Add(16);
                    }

                    break;
                case 3:
                    pins.Add(1);
                    pins.Add(2);
                    pins.Add(3);
                    if (cs == 0)
                    {
                        pins.Add(0);
                    }
                    else if (cs == 1)
                    {
                        pins.Add(24);
                    }

                    break;
                case 4:
                    pins.Add(5);
                    pins.Add(6);
                    pins.Add(7);
                    if (cs == 0)
                    {
                        pins.Add(4);
                    }
                    else if (cs == 1)
                    {
                        pins.Add(25);
                    }

                    break;
                case 5:
                    pins.Add(13);
                    pins.Add(14);
                    pins.Add(15);
                    if (cs == 0)
                    {
                        pins.Add(12);
                    }
                    else if (cs == 1)
                    {
                        pins.Add(26);
                    }

                    break;
                case 6:
                    pins.Add(19);
                    pins.Add(20);
                    pins.Add(21);
                    if (cs == 0)
                    {
                        pins.Add(18);
                    }
                    else if (cs == 1)
                    {
                        pins.Add(27);
                    }

                    break;

                default:
                    throw new NotSupportedException($"No bus number {connectionSettings.BusId}");
            }

            return pins.ToArray();
        }

        /// <inheritdoc />
        public override AlternatePinMode GetHardwareModeForPinUsage(int pinNumber, PinUsage usage, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, int bus = 0)
        {
            pinNumber = RemapPin(pinNumber, pinNumberingScheme);
            if (pinNumber >= PinCount)
            {
                throw new InvalidOperationException($"Invalid pin number {pinNumber}");
            }

            if (usage == PinUsage.Gpio)
            {
                // all pins support GPIO
                return AlternatePinMode.Gpio;
            }

            if (usage == PinUsage.I2c)
            {
                // The Pi4 has a big number of pins that can become I2C pins
                switch (pinNumber)
                {
                    // Busses 0 and 1 run on Alt0
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        return AlternatePinMode.Alt0;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                        return AlternatePinMode.Alt5;
                    case 22:
                    case 23:
                        return AlternatePinMode.Alt5;
                }

                throw new NotSupportedException($"No I2C support on Pin {pinNumber}.");
            }

            if (usage == PinUsage.Pwm)
            {
                if (pinNumber == 12 || pinNumber == 13)
                {
                    return AlternatePinMode.Alt0;
                }

                if (pinNumber == 18 || pinNumber == 19)
                {
                    return AlternatePinMode.Alt5;
                }

                throw new NotSupportedException($"No Pwm support on Pin {pinNumber}.");
            }

            if (usage == PinUsage.Spi)
            {
                switch (pinNumber)
                {
                    case 7: // Pin 7 can be assigned to either SPI0 or SPI4
                        return bus == 0 ? AlternatePinMode.Alt0 : AlternatePinMode.Alt3;
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        return AlternatePinMode.Alt0;
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        return AlternatePinMode.Alt3;
                    case 4:
                    case 5:
                    case 6:
                        return AlternatePinMode.Alt3;
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        return AlternatePinMode.Alt3;
                    case 16:
                    case 17:
                        return AlternatePinMode.Alt4;
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                        return bus == 6 ? AlternatePinMode.Alt3 : AlternatePinMode.Alt4;
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                        return AlternatePinMode.Alt5;
                }

                throw new NotSupportedException($"No SPI support on Pin {pinNumber}.");
            }

            if (usage == PinUsage.Uart)
            {
                switch (pinNumber)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        return AlternatePinMode.Alt4;
                    case 14:
                    case 15:
                        if (bus == 0)
                        {
                            return AlternatePinMode.Alt0;
                        }
                        else if (bus == 5)
                        {
                            return AlternatePinMode.Alt4;
                        }
                        else if (bus == 1)
                        {
                            return AlternatePinMode.Alt5;
                        }

                        break;
                    case 16:
                    case 17:
                        return (bus == 0) ? AlternatePinMode.Alt3 : AlternatePinMode.Alt5;
                }

                throw new NotSupportedException($"No Uart support on Pin {pinNumber}.");
            }

            throw new NotSupportedException($"There are no known pins for {usage}.");
        }

        /// <inheritdoc />
        public override I2cBusManager CreateOrGetDefaultI2cBus()
        {
            return CreateOrGetI2cBus(1, GetDefaultPinAssignmentForI2c(1));
        }

        /// <inheritdoc />
        public override int GetDefaultPinAssignmentForPwm(int chip, int channel)
        {
            // The default assignment is 12 & 13, but 18 and 19 is supported as well
            if (chip == 0 && channel == 0)
            {
                return ConvertPinNumber(12, PinNumberingScheme.Logical, DefaultPinNumberingScheme);
            }

            if (chip == 0 && channel == 1)
            {
                return ConvertPinNumber(13, PinNumberingScheme.Logical, DefaultPinNumberingScheme);
            }

            throw new NotSupportedException($"No such PWM Channel: Chip {chip} channel {channel}.");
        }

        /// <summary>
        /// Switches a pin to a certain alternate mode. (ALTn mode)
        /// </summary>
        /// <param name="pinNumber">The pin number in the logical scheme</param>
        /// <param name="usage">The desired usage</param>
        protected override void ActivatePinMode(int pinNumber, PinUsage usage)
        {
            if (_managedGpioController == null)
            {
                throw new InvalidOperationException("Board not initialized");
            }

            AlternatePinMode modeToSet = GetHardwareModeForPinUsage(pinNumber, usage, PinNumberingScheme.Logical);
            if (modeToSet != AlternatePinMode.Unknown)
            {
                _managedGpioController.SetAlternatePinMode(pinNumber, modeToSet);
            }

            base.ActivatePinMode(pinNumber, usage);
        }

        /// <summary>
        /// Gets the current alternate pin mode. (ALTn mode)
        /// </summary>
        /// <param name="pinNumber">Pin number, in the logical scheme</param>
        /// <returns>The current pin usage</returns>
        /// <remarks>This also works for closed pins, but then uses a bit of heuristics to get the correct mode</remarks>
        public override PinUsage DetermineCurrentPinUsage(int pinNumber)
        {
            if (_managedGpioController == null)
            {
                throw new InvalidOperationException("Board not initialized");
            }

            PinUsage cached = base.DetermineCurrentPinUsage(pinNumber);
            if (cached != PinUsage.Unknown)
            {
                return cached;
            }

            AlternatePinMode pinMode = _managedGpioController.GetAlternatePinMode(pinNumber);
            if (pinMode == AlternatePinMode.Gpio)
            {
                return PinUsage.Gpio;
            }

            // Do some heuristics: If the given pin number can be used for I2C with the same Alt mode, we can assume that's what it
            // it set to.
            AlternatePinMode possibleAltMode = GetHardwareModeForPinUsage(pinNumber, PinUsage.I2c, DefaultPinNumberingScheme);
            if (possibleAltMode == pinMode)
            {
                return PinUsage.I2c;
            }

            possibleAltMode = GetHardwareModeForPinUsage(pinNumber, PinUsage.Spi, DefaultPinNumberingScheme);
            if (possibleAltMode == pinMode)
            {
                return PinUsage.Spi;
            }

            possibleAltMode = GetHardwareModeForPinUsage(pinNumber, PinUsage.Pwm, DefaultPinNumberingScheme);
            if (possibleAltMode == pinMode)
            {
                return PinUsage.Pwm;
            }

            return PinUsage.Unknown;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _managedGpioController?.Dispose();
                _managedGpioController = null;
            }

            base.Dispose(disposing);
        }
    }
}
