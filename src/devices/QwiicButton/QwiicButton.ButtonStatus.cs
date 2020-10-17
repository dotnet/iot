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
        /// Returns whether the button is pressed down.
        /// </summary>
        public bool IsPressedDown()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus));
            return status.IsPressedDown;
        }

        /// <summary>
        /// Returns whether the button is clicked, i.e. pressed down then released.
        /// After the button is clicked, must be manually reset to false by calling <see cref="ClearEventBits"/>.
        /// </summary>
        public bool HasBeenClicked()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus));
            return status.HasBeenClicked;
        }

        /// <summary>
        /// Returns whether a new button status event has occurred.
        /// After an event has occurred, must be manually reset to false by calling <see cref="ClearEventBits"/>.
        /// </summary>
        public bool IsEventAvailable()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus));
            return status.IsEventAvailable;
        }

        /// <summary>
        /// Sets <see cref="IsPressedDown"/>, <see cref="HasBeenClicked"/> and <see cref="IsEventAvailable"/> to false.
        /// </summary>
        public void ClearEventBits()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus))
            {
                IsEventAvailable = false,
                HasBeenClicked = false,
                IsPressedDown = false
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
