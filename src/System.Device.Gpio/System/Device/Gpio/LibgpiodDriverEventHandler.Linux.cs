// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers
{
    internal sealed class LibGpiodDriverEventHandler : IDisposable
    {
        public event PinChangeEventHandler ValueRising;

        public event PinChangeEventHandler ValueFalling;

        public int PinNumber;

        public SafeLineHandle PinHandle = null;

        public CancellationTokenSource CancellationTokenSource;

        public Task Task { get; set; }

        public LibGpiodDriverEventHandler() { }

        public LibGpiodDriverEventHandler(int pinNumber, CancellationTokenSource cancellationTokenSource) {
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
                CancellationTokenSource.Cancel();
                Task?.Wait();               
                PinHandle?.Dispose();
                PinHandle = null;
            }
            ValueRising = null;
            ValueFalling = null;
        }
    }
}
