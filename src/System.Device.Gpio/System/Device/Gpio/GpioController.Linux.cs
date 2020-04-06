// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Drivers;

namespace System.Device.Gpio
{
    public sealed partial class GpioController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GpioController"/> class that will use the specified numbering scheme.
        /// The controller will default to use the driver that best applies given the platform the program is executing on.
        /// </summary>
        /// <param name="numberingScheme">The numbering scheme used to represent pins provided by the controller.</param>
        public GpioController(PinNumberingScheme numberingScheme)
            : this(numberingScheme, GetBestDriverForBoard())
        {
        }

        /// <summary>
        /// Attempt to get the best applicable driver for the board the program is executing on.
        /// </summary>
        /// <returns>A driver that works with the board the program is executing on.</returns>
        private static GpioDriver GetBestDriverForBoard()
        {
            BoardIdentification board = BoardIdentification.LoadBoard();
            switch (board.GetBoardModel())
            {
                case BoardIdentification.Model.RaspberryPiB3:
                    return new RaspberryPi3Driver();
                case BoardIdentification.Model.RaspberryPiComputeModule3:
                    return new RaspberryPiCM3Driver();
                default:
                    return UnixDriver.Create();
            }
        }
    }
}
