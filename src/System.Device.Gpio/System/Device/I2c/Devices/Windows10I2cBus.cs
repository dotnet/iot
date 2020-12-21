// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Device.I2c
{
    internal class Windows10I2cBus : I2cBus
    {
        public int BusId { get; }
        private Dictionary<int, Windows10I2cBusDevice> _devices = new Dictionary<int, Windows10I2cBusDevice>();

        public Windows10I2cBus(int busId)
        {
            BusId = busId;
        }

        public override I2cDevice CreateDevice(int deviceAddress)
        {
            if (_devices.ContainsKey(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} is already open.", nameof(deviceAddress));
            }

            Windows10I2cBusDevice device = new Windows10I2cBusDevice(this, new I2cConnectionSettings(BusId, deviceAddress));
            _devices[deviceAddress] = device;

            return device;
        }

        public override void RemoveDevice(int deviceAddress)
        {
            if (!_devices.TryGetValue(deviceAddress, out Windows10I2cBusDevice? device))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} was not open.", nameof(deviceAddress));
            }

            RemoveDeviceNoCheck(deviceAddress, device);
        }

        internal void RemoveDeviceNoCheck(int deviceAddress, Windows10I2cBusDevice device)
        {
            _devices?.Remove(deviceAddress);
            device.DisposeDevice();
        }

        protected override void Dispose(bool disposing)
        {
            if (_devices != null)
            {
                foreach (var kv in _devices)
                {
                    kv.Value.DisposeDevice();
                }

                _devices = null!;
            }

            base.Dispose(disposing);
        }
    }
}
