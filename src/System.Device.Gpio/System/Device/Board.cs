using System;
using System.Collections.Generic;
using System.Device.Analog;
using System.Device.Boards;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;
using System.IO;
using System.Text;

namespace System.Device
{
    public abstract class Board : MarshalByRefObject, IDisposable
    {
        private readonly PinNumberingScheme _defaultNumberingScheme;
        private readonly object _pinReservationsLock;
        private readonly Dictionary<int, PinReservation> _pinReservations;
        private bool _initialized;
        private bool _disposed;

        protected Board(PinNumberingScheme defaultNumberingScheme)
        {
            _defaultNumberingScheme = defaultNumberingScheme;
            _pinReservations = new Dictionary<int, PinReservation>();
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
        /// Converts pin numbers in the active <see cref="PinNumberingScheme"/> to logical pin numbers.
        /// Does nothing if <see cref="PinNumberingScheme"/> is logical
        /// </summary>
        /// <param name="pinNumber">Pin numbers</param>
        /// <returns>The logical pin number</returns>
        public abstract int ConvertPinNumberToLogicalNumberingScheme(int pinNumber);

        /// <summary>
        /// Converts logical pin numbers to the active numbering scheme.
        /// This is the opposite of <see cref="ConvertPinNumberToLogicalNumberingScheme"/>.
        /// </summary>
        /// <param name="pinNumber">Logical pin number</param>
        /// <returns>The pin number in the given pin numbering scheme</returns>
        public abstract int ConvertLogicalNumberingSchemeToPinNumber(int pinNumber);

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

            int logicalPin = ConvertPinNumberToLogicalNumberingScheme(pinNumber);
            lock (_pinReservationsLock)
            {
                if (_pinReservations.TryGetValue(logicalPin, out var reservation))
                {
                    throw new InvalidOperationException($"Pin {pinNumber} has already been reserved for {reservation.Usage} by class {reservation.Owner}.");
                }

                PinReservation rsv = new PinReservation(logicalPin, usage, owner);
                _pinReservations.Add(logicalPin, rsv);
            }

            ActivatePinMode(logicalPin, usage);
        }

        public virtual void ReleasePin(int pinNumber, PinUsage usage, object owner)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Cannot release a pin if board is not initialized.");
            }

            int logicalPin = ConvertPinNumberToLogicalNumberingScheme(pinNumber);
            lock (_pinReservationsLock)
            {
                if (_pinReservations.TryGetValue(logicalPin, out var reservation))
                {
                    if (reservation.Owner != owner || reservation.Usage != usage)
                    {
                        throw new InvalidOperationException($"Cannot release Pin {pinNumber}, because you are not the owner or the usage is wrong. Class {reservation.Owner} has reserved the Pin for {reservation.Usage}");
                    }

                    _pinReservations.Remove(logicalPin);
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

        public abstract GpioController CreateGpioController(PinNumberingScheme pinNumberingScheme);
        public abstract I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings);

        public abstract SpiDevice CreateSpiDevice(SpiConnectionSettings settings);

        public abstract PwmChannel CreatePwmChannel(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCyclePercentage = 0.5);

        public abstract AnalogController CreateAnalogController(int chip);

        public static Board DetermineOptimalBoardForHardware(PinNumberingScheme defaultNumberingScheme = PinNumberingScheme.Logical)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                Board board = null;
                try
                {
                    board = new RaspberryPiBoard(defaultNumberingScheme, true);
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
                    board = new UnixBoard(defaultNumberingScheme, true);
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
            }
            else
            {
                // TODO: Create WindowsBoard()
            }

            throw new PlatformNotSupportedException("Could not find a matching board driver for this hardware");
        }
    }
}
