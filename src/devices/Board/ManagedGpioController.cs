// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Iot.Device.Board
{
    /// <summary>
    /// A GPIO Controller instance that manages pin usage
    /// </summary>
    internal class ManagedGpioController : GpioController, IDeviceManager
    {
        private readonly Board _board;
        private readonly GpioDriver _driver;

        public ManagedGpioController(Board board, GpioDriver driver)
        : base(PinNumberingScheme.Logical, driver)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        protected override int GetLogicalPinNumber(int pinNumber)
        {
            return pinNumber;
        }

        protected override void OpenPinCore(int pinNumber)
        {
            _board.ReservePin(pinNumber, PinUsage.Gpio, this);
            base.OpenPinCore(pinNumber);
        }

        protected override void ClosePinCore(int pinNumber)
        {
            base.ClosePinCore(pinNumber);
            _board.ReleasePin(pinNumber, PinUsage.Gpio, this);
        }

        public IReadOnlyCollection<int> GetActiveManagedPins()
        {
            return OpenPins.Select(x => x.PinNumber).ToList();
        }

        public override ComponentInformation QueryComponentInformation()
        {
            var self = base.QueryComponentInformation();
            self = self with
            {
                Description = "Managed GPIO Controller"
            };
            self.Properties["ManagedPins"] = string.Join(", ", GetActiveManagedPins());
            return self;
        }
    }
}
