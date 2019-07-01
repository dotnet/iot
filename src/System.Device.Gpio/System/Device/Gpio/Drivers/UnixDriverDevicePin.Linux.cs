// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio.Drivers
{
    internal sealed class UnixDriverDevicePin : IDisposable
    {
        public UnixDriverDevicePin()
        {
            FileDescriptor = -1;
        }

        public event PinChangeEventHandler ValueRising;
        public event PinChangeEventHandler ValueFalling;

        public int FileDescriptor;

        public void OnPinValueChanged(PinValueChangedEventArgs args)
        {
            if (ValueRising != null && args.ChangeType == PinEventTypes.Rising)
            {
                ValueRising?.Invoke(this, args);
            }
            if (ValueFalling != null && args.ChangeType == PinEventTypes.Falling)
            {
                ValueFalling?.Invoke(this, args);
            }
        }

        public bool IsCallbackListEmpty()
        {
            return ValueRising == null && ValueFalling == null;
        }

        public void Dispose()
        {
            if (FileDescriptor != -1)
            {
                Interop.close(FileDescriptor);
                FileDescriptor = -1;
            }
            ValueRising = null;
            ValueFalling = null;
        }
    }
}
