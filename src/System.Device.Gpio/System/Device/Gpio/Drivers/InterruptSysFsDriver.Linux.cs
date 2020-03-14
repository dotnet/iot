// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// This overrides the SysFsDriver, when only the interrupt callback methods are required.
    /// </summary>
    internal class InterruptSysFsDriver : SysFsDriver
    {
        private GpioDriver _gpioDriver;
        public InterruptSysFsDriver(GpioDriver gpioDriver)
            : base()
        {
            _gpioDriver = gpioDriver;
            StatusUpdateSleepTime = TimeSpan.Zero; // This driver does not need this "magic sleep" as we're directly accessing the hardware registers
        }

        protected internal override PinValue Read(int pinNumber)
        {
            return _gpioDriver.Read(pinNumber);
        }

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            base.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
            // Yield our time slice, so the event handler has time to start.
            // Otherwise, we may miss the first event.
            Thread.Sleep(0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // not our instance
                _gpioDriver = null;
            }

            base.Dispose(disposing);
        }
    }
}
