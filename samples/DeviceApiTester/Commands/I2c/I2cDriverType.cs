using System.Device.I2c.Drivers;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.I2c
{
    public enum I2cDriverType
    {
        [ImplementationType(typeof(Windows10I2cDevice))]
        Windows,

        [ImplementationType(typeof(UnixI2cDevice))]
        Unix,
    }
}
