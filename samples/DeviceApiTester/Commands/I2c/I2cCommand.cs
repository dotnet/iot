using System.Device.I2c;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.I2c
{
    public abstract class I2cCommand : DebuggableCommand
    {
        [Option('d', "device", HelpText = "The I2cDevice to use: { Windows | Unix }", Required = true)]
        public I2cDriverType Device { get; set; }

        [Option('b', "bus-id", HelpText = "The bus id the I2C device to connect to", Required = true)]
        public int BusId { get; set; }

        [Option('a', "device-address", HelpText = "The device address for the connection to the I2C device", Required = true)]
        public int DeviceAddress { get; set; }

        protected I2cDevice CreateI2cDevice(I2cConnectionSettings connectionSettings)
        {
            return DriverFactory.CreateFromEnum<I2cDevice, I2cDriverType>(this.Device, connectionSettings);
        }
    }
}
