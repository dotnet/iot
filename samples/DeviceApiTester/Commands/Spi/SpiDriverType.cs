using System.Device.Spi.Drivers;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Spi
{
    public enum SpiDriverType
    {
        [ImplementationType(typeof(Windows10SpiDevice))]
        Windows,

        [ImplementationType(typeof(UnixSpiDevice))]
        Unix,
    }
}
