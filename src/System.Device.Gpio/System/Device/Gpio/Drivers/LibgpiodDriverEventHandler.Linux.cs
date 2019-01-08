// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System.Device.Gpio.Drivers
{
    internal sealed class LibgpiodDriverEventHandler : IDisposable
    {
        public event PinChangeEventHandler ValueRising;

        public event PinChangeEventHandler ValueFalling;

        public int PinNumber;

        public SafeLineHandle PinHandle = null;

        public CancellationTokenSource CancellationTokenSource;

        public LibgpiodDriverEventHandler() { }

        public LibgpiodDriverEventHandler(int pinNumber, CancellationTokenSource cancellationTokenSource) {
            PinNumber = pinNumber;
            CancellationTokenSource = cancellationTokenSource;
        }

        public void OnPinValueChanged(PinValueChangedEventArgs args, PinEventTypes detectionOfEventTypes)
        {
            if (detectionOfEventTypes == PinEventTypes.Rising && args.ChangeType == PinEventTypes.Rising)
                ValueRising?.Invoke(this, args);
            if (detectionOfEventTypes == PinEventTypes.Falling && args.ChangeType == PinEventTypes.Falling)
                ValueFalling?.Invoke(this, args);
        }

        public bool IsCallbackListEmpty()
        {
            return ValueRising == null && ValueFalling == null;
        }

        public void Dispose()
        {
            if (PinHandle != null)
            {
                Interop.gpiod_line_release(PinHandle);
                PinHandle.Dispose();
            }
            ValueRising = null;
            ValueFalling = null;
        }
    }
}
