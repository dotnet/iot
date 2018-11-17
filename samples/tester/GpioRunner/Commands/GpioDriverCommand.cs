using System.Device.Gpio;
using CommandLine;

namespace GpioRunner
{
    public abstract class GpioDriverCommand : DebuggableCommand
    {
        [Option('s', "scheme", HelpText = "The pin numbering scheme: { Logical | Board }", Required = false, Default = PinNumberingScheme.Logical)]
        public PinNumberingScheme Scheme { get; set; }

        [Option('d', "driver", HelpText = "The GpioDriver to use: { Default | Windows | UnixSysFs | HummingBoard | RPi3 }", Required = false, Default = DriverType.Default)]
        public DriverType Driver { get; set; }

        protected GpioController CreateController()
        {
            GpioDriver gpioDriver = DriverFactory.CreateFromEnum(this.Driver);

            return gpioDriver != null
                ? new GpioController(Scheme, gpioDriver)
                : new GpioController(Scheme);
        }
    }
}
