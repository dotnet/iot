// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GrovePiDevice.Models;
using System.Device.Gpio;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// Button is a generic digital button class
    /// </summary>
    public class Button : DigitalInput
    {
        /// <summary>
        /// Button constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public Button(GrovePi grovePi, GrovePort port) : base(grovePi, port)
        { }

        /// <summary>
        /// Returns "Pressed" then button is pressed, "Not pressed" otherwise
        /// </summary>
        /// <returns>Returns "Pressed" then button is pressed, "Not pressed" otherwise</returns>
        public override string ToString() => Value == PinValue.High ? "Pressed" : "Not presed";

        /// <summary>
        /// Get the name Button
        /// </summary>
        public new string SensorName => "Button";
    }
}
