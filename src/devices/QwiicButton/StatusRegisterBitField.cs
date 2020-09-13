//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.QwiicButton
{
    internal class StatusRegisterBitField
    {
        [Flags]
        private enum StatusRegisterBits
        {
            None = 0,
            EventAvailable = 1,
            HasBeenClicked = 2,
            IsPressed = 4,
        }

        public StatusRegisterBitField(byte statusRegisterValue)
        {
            StatusRegisterValue = statusRegisterValue;
        }

        public byte StatusRegisterValue { get; }

        /// <summary>
        /// User mutable, gets set to 1 when a new event occurs. User is expected to write 0 to clear the flag.
        /// </summary>
        public bool EventAvailable
        {
            get { return (((StatusRegisterBits)StatusRegisterValue) & StatusRegisterBits.EventAvailable) != StatusRegisterBits.None; }
        }

        /// <summary>
        /// Gets set to one if button is pushed.
        /// </summary>
        public bool IsPressed
        {
            get { return (((StatusRegisterBits)StatusRegisterValue) & StatusRegisterBits.IsPressed) != StatusRegisterBits.None; }
        }

        /// <summary>
        /// Defaults to zero on POR. Gets set to one when the button gets clicked. Must be cleared by the user.
        /// </summary>
        public bool HasBeenClicked
        {
            get { return (((StatusRegisterBits)StatusRegisterValue) & StatusRegisterBits.HasBeenClicked) != StatusRegisterBits.None; }
        }
    }
}