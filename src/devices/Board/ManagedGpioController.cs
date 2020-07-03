using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Iot.Device.Board
{
    internal class ManagedGpioController : GpioController
    {
        private readonly int[] _pinAssignment;
        private readonly Board _board;
        private readonly GpioDriver _driver;

        public ManagedGpioController(Board board, PinNumberingScheme numberingScheme, GpioDriver driver, int[] pinAssignment)
        : base(numberingScheme, driver)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _pinAssignment = pinAssignment;

            if (pinAssignment != null)
            {
                foreach (var pin in pinAssignment)
                {
                    if (_board.GetHardwareModeForPinUsage(pin, PinUsage.Gpio) != AlternatePinMode.Gpio)
                    {
                        throw new NotSupportedException($"Logical pin {pin} does not support Gpio");
                    }
                }
            }
        }

        public static new GpioDriver GetBestDriverForBoard()
        {
            GpioDriver driver = null;
            try
            {
                driver = GpioController.GetBestDriverForBoard();
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

        protected override int GetLogicalPinNumber(int pinNumber, PinNumberingScheme givenScheme)
        {
            return _board.ConvertPinNumber(pinNumber, givenScheme, PinNumberingScheme.Logical);
        }

        public override void OpenPin(int pinNumber)
        {
            if (_pinAssignment != null)
            {
                if (!_pinAssignment.Contains(pinNumber))
                {
                    throw new InvalidOperationException($"Pin {pinNumber} is not reserved for this GpioController");
                }
            }

            _board.ReservePin(pinNumber, PinUsage.Gpio, this);
            base.OpenPin(pinNumber);
        }

        protected override void ClosePin(int pinNumber, PinNumberingScheme numberingScheme)
        {
            base.ClosePin(pinNumber, numberingScheme);
            _board.ReleasePin(pinNumber, PinUsage.Gpio, this);
        }

        /// <summary>
        /// Returns the currently set "alternate" pin mode, for pins which are multiplexed between different functions
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <returns>The returned pin mode</returns>
        internal new AlternatePinMode GetAlternatePinMode(int pinNumber)
        {
            int mode = base.GetAlternatePinMode(pinNumber);
            if (mode < 0)
            {
                return AlternatePinMode.Gpio;
            }
            else
            {
                return (AlternatePinMode)(mode + 1);
            }
        }

        internal void SetAlternatePinMode(int pinNumber, AlternatePinMode altMode)
        {
            int mode = 0;
            if (altMode < 0)
            {
                throw new ArgumentException("Invalid mode requested", nameof(altMode));
            }

            if (altMode == AlternatePinMode.Gpio)
            {
                // When going back to Gpio, default to input
                mode = -1;
            }
            else
            {
                mode = (int)altMode - 1;
            }

            SetAlternatePinMode(pinNumber, mode);
        }
    }
}
