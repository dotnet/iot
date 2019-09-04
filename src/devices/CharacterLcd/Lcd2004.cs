// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using System.Drawing;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// 20x4 HD44780 compatible character LCD display.
    /// </summary>
    public class Lcd2004 : Hd44780
    {
        /// <summary>
        /// Constructs a new HD44780 based 20x4 LCD controller.
        /// </summary>
        /// <param name="registerSelectPin">The pin that controls the regsiter select.</param>
        /// <param name="enablePin">The pin that controls the enable switch.</param>
        /// <param name="dataPins">Collection of pins holding the data that will be printed on the screen.</param>
        /// <param name="backlightPin">The optional pin that controls the backlight of the display.</param>
        /// <param name="backlightBrightness">The brightness of the backlight. 0.0 for off, 1.0 for on.</param>
        /// <param name="readWritePin">The optional pin that controls the read and write switch.</param>
        /// <param name="controller">The controller to use with the LCD. If not specified, uses the platform default.</param>
        public Lcd2004(int registerSelectPin, int enablePin, int[] dataPins, int backlightPin = -1, float backlightBrightness = 1.0f, int readWritePin = -1, GpioController controller = null)
            : base(new Size(20, 4), LcdInterface.CreateGpio(registerSelectPin, enablePin, dataPins, backlightPin, backlightBrightness, readWritePin, controller))
        {
        }
    }
}
