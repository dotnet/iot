// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler
{
    internal class ArduinoNativeI2cBus : I2cBus
    {
        private readonly ArduinoNativeBoard _board;
        private readonly int _busId;
        private readonly HashSet<int> _usedAddresses;

        public ArduinoNativeI2cBus(ArduinoNativeBoard board, int busId)
        {
            _board = board;
            _busId = busId;
            _usedAddresses = new HashSet<int>();
        }

        public override I2cDevice CreateDevice(int deviceAddress)
        {
            if (_usedAddresses.Contains(deviceAddress))
            {
                throw new InvalidOperationException($"Device number {deviceAddress} is already in use");
            }

            var device = new ArduinoNativeI2cDevice(_board, this, new I2cConnectionSettings(_busId, deviceAddress));
            _usedAddresses.Add(deviceAddress);
            return device;
        }

        public override void RemoveDevice(int deviceAddress)
        {
            _usedAddresses.Remove(deviceAddress);
        }

        public override ComponentInformation QueryComponentInformation()
        {
            var self = new ComponentInformation(this, "Arduino Native I2C Bus Driver");
            self.Properties["BusNo"] = _busId.ToString(CultureInfo.CurrentCulture);

            return self;
        }
    }
}
