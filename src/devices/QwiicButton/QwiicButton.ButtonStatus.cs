//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

namespace Iot.Device.QwiicButton
{
    public partial class QwiicButton
    {
        /// <summary>
        /// Returns whether the button is pressed.
        /// </summary>
        public bool IsPressed()
        {
            var status = new StatusRegisterBitField(_i2cBus.ReadSingleRegister(Register.ButtonStatus));
            return status.IsPressed;
        }

        /// <summary>
        /// Returns whether the button is clicked.
        /// </summary>
        public bool HasBeenClicked()
        {
            var status = new StatusRegisterBitField(_i2cBus.ReadSingleRegister(Register.ButtonStatus));
            return status.HasBeenClicked;
        }

        /// <summary>
        /// Returns whether a new button status event has occurred.
        /// </summary>
        public bool EventAvailable()
        {
            var status = new StatusRegisterBitField(_i2cBus.ReadSingleRegister(Register.ButtonStatus));
            return status.EventAvailable;
        }

        /// <summary>
        /// Sets <see cref="IsPressed"/>, <see cref="HasBeenClicked"/> and <see cref="EventAvailable"/> to false.
        /// </summary>
        public byte ClearEventBits()
        {
            var status = new StatusRegisterBitField(_i2cBus.ReadSingleRegister(Register.ButtonStatus))
            {
                EventAvailable = false,
                HasBeenClicked = false,
                IsPressed = false
            };
            return _i2cBus.WriteSingleRegisterWithReadback(Register.ButtonStatus, status.StatusRegisterValue);
        }

        /// <summary>
        /// Returns the time in milliseconds that the button waits for the mechanical contacts to settle.
        /// </summary>
        public ushort GetDebounceTime()
        {
            return _i2cBus.ReadDoubleRegister(Register.ButtonDebounceTime);
        }

        /// <summary>
        /// Sets the time in milliseconds that the button waits for the mechanical contacts to settle and checks if the register was set properly.
        /// Returns 0 on success, 1 on register I2C write fail, and 2 if the value didn't get written into the register properly.
        /// </summary>
        public byte SetDebounceTime(ushort time)
        {
            return (byte)_i2cBus.WriteDoubleRegisterWithReadback(Register.ButtonDebounceTime, time);
        }
    }
}
