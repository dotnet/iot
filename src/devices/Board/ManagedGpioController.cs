// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    /// <summary>
    /// A GPIO Controller instance that manages pin usage
    /// </summary>
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

        protected override int GetLogicalPinNumber(int pinNumber)
        {
            return _board.ConvertPinNumber(pinNumber, NumberingScheme, PinNumberingScheme.Logical);
        }

        private int ConvertToBoardPinNumbering(int controllerNumber)
        {
            return _board.ConvertPinNumber(controllerNumber, NumberingScheme, _board.DefaultPinNumberingScheme);
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
    }
}
