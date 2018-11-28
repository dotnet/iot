// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Devices.Enumeration;
using WinI2c = Windows.Devices.I2c;

namespace System.Device.I2c
{
    public class Windows10I2cDevice : I2cDevice
    {
        private readonly I2cConnectionSettings _settings;
        private WinI2c.I2cDevice _winDevice;

        public Windows10I2cDevice(I2cConnectionSettings settings)
        {
            _settings = settings;
            var winSettings = new WinI2c.I2cConnectionSettings(settings.DeviceAddress);

            string deviceSelector = WinI2c.I2cDevice.GetDeviceSelector($"I2C{settings.BusId}");

            DeviceInformationCollection deviceInformationCollection = DeviceInformation.FindAllAsync(deviceSelector).GetResults();
            _winDevice = WinI2c.I2cDevice.FromIdAsync(deviceInformationCollection[0].Id, winSettings).GetResults();
        }

        public override I2cConnectionSettings ConnectionSettings => _settings;

        public override byte ReadByte()
        {
            byte[] buffer = new byte[1];
            _winDevice.Read(buffer);
            return buffer[0];
        }

        public override void Read(Span<byte> buffer)
        {
            byte[] byteArray = new byte[buffer.Length];
            _winDevice.Read(byteArray);
            new Span<byte>(byteArray).CopyTo(buffer);
        }

        public override void WriteByte(byte data)
        {
            _winDevice.Write(new[] { data });
        }

        public override void Write(Span<byte> data)
        {
            _winDevice.Write(data.ToArray());
        }

        public override void Dispose(bool disposing)
        {
            _winDevice?.Dispose();
            _winDevice = null;

            base.Dispose(disposing);
        }
    }
}
