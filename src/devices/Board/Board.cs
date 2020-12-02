// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Iot.Device.Board
{
    /// <summary>
    /// Base class for all board abstractions.
    /// A "board" is a piece of hardware that offers low-level interfaces to other devices. Typically, it has GPIO pins and one or multiple SPI or I2C busses.
    /// There should be exactly one instance of a board class per hardware component in an application, but it is possible to work with multiple boards
    /// at once (i.e. when having a GPIO expander connected to the Raspberry Pi)
    /// </summary>
    public abstract class Board : MarshalByRefObject, IDisposable
    {
        // See comment at GetBestDriverForBoardOnWindows. This should get some specific factory pattern
        private const string BaseBoardProductRegistryValue = @"SYSTEM\HardwareConfig\Current\BaseBoardProduct";
        private const string RaspberryPi2Product = "Raspberry Pi 2";
        private const string RaspberryPi3Product = "Raspberry Pi 3";
        private const string HummingBoardProduct = "HummingBoard-Edge";

        private readonly PinNumberingScheme _defaultNumberingScheme;
        private readonly object _pinReservationsLock;
        private readonly Dictionary<int, List<PinReservation>> _pinReservations;
        private bool _initialized;
        private bool _disposed;

        /// <summary>
        /// Constructs a board instance with the given default numbering scheme.
        /// All methods will use the given numbering scheme, unless another scheme is explicitly given in a call.
        /// </summary>
        /// <param name="defaultNumberingScheme">Default numbering scheme for the board.</param>
        /// <remarks>
        /// The constructor will never throw an exception. Call <see cref="Initialize"/> to initialize the hardware and check
        /// whether an instance can actually run on the current hardware.
        /// </remarks>
        protected Board(PinNumberingScheme defaultNumberingScheme)
        {
            _defaultNumberingScheme = defaultNumberingScheme;
            _pinReservations = new Dictionary<int, List<PinReservation>>();
            _pinReservationsLock = new object();
            _initialized = false;
            _disposed = false;
        }

        /// <summary>
        /// (Temporary) Register to this event to receive log messages.
        /// </summary>
        public event Action<string, Exception?>? LogMessages;

        /// <summary>
        /// True if the board instance is initialized
        /// </summary>
        protected bool Initialized => _initialized;

        /// <summary>
        /// True if this instance is disposed.
        /// Any attempt to use it after this becomes true results in undefined behavior.
        /// </summary>
        protected bool Disposed
        {
            get
            {
                return _disposed;
            }
        }

        /// <summary>
        /// The default pin numbering scheme
        /// </summary>
        public PinNumberingScheme DefaultPinNumberingScheme
        {
            get
            {
                return _defaultNumberingScheme;
            }
        }

        /// <summary>
        /// (Temporary) Log a message
        /// </summary>
        protected void Log(string message, Exception? exception = null)
        {
            LogMessages?.Invoke(message, null);
        }

        /// <summary>
        /// Converts pin numbers from one numbering scheme to another.
        /// </summary>
        /// <param name="pinNumber">Pin number to convert</param>
        /// <param name="inputScheme">The numbering scheme of the input pin</param>
        /// <param name="outputScheme">The desired numbering scheme</param>
        /// <returns>The converted pin number</returns>
        public abstract int ConvertPinNumber(int pinNumber, PinNumberingScheme inputScheme, PinNumberingScheme outputScheme);

        /// <summary>
        /// Reserves a pin for a specific usage. This is done automatically if a known interface (i.e. GpioController) is
        /// used to open the pin, but may be used to block a pin explicitly, i.e. for UART.
        /// </summary>
        /// <param name="pinNumber">The pin number, in the boards default numbering scheme</param>
        /// <param name="usage">Intended usage of the pin</param>
        /// <param name="owner">Class that owns the pin (use "this")</param>
        /// <exception cref="InvalidOperationException">The pin is already reserved</exception>
        public virtual void ReservePin(int pinNumber, PinUsage usage, object owner)
        {
            if (!_initialized)
            {
                Initialize();
            }

            int logicalPin = ConvertPinNumber(pinNumber, DefaultPinNumberingScheme, PinNumberingScheme.Logical);

            lock (_pinReservationsLock)
            {
                if (!UsageCanSharePins(usage))
                {
                    if (_pinReservations.TryGetValue(logicalPin, out List<PinReservation>? reservations))
                    {
                        PinReservation reservation = reservations.First();
                        throw new InvalidOperationException($"Pin {pinNumber} has already been reserved for {reservation.Usage} by class {reservation.Owner}.");
                    }

                    PinReservation rsv = new PinReservation(logicalPin, usage, owner);
                    _pinReservations.Add(logicalPin, new List<PinReservation>() { rsv });
                }
                else
                {
                    if (_pinReservations.TryGetValue(logicalPin, out List<PinReservation>? reservations))
                    {
                        PinReservation reservation = reservations.First();
                        if (reservation.Usage != usage)
                        {
                            throw new InvalidOperationException($"Pin {pinNumber} has already been reserved for {reservation.Usage} by class {reservation.Owner}.");
                        }
                    }
                    else
                    {
                        reservations = new List<PinReservation>();
                        _pinReservations.Add(logicalPin, reservations);
                    }

                    PinReservation rsv = new PinReservation(logicalPin, usage, owner);
                    reservations.Add(rsv);
                }
            }

            ActivatePinMode(logicalPin, usage);
        }

        /// <summary>
        /// Removes the reservation for a pin.
        /// See <see cref="ReservePin"/> for details.
        /// </summary>
        /// <param name="pinNumber">The pin number to free, in the numbering scheme of the board</param>
        /// <param name="usage">The current pin usage</param>
        /// <param name="owner">The current pin owner</param>
        /// <exception cref="InvalidOperationException">The pin is not reserved, or the owner is not correct</exception>
        public virtual void ReleasePin(int pinNumber, PinUsage usage, object owner)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Cannot release a pin if board is not initialized.");
            }

            int logicalPin = ConvertPinNumber(pinNumber, DefaultPinNumberingScheme, PinNumberingScheme.Logical);

            lock (_pinReservationsLock)
            {
                if (_pinReservations.TryGetValue(logicalPin, out List<PinReservation>? reservations))
                {
                    if (!UsageCanSharePins(usage))
                    {
                        PinReservation reservation = reservations.Single();
                        if (reservation.Owner != owner || reservation.Usage != usage)
                        {
                            throw new InvalidOperationException($"Cannot release Pin {pinNumber}, because you are not the owner or the usage is wrong. Class {reservation.Owner} has reserved the Pin for {reservation.Usage}");
                        }

                        _pinReservations.Remove(logicalPin);
                    }
                    else
                    {
                        PinReservation? reservation = reservations.FirstOrDefault(x => x.Owner == owner);
                        if (reservation == null)
                        {
                            throw new InvalidOperationException($"Cannot release Pin {pinNumber}, because you are not a valid owner.");
                        }

                        if (reservation.Usage != usage)
                        {
                            throw new InvalidOperationException($"Cannot release Pin {pinNumber}, because you are not the owner or the usage is wrong. Class {reservation.Owner} has reserved the Pin for {reservation.Usage}");
                        }

                        reservations.Remove(reservation);
                        if (reservations.Count == 0)
                        {
                            _pinReservations.Remove(logicalPin);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Cannot release Pin {pinNumber}, because it is not reserved.");
                }
            }
        }

        /// <summary>
        /// Returns the current usage of a pin
        /// </summary>
        /// <param name="pinNumber">Pin number in the current numbering scheme</param>
        /// <returns>The current usage of a pin</returns>
        public abstract PinUsage DetermineCurrentPinUsage(int pinNumber);

        /// <summary>
        /// Override this method if something special needs to be done to use the pin for the given device.
        /// Many devices support multiple functions per Pin, but not at the same time, so that some kind of
        /// multiplexer needs to be set accordingly.
        /// </summary>
        /// <param name="pinNumber">The logical pin number to use.</param>
        /// <param name="usage">The intended usage</param>
        protected virtual void ActivatePinMode(int pinNumber, PinUsage usage)
        {
        }

        /// <summary>
        /// Initialize the board and test whether it works on the current hardware.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">The required hardware cannot be found</exception>
        public virtual void Initialize()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(ToString());
            }

            _initialized = true;
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private sealed class PinReservation
        {
            public PinReservation(int pin, PinUsage usage, object owner)
            {
                Pin = pin;
                Usage = usage;
                Owner = owner;
            }

            public int Pin { get; }
            public PinUsage Usage { get; }

            /// <summary>
            /// Component that owns the pin (used mainly for debugging)
            /// </summary>
            public object Owner { get; }
        }

        /// <summary>
        /// Return an instance of a <see cref="GpioController"/> for the current board
        /// </summary>
        /// <returns>An instance of a GpioController. The controller used pin management to prevent reusing the same pin for different purposes
        /// (or for purposes for which it is not suitable)</returns>
        public virtual GpioController CreateGpioController()
        {
            return CreateGpioController(DefaultPinNumberingScheme);
        }

        /// <summary>
        /// Return an instance of a <see cref="GpioController"/> for the current board, specifying the numbering scheme to use
        /// </summary>
        /// <returns>An instance of a GpioController. The controller used pin management to prevent reusing the same pin for different purposes
        /// (or for purposes for which it is not suitable)</returns>
        public virtual GpioController CreateGpioController(PinNumberingScheme pinNumberingScheme)
        {
            GpioDriver? driver = TryCreateBestGpioDriver();
            if (driver == null)
            {
                // Should be resolved in overloads of this class
                throw new NotSupportedException("No Gpio Driver for this board could be found.");
            }

            return new ManagedGpioController(this, pinNumberingScheme, driver);
        }

        /// <summary>
        /// Tries to create the best possible GPIO driver for this hardware.
        /// </summary>
        /// <returns>An instance to the optimal Gpio Driver for this board, or null if none was found</returns>
        /// <remarks>The base implementation will never return null, but create a dummy instance instead</remarks>
        protected virtual GpioDriver? TryCreateBestGpioDriver()
        {
            GpioDriver? driver = null;
            try
            {
                driver = GetBestDriverForBoard();
            }
            catch (Exception x) when (x is PlatformNotSupportedException || x is NotSupportedException) // That would be serious
            {
            }

            if (driver == null)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    driver = new KeyboardGpioDriver();
                }
                else
                {
                    driver = new DummyGpioDriver();
                }
            }

            return driver;
        }

        /// <summary>
        /// Tries to create the GPIO driver that best matches the current hardware
        /// </summary>
        /// <returns>An instance of a GpioDriver that best matches the current hardware</returns>
        /// <exception cref="PlatformNotSupportedException">No matching driver could be found</exception>
        private static GpioDriver GetBestDriverForBoard()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return GetBestDriverForBoardOnWindows();
            }
            else
            {
                return GetBestDriverForBoardOnLinux();
            }
        }

        /// <summary>
        /// Attempt to get the best applicable driver for the board the program is executing on.
        /// </summary>
        /// <returns>A driver that works with the board the program is executing on.</returns>
        private static GpioDriver GetBestDriverForBoardOnLinux()
        {
            try
            {
                // Because we can't construct the internal driver here (and the identification isn't public either), we'll have to go trough the exception
                return new RaspberryPi3Driver();
            }
            catch (Exception x) when (x is NotSupportedException || x is PlatformNotSupportedException)
            {
            }

            return UnixDriver.Create();
        }

        /// <summary>
        /// Attempt to get the best applicable driver for the board the program is executing on.
        /// </summary>
        /// <returns>A driver that works with the board the program is executing on.</returns>
        /// <remarks>
        ///     This really feels like it needs a driver-based pattern, where each driver exposes a static method:
        ///     public static bool IsSpecificToCurrentEnvironment { get; }
        ///     The GpioController could use reflection to find all GpioDriver-derived classes and call this
        ///     static method to determine if the driver considers itself to be the best match for the environment.
        /// (Implementation copied from <see cref="GpioController"/>, where it is private)
        /// </remarks>
        private static GpioDriver GetBestDriverForBoardOnWindows()
        {
#pragma warning disable CA1416 // Registry.LocalMachine is only supported on Windows, but we will only hit this method if we are on Windows.
            string? baseBoardProduct = Registry.LocalMachine.GetValue(BaseBoardProductRegistryValue, string.Empty)?.ToString();
#pragma warning restore CA1416

            if (baseBoardProduct is null)
            {
                throw new Exception("Single board computer type cannot be detected.");
            }

            if (baseBoardProduct == RaspberryPi3Product || baseBoardProduct.StartsWith($"{RaspberryPi3Product} ") ||
                baseBoardProduct == RaspberryPi2Product || baseBoardProduct.StartsWith($"{RaspberryPi2Product} "))
            {
                return new RaspberryPi3Driver();
            }

            if (baseBoardProduct == HummingBoardProduct || baseBoardProduct.StartsWith($"{HummingBoardProduct} "))
            {
                return new HummingBoardDriver();
            }

            // Default for Windows IoT Core on a non-specific device
            return new Windows10Driver();
        }

        /// <summary>
        /// This method shall be overriden by clients, providing just the basic interface to an I2cDevice.
        /// </summary>
        /// <param name="connectionSettings">I2C Connection settings</param>
        /// <param name="pinAssignment">Pins assigned to the device</param>
        /// <returns>An I2cDevice connection</returns>
        protected abstract I2cDevice CreateSimpleI2cDevice(I2cConnectionSettings connectionSettings, int[] pinAssignment);

        /// <summary>
        /// Create an I2C device instance
        /// </summary>
        /// <param name="connectionSettings">Connection parameters (contains I2C address and bus number)</param>
        /// <param name="pinAssignment">The set of pins to use for I2C. The parameter can be null if the hardware requires a fixed mapping from
        /// pins to I2C for the given bus.</param>
        /// <param name="pinNumberingScheme">The numbering scheme in which the <paramref name="pinAssignment"/> is given</param>
        /// <returns>An I2C device instance</returns>
        public I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings, int[] pinAssignment, PinNumberingScheme pinNumberingScheme)
        {
            if (pinAssignment == null || pinAssignment.Length != 2)
            {
                throw new ArgumentException($"Invalid argument. Must provide exactly two pins for I2C", nameof(pinAssignment));
            }

            return new I2cDeviceManager(this, connectionSettings, RemapPins(pinAssignment, pinNumberingScheme), CreateSimpleI2cDevice);
        }

        /// <summary>
        /// Create an I2C device instance on a default bus.
        /// </summary>
        /// <param name="connectionSettings">Connection parameters (contains I2C address and bus number)</param>
        /// <returns>An I2C device instance</returns>
        /// <remarks>This method can only be used for bus numbers where the corresponding pins are hardwired
        /// (i.e. bus 0 and 1 on the Raspi always use pins 0/1 and 2/3)</remarks>
        public I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings)
        {
            // Returns logical pin numbers for the selected bus (or an exception if using a bus number > 1, because that
            // requires specifying the pins)
            int[] pinAssignment = GetDefaultPinAssignmentForI2c(connectionSettings);
            return CreateI2cDevice(connectionSettings, pinAssignment, PinNumberingScheme.Logical);
        }

        /// <summary>
        /// Create an SPI device instance
        /// </summary>
        /// <param name="connectionSettings">Connection parameters (contains bus number and CS pin number)</param>
        /// <param name="pinAssignment">The set of pins to use for SPI. The parameter can be null if the hardware requires a fixed mapping from
        /// pins to SPI for the given bus.</param>
        /// <param name="pinNumberingScheme">The numbering scheme in which the <paramref name="pinAssignment"/> is given</param>
        /// <returns>An SPI device instance</returns>
        public SpiDevice CreateSpiDevice(SpiConnectionSettings connectionSettings, int[] pinAssignment, PinNumberingScheme pinNumberingScheme)
        {
            if (pinAssignment == null)
            {
                throw new ArgumentNullException(nameof(pinAssignment));
            }

            if (pinAssignment.Length != 3 && pinAssignment.Length != 4)
            {
                throw new ArgumentException($"Invalid argument. Must provide three or four pins for SPI", nameof(pinAssignment));
            }

            return new SpiDeviceManager(this, connectionSettings, RemapPins(pinAssignment, pinNumberingScheme), CreateSimpleSpiDevice);
        }

        /// <summary>
        /// Create an SPI device instance
        /// </summary>
        /// /// <param name="connectionSettings">Connection parameters (contains SPI CS pin number and bus number)</param>
        /// <returns>An SPI device instance</returns>
        /// <remarks>This method can only be used for bus numbers where the corresponding pins are hardwired</remarks>
        public SpiDevice CreateSpiDevice(SpiConnectionSettings connectionSettings)
        {
            // Returns logical pin numbers for the selected bus (or an exception if using a bus number > 1, because that
            // requires specifying the pins)
            int[] pinAssignment = GetDefaultPinAssignmentForSpi(connectionSettings);
            return CreateSpiDevice(connectionSettings, pinAssignment, PinNumberingScheme.Logical);
        }

        /// <summary>
        /// Overriden by derived implementations to create the base SPI device.
        /// </summary>
        protected abstract SpiDevice CreateSimpleSpiDevice(SpiConnectionSettings settings, int[] pins);

        /// <summary>
        /// Overriden by derived implementations to provide the PWM device
        /// </summary>
        protected abstract PwmChannel CreateSimplePwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage);

        /// <summary>
        /// Creates a PWM channel
        /// </summary>
        /// <param name="chip">Chip number. Usually 0</param>
        /// <param name="channel">Channel on the given chip</param>
        /// <param name="frequency">Initial frequency</param>
        /// <param name="dutyCyclePercentage">Initial duty cycle</param>
        /// <param name="pin">The pin number for the pwm channel. Used if not hardwired (i.e. on the Raspi, it is possible to use different pins for the same PWM channel)</param>
        /// <param name="pinNumberingScheme">The pin numbering scheme for the pin</param>
        /// <returns>A pwm channel instance</returns>
        public PwmChannel CreatePwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage,
            int pin, PinNumberingScheme pinNumberingScheme)
        {
            return new PwmChannelManager(this, pin, chip, channel, frequency, dutyCyclePercentage, CreateSimplePwmChannel);
        }

        /// <summary>
        /// Creates a PWM channel for the default pin assignment
        /// </summary>
        /// <param name="chip">Chip number. Usually 0</param>
        /// <param name="channel">Channel on the given chip</param>
        /// <param name="frequency">Initial frequency</param>
        /// <param name="dutyCyclePercentage">Initial duty cycle</param>
        /// <returns>A pwm channel instance</returns>
        public PwmChannel CreatePwmChannel(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCyclePercentage = 0.5)
        {
            int pin = GetDefaultPinAssignmentForPwm(chip, channel);
            return CreatePwmChannel(chip, channel, frequency, dutyCyclePercentage, RemapPin(pin, DefaultPinNumberingScheme), PinNumberingScheme.Logical);
        }

        /// <summary>
        /// Overriden by derived class. Provides the default pin for a given channel.
        /// </summary>
        /// <param name="chip">Chip number</param>
        /// <param name="channel">Channel number</param>
        /// <returns>The default pin for the given PWM channel</returns>
        public abstract int GetDefaultPinAssignmentForPwm(int chip, int channel);

        /// <summary>
        /// Gets the board-specific hardware mode for a particular pin and pin usage (i.e. the different ALTn modes on the raspberry pi)
        /// </summary>
        /// <param name="pinNumber">Pin number to use</param>
        /// <param name="usage">Requested usage</param>
        /// <param name="pinNumberingScheme">Pin numbering scheme for the pin provided (logical or physical)</param>
        /// <param name="bus">Optional bus argument, for SPI and I2C pins</param>
        /// <returns>
        /// A hardware-dependent instance of <see cref="AlternatePinMode"/> describing the mode the pin is in.</returns>
        public abstract AlternatePinMode GetHardwareModeForPinUsage(int pinNumber, PinUsage usage,
            PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, int bus = 0);

        /// <summary>
        /// Overriden by derived classes: Provides the default pin assignment for the given I2C bus
        /// </summary>
        /// <param name="connectionSettings">Connection settings to check</param>
        /// <returns>The set of pins for the given I2C bus</returns>
        public abstract int[] GetDefaultPinAssignmentForI2c(I2cConnectionSettings connectionSettings);

        /// <summary>
        /// Overriden by derived classes: Provides the default pin assignment for the given SPI bus
        /// </summary>
        /// <param name="connectionSettings">Connection settings to check</param>
        /// <returns>The set of pins for the given SPI bus</returns>
        public abstract int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings);

        /// <summary>
        /// Converts a pin number to the logical scheme
        /// </summary>
        protected int RemapPin(int pin, PinNumberingScheme providedScheme)
        {
            return ConvertPinNumber(pin, providedScheme, PinNumberingScheme.Logical);
        }

        /// <summary>
        /// Converts a set of pins to the logical scheme
        /// </summary>
        protected int[]? RemapPins(int[] pins, PinNumberingScheme providedScheme)
        {
            if (pins == null)
            {
                return null;
            }

            if (providedScheme == PinNumberingScheme.Logical)
            {
                return pins;
            }

            int[] newPins = new int[pins.Length];
            for (int i = 0; i < pins.Length; i++)
            {
                newPins[i] = ConvertPinNumber(pins[i], providedScheme, PinNumberingScheme.Logical);
            }

            return newPins;
        }

        private bool UsageCanSharePins(PinUsage usage)
        {
            switch (usage)
            {
                // Several I2C devices can share the same Pins (because we're allocating devices, not buses)
                case PinUsage.I2c:
                case PinUsage.Spi:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Create an instance of the best possible board abstraction.
        /// </summary>
        /// <param name="defaultNumberingScheme">The default pin numbering scheme the board should use</param>
        /// <returns>A board instance, to be used across the application</returns>
        /// <exception cref="PlatformNotSupportedException">The board could not be identified</exception>
        /// <remarks>The detection concept should be refined, but this requires a public detection api</remarks>
        public static Board Create(PinNumberingScheme defaultNumberingScheme = PinNumberingScheme.Logical)
        {
            Board? board = null;
            try
            {
                board = new RaspberryPiBoard(defaultNumberingScheme);
                board.Initialize();
            }
            catch (Exception x) when ((x is NotSupportedException) || (x is IOException))
            {
                board?.Dispose();
                board = null;
            }

            if (board != null)
            {
                return board;
            }

            try
            {
                board = new GenericBoard(defaultNumberingScheme);
                board.Initialize();
            }
            catch (Exception x) when ((x is NotSupportedException) || (x is IOException))
            {
                board?.Dispose();
                board = null;
            }

            if (board != null)
            {
                return board;
            }

            throw new PlatformNotSupportedException("Could not find a matching board driver for this hardware");
        }
    }
}
