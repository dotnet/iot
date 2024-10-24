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
        private readonly HashSet<int> _openPins;

        public ManagedGpioController(Board board, GpioDriver driver)
        : base(driver)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _openPins = new HashSet<int>();
        }

        protected override int GetLogicalPinNumber(int pinNumber)
        {
            return pinNumber;
        }

        protected override void OpenPinCore(int pinNumber)
        {
            _board.ReservePin(pinNumber, PinUsage.Gpio, this);
            base.OpenPinCore(pinNumber);
            _openPins.Add(pinNumber);
        }

        protected override void ClosePinCore(int pinNumber)
        {
            base.ClosePinCore(pinNumber);
            _board.ReleasePin(pinNumber, PinUsage.Gpio, this);
            _openPins.Remove(pinNumber);
        }

        public IReadOnlyCollection<int> GetActiveManagedPins()
        {
            return _openPins;
        }

        public override ComponentInformation QueryComponentInformation()
        {
            var self = base.QueryComponentInformation();
            self = self with
            {
                Description = "Managed GPIO Controller"
            };
            return self;
        }
    }
}
