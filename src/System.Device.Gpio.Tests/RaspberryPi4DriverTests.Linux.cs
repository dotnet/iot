using System.Device.Gpio.Drivers;
using System.Threading;
using Xunit;

namespace System.Device.Gpio.Tests
{
    public class RaspberryPi4DriverTests : GpioControllerTestBase
    {
        /// <summary>
        /// Leave this pin open (unconnected) for the tests
        /// </summary>
        private const int OpenPin = 23;

        protected override GpioDriver GetTestDriver()
        {
            return new RaspberryPi4Driver();
        }

        protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;

        [Fact]
        public void InputPullResistorsWork()
        {
            // This is a hack to test whether we're running on a Pi4 without duplicating that detection logic. 
            var bestDriver = GpioController.GetBestDriverForBoard();
            if (!(bestDriver is RaspberryPi4Driver))
            {
                bestDriver.Dispose();
                return;
            }
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), bestDriver))
            {
                controller.OpenPin(OpenPin, PinMode.InputPullUp);
                Thread.Sleep(10);
                Assert.Equal(PinValue.High, controller.Read(OpenPin));
                controller.SetPinMode(OpenPin, PinMode.InputPullDown);
                Assert.Equal(PinValue.Low, controller.Read(OpenPin));
                controller.SetPinMode(OpenPin, PinMode.InputPullUp);
                Assert.Equal(PinValue.High, controller.Read(OpenPin));
            }
        }
    }
}
