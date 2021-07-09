using System;
using System.Collections.Generic;
using System.Device.I2c;
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
    }
}
