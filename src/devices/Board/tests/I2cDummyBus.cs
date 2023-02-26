// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.Globalization;
using System.Text;
using Board.Tests;

namespace Iot.Device.Board.Tests
{
    internal class I2cDummyBus : I2cBus
    {
        private readonly int _busNumber;

        public I2cDummyBus(int busNumber)
        {
            _busNumber = busNumber;
        }

        public override I2cDevice CreateDevice(int deviceAddress)
        {
            return new I2cDummyDevice(new I2cConnectionSettings(_busNumber, deviceAddress));
        }

        public override void RemoveDevice(int deviceAddress)
        {
        }

        public override ComponentInformation QueryComponentInformation()
        {
            var self = new ComponentInformation(this, "Dummy I2C Bus");
            self.Properties["BusNo"] = _busNumber.ToString(CultureInfo.InvariantCulture);
            return self;
        }
    }
}
