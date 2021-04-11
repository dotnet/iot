using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    internal class ArduinoI2cBus : I2cBus
    {
        private readonly ArduinoBoard _board;
        private readonly int _busId;
        private readonly HashSet<int> _usedAddresses;

        public ArduinoI2cBus(ArduinoBoard board, int busId)
        {
            _board = board;
            _busId = busId;
            _usedAddresses = new HashSet<int>();
        }

        /// <inheritdoc />
        public override I2cDevice CreateDevice(int deviceAddress)
        {
            if (_usedAddresses.Contains(deviceAddress))
            {
                throw new InvalidOperationException($"Device number {deviceAddress} is already in use");
            }

            var device = new ArduinoI2cDevice(_board, this, new I2cConnectionSettings(_busId, deviceAddress));
            _usedAddresses.Add(deviceAddress);
            return device;
        }

        public override void RemoveDevice(int deviceAddress)
        {
            _usedAddresses.Remove(deviceAddress);
        }
    }
}
