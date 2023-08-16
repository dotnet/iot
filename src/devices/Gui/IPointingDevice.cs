// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Gui
{
    /// <summary>
    /// Interface representing a (virtual) pointing device.
    /// </summary>
    public interface IPointingDevice : IDisposable
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
        /// Move the cursor to the given position
        /// </summary>
        /// <param name="point">Absolute position on screen to move cursor to</param>
        void MoveTo(Point point);

        /// <summary>
        /// Move the cursor by the given amount
        /// </summary>
        /// <param name="x">Move by this amount in x</param>
        /// <param name="y">Move by this amount in y</param>
        void MoveBy(int x, int y);

        /// <summary>
        /// Click once with the given buttons
        /// </summary>
        /// <param name="buttons">Button(s) to press</param>
        void Click(MouseButton buttons);

        /// <summary>
        /// Returns the current position (always absolute)
        /// </summary>
        Point GetPosition();

        /// <summary>
        /// Press (and start holding) the given buttons
        /// </summary>
        /// <param name="buttons">Buttons to press</param>
        void ButtonDown(MouseButton buttons);

        /// <summary>
        /// Release the given buttons
        /// </summary>
        /// <param name="buttons">Buttons to release</param>
        void ButtonUp(MouseButton buttons);
    }
}
