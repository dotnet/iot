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
        private readonly object _initLock = new object();

        private ManagedGpioController? _managedGpioController;
        private RaspberryPi3Driver? _raspberryPi3Driver;
        private bool _initialized;

        /// <summary>
        /// Creates an instance of a Rasperry Pi board.
        /// </summary>
        public RaspberryPiBoard()
        {
            // TODO: Ideally detect board type, so that invalid combinations can be prevented (i.e. I2C bus 2 on Raspi 3)
            PinCount = 28;
            _initialized = false;
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
        protected override void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            lock (_initLock)
            {
                if (_initialized)
                {
                    return;
                }

                // Needs to be a raspi 3 driver here (either unix or windows)
                GpioDriver? driver = TryCreateBestGpioDriver();
                if (driver == null)
                {
                    throw new NotSupportedException("Could not initialize the RaspberryPi GPIO driver");
                }

                _managedGpioController = new ManagedGpioController(this, driver);
                _raspberryPi3Driver = driver as RaspberryPi3Driver;

                PinCount = _managedGpioController.PinCount;
                _initialized = true;
            }

            base.Initialize();
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
                sda,
                scl
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

        /// <summary>
        /// Gets the board-specific hardware mode for a particular pin and pin usage (i.e. the different ALTn modes on the raspberry pi)
        /// </summary>
        /// <param name="pinNumber">Pin number to use</param>
        /// <param name="usage">Requested usage</param>
        /// <param name="pinNumberingScheme">Pin numbering scheme for the pin provided (logical or physical)</param>
        /// <param name="bus">Optional bus argument, for SPI and I2C pins</param>
        /// <returns>
        /// A member of <see cref="RaspberryPi3Driver.AltMode"/> describing the mode the pin is in.</returns>
        private RaspberryPi3Driver.AltMode GetHardwareModeForPinUsage(int pinNumber, PinUsage usage, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, int bus = 0)
        {
            if (pinNumber >= PinCount)
            {
                throw new InvalidOperationException($"Invalid pin number {pinNumber}");
            }

            if (usage == PinUsage.Gpio)
            {
                // all pins support GPIO
                return RaspberryPi3Driver.AltMode.Input;
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
                        return RaspberryPi3Driver.AltMode.Alt0;
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
                        return RaspberryPi3Driver.AltMode.Alt5;
                    case 22:
                    case 23:
                        return RaspberryPi3Driver.AltMode.Alt5;
                }

                throw new NotSupportedException($"No I2C support on Pin {pinNumber}.");
            }

            if (usage == PinUsage.Pwm)
            {
                if (pinNumber == 12 || pinNumber == 13)
                {
                    return RaspberryPi3Driver.AltMode.Alt0;
                }

                if (pinNumber == 18 || pinNumber == 19)
                {
                    return RaspberryPi3Driver.AltMode.Alt5;
                }

                throw new NotSupportedException($"No Pwm support on Pin {pinNumber}.");
            }

            if (usage == PinUsage.Spi)
            {
                switch (pinNumber)
                {
                    case 7: // Pin 7 can be assigned to either SPI0 or SPI4
                        return bus == 0 ? RaspberryPi3Driver.AltMode.Alt0 : RaspberryPi3Driver.AltMode.Alt3;
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        return RaspberryPi3Driver.AltMode.Alt0;
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        return RaspberryPi3Driver.AltMode.Alt3;
                    case 4:
                    case 5:
                    case 6:
                        return RaspberryPi3Driver.AltMode.Alt3;
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        return RaspberryPi3Driver.AltMode.Alt3;
                    case 16:
                    case 17:
                        return RaspberryPi3Driver.AltMode.Alt4;
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                        return bus == 6 ? RaspberryPi3Driver.AltMode.Alt3 : RaspberryPi3Driver.AltMode.Alt4;
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                        return RaspberryPi3Driver.AltMode.Alt5;
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
                        return RaspberryPi3Driver.AltMode.Alt4;
                    case 14:
                    case 15:
                        if (bus == 0)
                        {
                            return RaspberryPi3Driver.AltMode.Alt0;
                        }
                        else if (bus == 5)
                        {
                            return RaspberryPi3Driver.AltMode.Alt4;
                        }
                        else if (bus == 1)
                        {
                            return RaspberryPi3Driver.AltMode.Alt5;
                        }

                        break;
                    case 16:
                    case 17:
                        return (bus == 0) ? RaspberryPi3Driver.AltMode.Alt3 : RaspberryPi3Driver.AltMode.Alt5;
                }

                throw new NotSupportedException($"No Uart support on Pin {pinNumber}.");
            }

            throw new NotSupportedException($"There are no known pins for {usage}.");
        }

        /// <inheritdoc />
        public override int GetDefaultI2cBusNumber()
        {
            return 1;
        }

        /// <inheritdoc />
        public override int GetDefaultPinAssignmentForPwm(int chip, int channel)
        {
            // The default assignment is 12 & 13, but 18 and 19 is supported as well
            if (chip == 0 && channel == 0)
            {
                return 12;
            }

            if (chip == 0 && channel == 1)
            {
                return 13;
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

            if (_raspberryPi3Driver == null || !_raspberryPi3Driver.AlternatePinModeSettingSupported)
            {
                throw new NotSupportedException("Alternate pin mode setting not supported by driver");
            }

            var modeToSet = GetHardwareModeForPinUsage(pinNumber, usage, PinNumberingScheme.Logical);
            if (modeToSet != RaspberryPi3Driver.AltMode.Unknown)
            {
                _raspberryPi3Driver.SetAlternatePinMode(pinNumber, modeToSet);
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

            if (_raspberryPi3Driver == null || !_raspberryPi3Driver.AlternatePinModeSettingSupported)
            {
                throw new NotSupportedException("Alternate pin mode setting not supported by driver");
            }

            var pinMode = _raspberryPi3Driver.GetAlternatePinMode(pinNumber);
            if (pinMode == RaspberryPi3Driver.AltMode.Input || pinMode == RaspberryPi3Driver.AltMode.Output)
            {
                return PinUsage.Gpio;
            }

            // Do some heuristics: If the given pin number can be used for I2C with the same Alt mode, we can assume that's what it
            // it set to.
            var possibleAltMode = GetHardwareModeForPinUsage(pinNumber, PinUsage.I2c, DefaultPinNumberingScheme);
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
