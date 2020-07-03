using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;
using System.IO;
using System.Linq;
using System.Text;

namespace Iot.Device.Board
{
    public abstract class Board : MarshalByRefObject, IDisposable
    {
        private readonly PinNumberingScheme _defaultNumberingScheme;
        private readonly object _pinReservationsLock;
        private readonly Dictionary<int, List<PinReservation>> _pinReservations;
        private bool _initialized;
        private bool _disposed;

        protected Board(PinNumberingScheme defaultNumberingScheme)
        {
            _defaultNumberingScheme = defaultNumberingScheme;
            _pinReservations = new Dictionary<int, List<PinReservation>>();
            _pinReservationsLock = new object();
            _initialized = false;
            _disposed = false;
        }

        ~Board()
        {
            Dispose(false);
        }

        public event Action<string, Exception> LogMessages;

        protected bool Initialized
        {
            get
            {
                return _initialized;
            }
        }

        protected bool Disposed
        {
            get
            {
                return _disposed;
            }
        }

        public PinNumberingScheme DefaultPinNumberingScheme
        {
            get
            {
                return _defaultNumberingScheme;
            }
        }

        protected void Log(string message, Exception exception = null)
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
                    if (_pinReservations.TryGetValue(logicalPin, out List<PinReservation> reservations))
                    {
                        PinReservation reservation = reservations.First();
                        throw new InvalidOperationException($"Pin {pinNumber} has already been reserved for {reservation.Usage} by class {reservation.Owner}.");
                    }

                    PinReservation rsv = new PinReservation(logicalPin, usage, owner);
                    _pinReservations.Add(logicalPin, new List<PinReservation>() { rsv });
                }
                else
                {
                    if (_pinReservations.TryGetValue(logicalPin, out List<PinReservation> reservations))
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

        public virtual void ReleasePin(int pinNumber, PinUsage usage, object owner)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Cannot release a pin if board is not initialized.");
            }

            int logicalPin = ConvertPinNumber(pinNumber, DefaultPinNumberingScheme, PinNumberingScheme.Logical);

            lock (_pinReservationsLock)
            {
                if (_pinReservations.TryGetValue(logicalPin, out List<PinReservation> reservations))
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
                        PinReservation reservation = reservations.FirstOrDefault(x => x.Owner == owner);
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

        public virtual void Initialize()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(ToString());
            }

            _initialized = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }

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

        public virtual GpioController CreateGpioController(int[] pinAssignment = null)
        {
            return CreateGpioController(pinAssignment, DefaultPinNumberingScheme);
        }

        public abstract GpioController CreateGpioController(int[] pinAssignment, PinNumberingScheme pinNumberingScheme);

        /// <summary>
        /// This method shall be overriden by clients, providing just the basic interface to an I2cDevice.
        /// </summary>
        /// <param name="connectionSettings">I2C Connection settings</param>
        /// <param name="pinAssignment">Pins assigned to the device</param>
        /// <returns>An I2cDevice connection</returns>
        protected abstract I2cDevice CreateSimpleI2cDevice(I2cConnectionSettings connectionSettings, int[] pinAssignment);

        public I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings, int[] pinAssignment, PinNumberingScheme pinNumberingScheme)
        {
            if (pinAssignment == null || pinAssignment.Length != 2)
            {
                throw new ArgumentException($"Invalid argument. Must provide exactly two pins for I2C", nameof(pinAssignment));
            }

            return new I2cDeviceManager(this, connectionSettings, RemapPins(pinAssignment, pinNumberingScheme), CreateSimpleI2cDevice);
        }

        public I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings)
        {
            // Returns logical pin numbers for the selected bus (or an exception if using a bus number > 1, because that
            // requires specifying the pins)
            int[] pinAssignment = GetDefaultPinAssignmentForI2c(connectionSettings);
            return CreateI2cDevice(connectionSettings, pinAssignment, PinNumberingScheme.Logical);
        }

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

        public SpiDevice CreateSpiDevice(SpiConnectionSettings connectionSettings)
        {
            // Returns logical pin numbers for the selected bus (or an exception if using a bus number > 1, because that
            // requires specifying the pins)
            int[] pinAssignment = GetDefaultPinAssignmentForSpi(connectionSettings);
            return CreateSpiDevice(connectionSettings, pinAssignment, PinNumberingScheme.Logical);
        }

        protected abstract SpiDevice CreateSimpleSpiDevice(SpiConnectionSettings settings, int[] pins);

        protected abstract PwmChannel CreateSimplePwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage);

        public PwmChannel CreatePwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage,
            int pin, PinNumberingScheme pinNumberingScheme)
        {
            return new PwmChannelManager(this, pin, chip, channel, frequency, dutyCyclePercentage, CreateSimplePwmChannel);
        }

        public PwmChannel CreatePwmChannel(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCyclePercentage = 0.5)
        {
            int pin = GetDefaultPinAssignmentForPwm(chip, channel);
            return CreatePwmChannel(chip, channel, frequency, dutyCyclePercentage, RemapPin(pin, DefaultPinNumberingScheme), PinNumberingScheme.Logical);
        }

        public abstract int GetDefaultPinAssignmentForPwm(int chip, int channel);

        /// <summary>
        /// Gets the board-specific hardware mode for a particular pin and pin usage (i.e. the different ALTn modes on the raspberry pi)
        /// </summary>
        /// <param name="pinNumber">Pin number to use</param>
        /// <param name="usage">Requested usage</param>
        /// <param name="pinNumberingScheme">Pin numbering scheme for the pin provided (logical or physical)</param>
        /// <param name="bus">Optional bus argument, for SPI and I2C pins</param>
        /// <returns>
        /// -2: It is unknown whether this pin can be used for the given usage
        /// -1: Pin does not support the given usage
        /// 0: Pin supports the given usage, no special mode is needed (i.e. digital in/out)
        /// >0: Mode to set (hardware dependent)</returns>
        public abstract AlternatePinMode GetHardwareModeForPinUsage(int pinNumber, PinUsage usage,
            PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, int bus = 0);

        public abstract int[] GetDefaultPinAssignmentForI2c(I2cConnectionSettings connectionSettings);

        public abstract int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings);

        protected int RemapPin(int pin, PinNumberingScheme providedScheme)
        {
            return ConvertPinNumber(pin, providedScheme, PinNumberingScheme.Logical);
        }

        protected int[] RemapPins(int[] pins, PinNumberingScheme providedScheme)
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

        //// Todo separately
        //// public abstract AnalogController CreateAnalogController(int chip);

        public static Board DetermineOptimalBoardForHardware(PinNumberingScheme defaultNumberingScheme = PinNumberingScheme.Logical)
        {
            Board board = null;
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
