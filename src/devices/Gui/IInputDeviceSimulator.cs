// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// Simulator for an input device
    /// </summary>
    public interface IInputDeviceSimulator
    {
        /// <summary>
        /// Returns true if this device expects absolute coordinates, false if it requires relative coordinates
        /// </summary>
        bool AbsoluteCoordinates
        {
            get;
        }

        /// <summary>
        /// Move the cursor to the given position
        /// </summary>
        /// <param name="x">X position of cursor</param>
        /// <param name="y">Y position of cursor</param>
        void MoveTo(int x, int y);

        /// <summary>
        /// Click once with the given button at the given position
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="button">Button to press</param>
        void Click(int x, int y, MouseButton button);

        /// <summary>
        /// Returns the current position (always absolute)
        /// </summary>
        (int X, int Y) GetPosition();

        /// <summary>
        /// Press (and start holding) the button at the given position
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="button">Button to press</param>
        void ButtonDown(int x, int y, MouseButton button);

        /// <summary>
        /// Release the button at the given position
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="button">Button to press</param>
        void ButtonUp(int x, int y, MouseButton button);
    }
}
