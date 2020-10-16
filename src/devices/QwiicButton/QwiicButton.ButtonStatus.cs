// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.QwiicButton.RegisterMapping;

namespace Iot.Device.QwiicButton
{
    public sealed partial class QwiicButton
    {
        /// <summary>
        /// Returns whether the button is pressed, i.e. pushed in.
        /// </summary>
        public bool IsPressed()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus));
            return status.IsPressed;
        }

        /// <summary>
        /// Returns whether the button is clicked, i.e. pressed and released.
        /// </summary>
        public bool HasBeenClicked()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus));
            return status.HasBeenClicked;
        }

        /// <summary>
        /// Returns whether a new button status event has occurred.
        /// </summary>
        public bool IsEventAvailable()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus));
            return status.IsEventAvailable;
        }

        /// <summary>
        /// Sets <see cref="IsPressed"/>, <see cref="HasBeenClicked"/> and <see cref="IsEventAvailable"/> to false.
        /// </summary>
        public void ClearEventBits()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus))
            {
                IsEventAvailable = false,
                HasBeenClicked = false,
                IsPressed = false
            };
            _registerAccess.WriteRegister(Register.ButtonStatus, status.StatusRegisterValue);
        }

        /// <summary>
        /// Returns interval of time that the button waits for the mechanical contacts to settle.
        /// Default is 10 milliseconds.
        /// </summary>
        public TimeSpan GetDebounceTime()
        {
            var debounceTimeInMs = _registerAccess.ReadRegister<ushort>(Register.ButtonDebounceTime);
            return TimeSpan.FromMilliseconds(debounceTimeInMs);
        }

        /// <summary>
        /// Sets the time in milliseconds that the button waits for the mechanical contacts to settle.
        /// Default is 10 milliseconds.
        /// </summary>
        public void SetDebounceTime(ushort time)
        {
            _registerAccess.WriteRegister(Register.ButtonDebounceTime, time);
        }
    }
}
