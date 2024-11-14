// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnitsNet;

namespace Iot.Device.Board
{
    /// <summary>
    /// Raspberry Pi specific board implementation.
    /// Contains all the knowledge about which pins can be used for what purpose.
    /// </summary>
    public class RaspberryPiBoard : GenericBoard
    {
        private readonly string[] _configFile = new string[] { "/boot/firmware/config.txt", "/boot/config.txt" };
        private readonly object _initLock = new object();
        private readonly string[] _possibleI2cActivations = new string[]
        {
            "dtparam=i2c=on",
            "dtparam=i2c_arm=on",
            "dtparam=i2c_baudrate=",
            "dtparam=i2c_arm_baudrate=",
            // Activating only 1 I2C with the default pins:
            "dtoverlay=i2c0",
            "dtoverlay=i2c1",
            "dtoverlay=i2c3",
            "dtoverlay=i2c4",
            "dtoverlay=i2c5",
            "dtoverlay=i2c6",
            // We will use those ones to ensure valid options.
            "dtoverlay=i2c0,pins_0_1",
            "dtoverlay=i2c0,pins_28_29",
            "dtoverlay=i2c0,pins_44_45",
            "dtoverlay=i2c0,pins_46_47",
            "dtoverlay=i2c1,pins_2_3",
            "dtoverlay=i2c1,pins_44_45",
            "dtoverlay=i2c3,pins_2_3",
            "dtoverlay=i2c3,pins_4_5",
            "dtoverlay=i2c4,pins_6_7",
            "dtoverlay=i2c4,pins_8_9",
            "dtoverlay=i2c5,pins_10_11",
            "dtoverlay=i2c5,pins_12_13",
            "dtoverlay=i2c6,pins_0_1",
            "dtoverlay=i2c6,pins_22_23",
        };
        private readonly string[] _possibleSpiActivations = new string[]
        {
            "dtparam=spi=on",
            "dtoverlay=spi0-0cs",
            "dtoverlay=spi0-1cs",
            "dtoverlay=spi0-2cs",
            "dtoverlay=spi1-1cs",
            "dtoverlay=spi1-2cs",
            "dtoverlay=spi1-3cs",
            "dtoverlay=spi2-1cs",
            "dtoverlay=spi2-2cs",
            "dtoverlay=spi2-3cs",
            "dtoverlay=spi3-1cs",
            "dtoverlay=spi3-2cs",
            "dtoverlay=spi4-1cs",
            "dtoverlay=spi4-2cs",
            "dtoverlay=spi5-1cs",
            "dtoverlay=spi5-2cs",
            "dtoverlay=spi6-1cs",
            "dtoverlay=spi6-2cs",
        };
        private readonly string[] _possiblePwmActivations = new string[]
        {
            "dtoverlay=pwm",
            "dtoverlay=pwm-2chan",
        };

        private ManagedGpioController? _managedGpioController;
        private RaspberryPi3Driver? _raspberryPi3Driver;
        private bool _initialized;
        private List<string> _activateI2c = new List<string>();
        private List<string> _activateSpi = new List<string>();
        private List<string> _activatePwm = new List<string>();

        /// <summary>
        /// Creates an instance of a Rasperry Pi board.
        /// </summary>
        public RaspberryPiBoard()
        {
            // TODO: Ideally detect board type, so that invalid combinations can be prevented (i.e. I2C bus 2 on Raspi 3)
            PinCount = 28;
            _initialized = false;

            // Try to find the right configuration file
            // There has been changes in RPI and it's now "/boot/firmware/config.txt"
            // On older OS, the location is "/boot/config.txt"
            // BUT, the "/boot/config.txt" still exists on newer OS but the file is not accessible
            // So we have to try first the existance of the new location, then the old one
            if (File.Exists(_configFile[0]))
            {
                ConfigurationFile = _configFile[0];
            }
            else
            {
                ConfigurationFile = _configFile[1];
            }
        }

        /// <summary>
        /// Number of pins of the board
        /// </summary>
        public int PinCount
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the path to the configuration file for Raspberry PI.
        /// </summary>
        public string ConfigurationFile { get; set; } = "/boot/config.txt";

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
                case 2:
                    // Available only on the compute module
                    // GPIO​40 / PWM0 / SPI2 MISO / UART1 TX
                    // ​GPIO​41 / PWM1 / SPI2 MOSI / UART1 RX
                    // GPIO​42 / GPCLK1 / SPI2 SCLK / UART1 RTS
                    // GPIO​43 / GPCLK2 / SPI2 CE0 / UART1 CTS
                    // GPIO​44 / GPCLK1 / I2C0 SDA / I2C1 SDA / SPI2 CE1
                    // GPIO45 / PWM1 / I2C0 SCL / I2C1 SCL / SPI2 CE2
                    pins.Add(40);
                    pins.Add(41);
                    pins.Add(42);
                    if (cs == 0)
                    {
                        pins.Add(43);
                    }
                    else if (cs == 1)
                    {
                        pins.Add(44);
                    }
                    else if (cs == 2)
                    {
                        pins.Add(45);
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
        /// <param name="bus">Optional bus argument, for SPI and I2C pins</param>
        /// <returns>
        /// A member of <see cref="RaspberryPi3Driver.AltMode"/> describing the mode the pin is in.</returns>
        private RaspberryPi3Driver.AltMode GetHardwareModeForPinUsage(int pinNumber, PinUsage usage, int bus = 0)
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

            var modeToSet = GetHardwareModeForPinUsage(pinNumber, usage);
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
            var possibleAltMode = GetHardwareModeForPinUsage(pinNumber, PinUsage.I2c);
            if (possibleAltMode == pinMode)
            {
                return PinUsage.I2c;
            }

            possibleAltMode = GetHardwareModeForPinUsage(pinNumber, PinUsage.Spi);
            if (possibleAltMode == pinMode)
            {
                return PinUsage.Spi;
            }

            possibleAltMode = GetHardwareModeForPinUsage(pinNumber, PinUsage.Pwm);
            if (possibleAltMode == pinMode)
            {
                return PinUsage.Pwm;
            }

            return PinUsage.Unknown;
        }

        /// <summary>
        /// Checks if the I2C overlay is activated in the configuraztion file.
        /// </summary>
        /// <returns>True if it is.</returns>
        public bool IsI2cActivated()
        {
            _activateI2c.Clear();
            // We are checking possible activation from here: https://github.com/dotnet/iot/blob/main/Documentation/raspi-i2c.md
            var config = File.ReadAllText(ConfigurationFile).Split('\n').Where(m => !m.Trim().StartsWith("#")).Where(m => m.Length > 1);

            foreach (var possibleActivation in _possibleI2cActivations)
            {
                // We need the actual line as the configuration can be more complex than the list.
                var choices = config.Where(m => m.Trim().StartsWith(possibleActivation));
                if (choices.Any())
                {
                    _activateI2c.Add(choices.First().Replace("\r", string.Empty));

                }
            }

            return _activateI2c.Any();
        }

        /// <summary>
        /// Gets the overlay pin assignment for I2C.
        /// </summary>
        /// <param name="busId">Bus Id.</param>
        /// <returns>The set of pins for the given I2C bus.</returns>
        public int[] GetOverlayPinAssignmentForI2c(int busId)
        {
            int scl = -1;
            int sda = -1;
            if (!IsI2cActivated())
            {
                return new int[0];
            }

            // Checks if there is an overlay because it does override the default configuration
            var dtoverlay = _activateI2c.Where(m => m.StartsWith($"dtoverlay=i2c{busId},")).FirstOrDefault();
            if (string.IsNullOrEmpty(dtoverlay))
            {
                // Give another try without the parameters
                dtoverlay = _activateI2c.Where(m => m.StartsWith($"dtoverlay=i2c{busId}")).FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(dtoverlay))
            {
                // we have an overlay, extract the pins and check them
                // dtoverlay=i2c1,pins_2_3
                if (dtoverlay.IndexOf('_') > 0)
                {
                    var pins = dtoverlay.Substring(dtoverlay.IndexOf('_') + 1).Split('_');
                    sda = int.Parse(pins[0]);
                    scl = int.Parse(pins[1]);
                    // Rebuild the chain and check those are valid options
                    var rebuilt = $"dtoverlay=i2c{busId},pins_{sda}_{scl}";
                    if (!_possibleI2cActivations.Contains(rebuilt))
                    {
                        throw new InvalidOperationException($"Invalid I2C overlay configuration: {dtoverlay}");
                    }
                }
                else
                {
                    return GetDefaultPinAssignmentForI2c(busId);
                }
            }
            else
            {
                var dtparam = _activateI2c.Where(m => m.StartsWith($"dtparam"));
                if (dtparam.Any())
                {
                    // We're using the default one
                    return GetDefaultPinAssignmentForI2c(busId);
                }
            }

            return new int[]
            {
                // Return in the default scheme of the board
                sda,
                scl
            };
        }

        /// <summary>
        /// Checks if the SPI overlay is activated in the configuraztion file.
        /// </summary>
        /// <returns>True if it is.</returns>
        public bool IsSpiActivated()
        {
            _activateI2c.Clear();
            // We are checking possible activation from here: https://github.com/dotnet/iot/blob/main/Documentation/raspi-i2c.md
            var config = File.ReadAllText(ConfigurationFile).Split('\n').Where(m => !m.Trim().StartsWith("#")).Where(m => m.Length > 1);

            foreach (var possibleActivation in _possibleSpiActivations)
            {
                // We need the actual line as the configuration can be more complex than the list.
                var choices = config.Where(m => m.Trim().StartsWith(possibleActivation));
                if (choices.Any())
                {
                    // Remove the \r as introduced on the Windows machine for the tests.
                    _activateSpi.Add(choices.First().Replace("\r", string.Empty));
                }
            }

            return _activateSpi.Any();
        }

        /// <summary>
        /// Gets the overlay pin assignment for Spi.
        /// </summary>
        /// <param name="connectionSettings">Connection settings to check.</param>
        /// <returns>The set of pins for the given SPI bus. If no miso, it will be marked as -1.</returns>
        public int[] GetOverlayPinAssignmentForSpi(SpiConnectionSettings connectionSettings)
        {
            int[] pins = new int[0];
            if (!IsSpiActivated())
            {
                return pins;
            }

            // Checks if there is an overlay because it does override the default configuration
            var dtoverlay = _activateSpi.Where(m => m.StartsWith($"dtoverlay=spi{connectionSettings.BusId}")).FirstOrDefault();
            if (!string.IsNullOrEmpty(dtoverlay))
            {
                // Overlays look like this:
                // dtoverlay=spi4-2cs,cs1_pin=17,cs1_spidev=disabled
                // dtoverlay=spi0-2cs,cs0_pin=27,cs1_pin=22
                // Can be as well: dtoverlay=spi0-2cs
                // Or: dtoverlay=spi0-1cs,nomiso
                // First find number of Chip Select
                var numberCs = int.Parse(dtoverlay.Substring(dtoverlay.IndexOf('-') + 1, 1));
                if ((connectionSettings.ChipSelectLine >= numberCs))
                {
                    throw new ArgumentException($"SPI {connectionSettings.BusId} is setup with {numberCs} chip select and you ask for number {connectionSettings.ChipSelectLine}.");
                }

                // This does returns
                // MISO, MISO, CLK and CS
                pins = GetDefaultPinAssignmentForSpi(connectionSettings);
                // If SPI Bus is 0, check if we have no_miso
                if ((connectionSettings.BusId == 0) && dtoverlay.Contains("no_miso"))
                {
                    pins[0] = -1;
                }

                // Now let's check the CS if it's the default value or not
                string csDefined = $"cs{connectionSettings.ChipSelectLine}_pin=";
                if (dtoverlay.Contains(csDefined))
                {
                    // In case it's part of a string, we keep only the first one
                    var pinValueStr = dtoverlay.Substring(dtoverlay.IndexOf(csDefined) + csDefined.Length).Split(',')[0].Trim();
                    pins[3] = int.Parse(pinValueStr);
                }

                return pins;
            }
            else
            {
                var dtparam = _activateSpi.Where(m => m.StartsWith($"dtparam"));
                if (dtparam.Any())
                {
                    // We're using the default one
                    return GetDefaultPinAssignmentForSpi(connectionSettings);
                }
            }

            return pins;
        }

        /// <summary>
        /// Checks if the I2C overlay is activated in the configuraztion file.
        /// </summary>
        /// <returns>True if it is.</returns>
        public bool IsPwmActivated()
        {
            _activatePwm.Clear();
            // We are checking possible activation from here: https://github.com/dotnet/iot/blob/main/Documentation/raspi-pwm.md
            var config = File.ReadAllText(ConfigurationFile).Split('\n').Where(m => !m.Trim().StartsWith("#")).Where(m => m.Length > 1);

            foreach (var possibleActivation in _possiblePwmActivations)
            {
                // We need the actual line as the configuration can be more complex than the list.
                var choices = config.Where(m => m.Trim().StartsWith(possibleActivation));
                if (choices.Any())
                {
                    _activatePwm.Add(choices.First().Replace("\r", string.Empty));

                }
            }

            return _activatePwm.Any();
        }

        /// <summary>
        /// Gets the overlay pin assignment for Pwm.
        /// </summary>
        /// <param name="pwmChannel">The PWM channel.</param>
        /// <returns>The set of pins for the given Pwm bus on chipn 0 as only one supported.</returns>
        public int GetOverlayPinAssignmentForPwm(int pwmChannel)
        {
            int[] validPwm0 = new int[] { 12, 18, 40, 52 };
            int[] validPwm1 = new int[] { 13, 19, 41, 45, 53 };

            int pin = -1;
            if (!IsPwmActivated())
            {
                return pin;
            }

            // Checks if there is an overlay because it does override the default configuration
            var dtoverlay = _activatePwm.Where(m => m.StartsWith($"dtoverlay=pwm")).FirstOrDefault();
            if (!string.IsNullOrEmpty(dtoverlay))
            {
                // we have an overlay, extract the pins and check them
                // dtoverlay=pwm,pin=19,func=2
                // dtoverlay=pwm-2chan,pin=12,func=4,pin2=13,func2=4
                // Single or dual?
                if ((pwmChannel == 1) && dtoverlay.Contains("2chan"))
                {
                    // Do we have an overlay with pins?
                    var possibleDtoverlay = _activatePwm.Where(m => m.StartsWith($"dtoverlay=pwm-2chan,")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(possibleDtoverlay))
                    {
                        dtoverlay = possibleDtoverlay;
                    }

                    if (dtoverlay.IndexOf("pin2=") > 0)
                    {
                        // 2 channels
                        pin = int.Parse(dtoverlay.Substring(dtoverlay.IndexOf("pin2=") + 5).Split(',')[0]);
                        // Is it a potential valid pin?
                        if (!validPwm1.Contains(pin))
                        {
                            throw new ArgumentException($"PWM{pwmChannel} pin2 is not a valid pin.");
                        }
                    }
                    else
                    {
                        // We'll use the default one
                        pin = 19;
                    }
                }
                else if (pwmChannel == 0)
                {
                    // Do we have an overlay with pins?
                    var possibleDtoverlay = _activatePwm.Where(m => m.StartsWith($"dtoverlay=pwm,")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(possibleDtoverlay))
                    {
                        dtoverlay = possibleDtoverlay;
                    }

                    if (dtoverlay.IndexOf("pin=") > 0)
                    {
                        pin = int.Parse(dtoverlay.Substring(dtoverlay.IndexOf("pin=") + 4).Split(',')[0]);
                        // Is it a potential valid pin?
                        if (!validPwm0.Contains(pin))
                        {
                            throw new ArgumentException($"PWM{pwmChannel} pin is not a valid pin.");
                        }
                    }
                    else
                    {
                        // We'll use the default one
                        pin = 18;

                    }
                }
            }

            return pin;
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

        /// <inheritdoc />
        public override ComponentInformation QueryComponentInformation()
        {
            ComponentInformation self = base.QueryComponentInformation();
            var ret = self with
            {
                Description = $"Raspberry Pi with {PinCount} pins"
            };

            ret.Properties["PinCount"] = PinCount.ToString(CultureInfo.InvariantCulture);
            return ret;
        }
    }
}
