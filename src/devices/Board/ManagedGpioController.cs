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
        private readonly Board _board;
        private readonly GpioDriver _driver;

        public ManagedGpioController(Board board, PinNumberingScheme numberingScheme, GpioDriver driver)
        : base(numberingScheme, driver)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
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
            _board.ReservePin(pinNumber, PinUsage.Gpio, this);
            base.OpenPin(pinNumber);
        }

        protected override void ClosePin(int pinNumber, PinNumberingScheme numberingScheme)
        {
            base.ClosePin(pinNumber, numberingScheme);
            _board.ReleasePin(pinNumber, PinUsage.Gpio, this);
        }
    }
}
