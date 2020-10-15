// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.QwiicButton.RegisterMapping;

namespace Iot.Device.QwiicButton
{
    public sealed partial class QwiicButton
    {
        /// <summary>
        /// Returns whether the button is pressed.
        /// </summary>
        public bool IsPressed()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus));
            return status.IsPressed;
        }

        /// <summary>
        /// Returns whether the button is clicked.
        /// </summary>
        public bool HasBeenClicked()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus));
            return status.HasBeenClicked;
        }

        /// <summary>
        /// Returns whether a new button status event has occurred.
        /// </summary>
        public bool EventAvailable()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus));
            return status.EventAvailable;
        }

        /// <summary>
        /// Sets <see cref="IsPressed"/>, <see cref="HasBeenClicked"/> and <see cref="EventAvailable"/> to false.
        /// </summary>
        public void ClearEventBits()
        {
            var status = new StatusRegisterBitField(_registerAccess.ReadRegister<byte>(Register.ButtonStatus))
            {
                EventAvailable = false,
                HasBeenClicked = false,
                IsPressed = false
            };
            _registerAccess.WriteRegister(Register.ButtonStatus, status.StatusRegisterValue);
        }

        /// <summary>
        /// Returns the time in milliseconds that the button waits for the mechanical contacts to settle.
        /// </summary>
        public ushort GetDebounceTime()
        {
            return _registerAccess.ReadRegister<ushort>(Register.ButtonDebounceTime);
        }

        /// <summary>
        /// Sets the time in milliseconds that the button waits for the mechanical contacts to settle and checks if the register was set properly.
        /// </summary>
        public void SetDebounceTime(ushort time)
        {
            _registerAccess.WriteRegister(Register.ButtonDebounceTime, time);
        }
    }
}
