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

        private readonly object _pinReservationsLock;
        private readonly Dictionary<int, List<PinReservation>> _pinReservations;
        private readonly Dictionary<int, I2cBusManager> _i2cBuses;
        private readonly List<IDeviceManager> _managers;
        private bool _initialized;
        private bool _disposed;
        private Dictionary<int, PinUsage> _knownUsages;

        /// <summary>
        /// Constructs a board instance with the default numbering scheme.
        /// </summary>
        /// <remarks>
        /// The constructor will never throw an exception. Call any other method to initialize the hardware and check
        /// whether an instance can actually run on the current hardware.
        /// </remarks>
        protected Board()
        {
            _pinReservations = new Dictionary<int, List<PinReservation>>();
            _i2cBuses = new Dictionary<int, I2cBusManager>();
            _managers = new List<IDeviceManager>();
            _knownUsages = new Dictionary<int, PinUsage>();
            _pinReservationsLock = new object();
            _initialized = false;
            _disposed = false;
        }

        /// <summary>
        /// True if the board instance is initialized
        /// </summary>
        protected bool Initialized => _initialized;

        /// <summary>
        /// True if this instance is disposed.
        /// Any attempt to use it after this becomes true results in undefined behavior.
        /// </summary>
        protected bool Disposed => _disposed;

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

            lock (_pinReservationsLock)
            {
                if (!UsageCanSharePins(usage))
                {
                    if (_pinReservations.TryGetValue(pinNumber, out List<PinReservation>? reservations))
                    {
                        PinReservation reservation = reservations.First();
                        throw new InvalidOperationException($"Pin {pinNumber} has already been reserved for {reservation.Usage} by class {reservation.Owner}.");
                    }

                    PinReservation rsv = new PinReservation(pinNumber, usage, owner);
                    _pinReservations.Add(pinNumber, new List<PinReservation>() { rsv });
                }
                else
                {
                    if (_pinReservations.TryGetValue(pinNumber, out List<PinReservation>? reservations))
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
                        _pinReservations.Add(pinNumber, reservations);
                    }

                    PinReservation rsv = new PinReservation(pinNumber, usage, owner);
                    reservations.Add(rsv);
                }

                if (owner is IDeviceManager manager)
                {
                    AddManager(manager);
                }
            }

            ActivatePinMode(pinNumber, usage);
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

            lock (_pinReservationsLock)
            {
                if (_pinReservations.TryGetValue(pinNumber, out List<PinReservation>? reservations))
                {
                    if (!UsageCanSharePins(usage))
                    {
                        PinReservation reservation = reservations.Single();
                        if (reservation.Owner != owner || reservation.Usage != usage)
                        {
                            throw new InvalidOperationException($"Cannot release Pin {pinNumber}, because you are not the owner or the usage is wrong. Class {reservation.Owner} has reserved the Pin for {reservation.Usage}");
                        }

                        _pinReservations.Remove(pinNumber);
                        ClearOpenManagers();
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
                            _pinReservations.Remove(pinNumber);
                            ClearOpenManagers();
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
        /// Override this method if something special needs to be done to use the pin for the given device.
        /// Many devices support multiple functions per Pin, but not at the same time, so that some kind of
        /// multiplexer needs to be set accordingly.
        /// </summary>
        /// <param name="pinNumber">The logical pin number to use.</param>
        /// <param name="usage">The intended usage</param>
        protected virtual void ActivatePinMode(int pinNumber, PinUsage usage)
        {
            _knownUsages[pinNumber] = usage;
        }

        /// <summary>
        /// Returns the current usage of a pin
        /// </summary>
        /// <param name="pinNumber">Pin number in the current numbering scheme</param>
        /// <returns>The current usage of a pin</returns>
        /// <remarks>Note for implementations: An implementation of this method shall try to query the hardware if the cache reports <see cref="PinUsage.Unknown"/>.</remarks>
        public virtual PinUsage DetermineCurrentPinUsage(int pinNumber)
        {
            PinUsage usage;
            if (_knownUsages.TryGetValue(pinNumber, out usage))
            {
                return usage;
            }

            // The generic board only knows the usage if it has been explicitly set before
            return PinUsage.Unknown;
        }

        /// <summary>
        /// Initialize the board and test whether it works on the current hardware.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">The required hardware cannot be found</exception>
        protected virtual void Initialize()
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
            lock (_pinReservationsLock)
            {
                foreach (var bus in _i2cBuses)
                {
                    bus.Value.Dispose();
                }

                foreach (var bus in _managers.ToList())
                {
                    bus.Dispose();
                }

                _i2cBuses.Clear();
                _managers.Clear();
            }

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
        /// <returns>An instance of a GpioController. The controller uses pin management to prevent reusing the same pin for different purposes
        /// (or for purposes for which it is not suitable)</returns>
        /// <exception cref="NotSupportedException">Rare: No GPIO Controller was found for the current hardware. The default implementation will return
        /// a simulation interface if no hardware is available.</exception>
        /// <remarks>
        /// Derived classes should not normally override this method, but instead <see cref="TryCreateBestGpioDriver"/>.
        /// </remarks>
        public virtual GpioController CreateGpioController()
        {
            Initialize();

            GpioDriver? driver = TryCreateBestGpioDriver();
            if (driver == null)
            {
                // Should be resolved in overloads of this class
                throw new NotSupportedException("No Gpio Driver for this board could be found.");
            }

            return AddManager(new ManagedGpioController(this, driver));
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
            catch (Exception x) when (x is PlatformNotSupportedException || x is NotSupportedException) // Anything else would be serious
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

            // Default for Windows IoT Core on a non-specific device
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Create an I2C bus instance or return the existing instance for this bus
        /// </summary>
        /// <param name="busNumber">I2C bus number to create</param>
        /// <param name="pinAssignment">The set of pins to use for I2C. Can be null if the bus already exists</param>
        /// <returns>An I2C bus instance</returns>
        public virtual I2cBus CreateOrGetI2cBus(int busNumber, int[]? pinAssignment)
        {
            Initialize();
            if (_i2cBuses.TryGetValue(busNumber, out I2cBusManager? bus))
            {
                return bus;
            }

            if (pinAssignment == null || pinAssignment.Length != 2)
            {
                throw new ArgumentException($"Invalid argument. Must provide exactly two pins for I2C", nameof(pinAssignment));
            }

            bus = CreateI2cBusCore(busNumber, pinAssignment);
            _i2cBuses.Add(busNumber, bus);
            return AddManager(bus);
        }

        /// <summary>
        /// Create an I2C bus instance or return the existing instance for this bus
        /// </summary>
        /// <param name="busNumber">I2C bus number to create</param>
        /// <returns>An I2C bus instance</returns>
        public I2cBus CreateOrGetI2cBus(int busNumber)
        {
            Initialize();
            if (_i2cBuses.TryGetValue(busNumber, out I2cBusManager? bus))
            {
                return bus;
            }

            int[] pins = GetDefaultPinAssignmentForI2c(busNumber);
            bus = CreateI2cBusCore(busNumber, pins);
            _i2cBuses.Add(busNumber, bus);
            return bus;
        }

        /// <summary>
        /// Create an instance of an I2C bus in a derived class
        /// </summary>
        /// <param name="busNumber">The bus number to create</param>
        /// <param name="pins">The pins that are used for the bus</param>
        /// <returns>An <see cref="I2cBusManager"/> instance managing the bus</returns>
        protected abstract I2cBusManager CreateI2cBusCore(int busNumber, int[]? pins);

        /// <summary>
        /// Creates the default I2C bus for this board or returns the existing bus
        /// </summary>
        /// <returns>The number of the default I2C bus</returns>
        public abstract int GetDefaultI2cBusNumber();

        /// <summary>
        /// Create an I2C device instance on a default bus.
        /// </summary>
        /// <param name="connectionSettings">Connection parameters (contains I2C address and bus number)</param>
        /// <returns>An I2C device instance</returns>
        /// <remarks>This method can only be used for bus numbers where the corresponding pins are hardwired
        /// (i.e. bus 0 and 1 on the Raspi always use pins 0/1 and 2/3)</remarks>
        public I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings)
        {
            Initialize();
            // Returns logical pin numbers for the selected bus (or an exception if using a bus number > 1, because that
            // requires specifying the pins)
            if (_i2cBuses.TryGetValue(connectionSettings.BusId, out I2cBusManager? bus))
            {
                return bus.CreateDevice(connectionSettings.DeviceAddress);
            }

            int[] pinAssignment = GetDefaultPinAssignmentForI2c(connectionSettings.BusId);
            I2cBus newBus = CreateOrGetI2cBus(connectionSettings.BusId, pinAssignment);
            return newBus.CreateDevice(connectionSettings.DeviceAddress);
        }

        /// <summary>
        /// This is called from the buses Dispose method, so do NOT call bus.Dispose here
        /// </summary>
        /// <param name="bus">The bus that's being closed</param>
        /// <returns>True if the bus was removed, false if it didn't exist</returns>
        internal bool RemoveBus(I2cBusManager bus)
        {
            return _i2cBuses.Remove(bus.BusId);
        }

        /// <summary>
        /// Create an SPI device instance
        /// </summary>
        /// <param name="connectionSettings">Connection parameters (contains bus number and CS pin number)</param>
        /// <param name="pinAssignment">The set of pins to use for SPI. The parameter can be null if the hardware requires a fixed mapping from
        /// pins to SPI for the given bus.</param>
        /// <returns>An SPI device instance</returns>
        public SpiDevice CreateSpiDevice(SpiConnectionSettings connectionSettings, int[] pinAssignment)
        {
            Initialize();

            if (pinAssignment == null)
            {
                throw new ArgumentNullException(nameof(pinAssignment));
            }

            if (pinAssignment.Length != 3 && pinAssignment.Length != 4)
            {
                throw new ArgumentException($"Invalid argument. Must provide three or four pins for SPI", nameof(pinAssignment));
            }

            var manager = new SpiDeviceManager(this, connectionSettings, pinAssignment, CreateSimpleSpiDevice);
            return AddManager(manager);
        }

        /// <summary>
        /// Create an SPI device instance
        /// </summary>
        /// /// <param name="connectionSettings">Connection parameters (contains SPI CS pin number and bus number)</param>
        /// <returns>An SPI device instance</returns>
        /// <remarks>This method can only be used for bus numbers where the corresponding pins are hardwired</remarks>
        public SpiDevice CreateSpiDevice(SpiConnectionSettings connectionSettings)
        {
            Initialize();
            // Returns logical pin numbers for the selected bus (or an exception if using a bus number > 1, because that
            // requires specifying the pins)
            int[] pinAssignment = GetDefaultPinAssignmentForSpi(connectionSettings);
            return CreateSpiDevice(connectionSettings, pinAssignment);
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
        /// <returns>A pwm channel instance</returns>
        public PwmChannel CreatePwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage,
            int pin)
        {
            Initialize();
            return AddManager(new PwmChannelManager(this, pin, chip, channel, frequency, dutyCyclePercentage, CreateSimplePwmChannel));
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
            Initialize();
            int pin = GetDefaultPinAssignmentForPwm(chip, channel);
            return CreatePwmChannel(chip, channel, frequency, dutyCyclePercentage, pin);
        }

        /// <summary>
        /// Overriden by derived class. Provides the default pin for a given channel.
        /// </summary>
        /// <param name="chip">Chip number</param>
        /// <param name="channel">Channel number</param>
        /// <returns>The default pin for the given PWM channel</returns>
        public abstract int GetDefaultPinAssignmentForPwm(int chip, int channel);

        /// <summary>
        /// Overriden by derived classes: Provides the default pin assignment for the given I2C bus
        /// </summary>
        /// <param name="busId">Bus Id</param>
        /// <returns>The set of pins for the given I2C bus</returns>
        public abstract int[] GetDefaultPinAssignmentForI2c(int busId);

        /// <summary>
        /// Overriden by derived classes: Provides the default pin assignment for the given SPI bus
        /// </summary>
        /// <param name="connectionSettings">Connection settings to check</param>
        /// <returns>The set of pins for the given SPI bus</returns>
        public abstract int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings);

        private bool UsageCanSharePins(PinUsage usage)
        {
            switch (usage)
            {
                // Several SPI devices can share the same Pins (because we're allocating devices, not buses)
                case PinUsage.Spi:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Create an instance of the best possible board abstraction.
        /// </summary>
        /// <returns>A board instance, to be used across the application</returns>
        /// <exception cref="PlatformNotSupportedException">The board could not be identified</exception>
        /// <remarks>The detection concept should be refined, but this requires a public detection api</remarks>
        public static Board Create()
        {
            Board? board = CreateBoardInternal<RaspberryPiBoard>();

            if (board != null)
            {
                return board;
            }

            board = CreateBoardInternal<GenericBoard>();
            if (board != null)
            {
                return board;
            }

            throw new PlatformNotSupportedException("Could not find a matching board driver for this hardware");
        }

        private static T? CreateBoardInternal<T>()
            where T : Board, new()
        {
            T? board = null;
            try
            {
                board = new T();
                board.Initialize();
            }
            catch (Exception x) when ((x is NotSupportedException) || (x is IOException))
            {
                board?.Dispose();
                board = null;
            }

            return board;
        }

        private void ClearOpenManagers()
        {
            lock (_pinReservationsLock)
            {
                for (int index = 0; index < _managers.Count; index++)
                {
                    IDeviceManager m = _managers[index];
                    var managedPins = m.GetActiveManagedPins();
                    bool isInUse = false;
                    foreach (var pin in managedPins)
                    {
                        if (_pinReservations.ContainsKey(pin))
                        {
                            isInUse = true;
                            break;
                        }
                    }

                    if (!isInUse)
                    {
                        // Don't dispose the manager here - Would typically result in a recursive call to Dispose, as we're closing coming from closing pins
                        _managers.RemoveAt(index);
                        index--;
                    }
                }
            }
        }

        private T AddManager<T>(T manager)
            where T : IDeviceManager
        {
            lock (_pinReservationsLock)
            {
                if (!_managers.Contains(manager))
                {
                    _managers.Add(manager);
                }
            }

            return manager;
        }

        /// <inheritdoc cref="GpioController.QueryComponentInformation"/>
        public virtual ComponentInformation QueryComponentInformation()
        {
            ComponentInformation self = new ComponentInformation(this, "Generic Board");

            var controller = CreateGpioController();

            var controllerInfo = controller.QueryComponentInformation();
            self.AddSubComponent(controllerInfo);

            foreach (var e in _managers)
            {
                self.AddSubComponent(e.QueryComponentInformation());
            }

            return self;
        }
    }
}
