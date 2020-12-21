// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.I2c
{
    internal class Windows10I2cBusDevice : Windows10I2cDevice
    {
        public Windows10I2cBus Bus { get; }

        public Windows10I2cBusDevice(Windows10I2cBus bus, I2cConnectionSettings settings)
            : base(settings)
        {
            Bus = bus;
        }

        public void DisposeDevice()
        {
            base.Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            // We do not want to cause errors here on double dispose or when device is removed from the bus and then disposed
            Bus.RemoveDeviceNoCheck(ConnectionSettings.DeviceAddress, this);
        }
    }
}
