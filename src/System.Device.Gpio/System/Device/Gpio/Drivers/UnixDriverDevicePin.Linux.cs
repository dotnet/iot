// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio.Drivers
{
    internal sealed class UnixDriverDevicePin
    {
        public event PinChangeEventHandler ValueRising;
        public event PinChangeEventHandler ValueFalling;

        public void OnPinValueChanged(PinValueChangedEventArgs args)
        {
            if (args.ChangeType.HasFlag(PinEventTypes.Rising))
                ValueRising?.Invoke(this, args);
            if (args.ChangeType.HasFlag(PinEventTypes.Falling))
                ValueFalling?.Invoke(this, args);
        }

        public bool IsCallbackListEmpty()
        {
            if (ValueRising == null && ValueFalling == null)
                return true;
            if (ValueRising == null)
            {
                return ValueFalling.GetInvocationList().Length == 0;
            }
            if (ValueFalling == null)
            {
                return ValueRising.GetInvocationList().Length == 0;
            }
            return ValueFalling.GetInvocationList().Length == 0 && ValueRising.GetInvocationList().Length == 0;
        }
    }
}
