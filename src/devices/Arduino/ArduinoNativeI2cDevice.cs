using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public class ArduinoNativeI2cDevice : I2cDevice
    {
        // easier to access from the firmware here than in the structure
        private int _deviceAddress; // do not remove. Used in runtime.

        public ArduinoNativeI2cDevice(I2cConnectionSettings connectionSettings)
        {
            ConnectionSettings = connectionSettings;
            _deviceAddress = connectionSettings.DeviceAddress;
            Init(connectionSettings.BusId, connectionSettings.DeviceAddress);
        }

        public override I2cConnectionSettings ConnectionSettings { get; }

        [ArduinoImplementation(NativeMethod.ArduinoNativeI2cDeviceInit)]
        private void Init(int busId, int deviceAddress)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.ArduinoNativeI2cDeviceReadByte)]
        public override byte ReadByte()
        {
            throw new NotImplementedException();
        }

        public override void Read(Span<byte> buffer)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.ArduinoNativeI2cDeviceWriteByte)]
        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            throw new NotImplementedException();
        }
    }
}
