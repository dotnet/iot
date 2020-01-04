using System.Device.Gpio;

namespace ExplorerHat.Gpio
{
    /// <summary>
    /// Extension methods for <see cref="GpioController"/>
    /// </summary>
    public static class GpioControllerExtensions
    {
        /// <summary>
        /// Ensures pin opening
        /// </summary>
        /// <param name="controller">Instance to extend</param>
        /// <param name="pin">Pin number</param>
        /// <param name="pinMode">Pin opening mode to apply</param>
        public static void EnsureOpenPin(this GpioController controller, int pin, PinMode pinMode)
        {
            if (!controller.IsPinOpen(pin) || controller.GetPinMode(pin) != pinMode)
            {
                if (controller.IsPinOpen(pin))
                {
                    controller.ClosePin(pin);
                }

                controller.OpenPin(pin, pinMode);
            }
        }
    }
}
