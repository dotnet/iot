using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;

/// <summary>
/// Extension methods for GpioController
/// </summary>
public static class GpioControllerExtensions
{
    /// <summary>
    /// Opens a set of pins.
    /// </summary>
    public static void OpenPins(this GpioController controller, PinMode mode, params int[] pins)
    {
        foreach (var pin in pins)
        {
            controller.OpenPin(pin, mode);
        }
    }

    /// <summary>
    /// Opens a pin as PineMode.Output and writes value.
    /// </summary>
    public static void OpenPinAndWrite(this GpioController controller, int pin, PinValue value)
    {
        controller.OpenPin(pin, PinMode.Output);
        controller.Write(pin, value);
    }

    /// <summary>
    /// Writes a value to multiple pins.
    /// </summary>
    public static void WriteValueToPins(this GpioController controller, int value, params int[] pins)
    {
        for (int i = 0; i < pins.Length; i++)
        {
            controller.Write(pins[i], value);
        }
    }

    /// <summary>
    /// Writers multiple values to the same pin.
    /// </summary>
    public static void WriteValuesToPin(this GpioController controller, int pin, params int[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            controller.Write(pin, values[i]);
        }
    }
}
