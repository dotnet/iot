// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3.Sensors
{
    /// <summary>
    /// A Button class
    /// </summary>
    public class Button : DigitalInput
    {
        /// <summary>
        /// Button constructor
        /// </summary>
        /// <param name="goPiGo">The GoPiGo3 class</param>
        /// <param name="port">The Grove Port, need to be in the list of SupportedPorts</param>
        public Button(GoPiGo goPiGo, GrovePort port)
            : base(goPiGo, port)
        {
        }

        /// <summary>
        /// True if the button is pressed, flase otherwise
        /// </summary>
        public bool IsPressed => Value != 0;

        /// <summary>
        /// Get "Pressed" if the button is pressed "Not pressed" if not
        /// </summary>
        public override string ToString() => Value != 0 ? "Pressed" : "Not pressed";

        /// <summary>
        /// Get the sensor name "Button"
        /// </summary>
        public new string SensorName => "Button";
    }
}
