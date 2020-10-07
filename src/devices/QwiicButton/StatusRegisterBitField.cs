//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Common;

namespace Iot.Device.QwiicButton
{
    internal struct StatusRegisterBitField
    {
        [Flags]
        private enum StatusRegisterBits
        {
            EventAvailable = 1,
            HasBeenClicked = 2,
            IsPressed = 4,
        }

        public StatusRegisterBitField(byte statusRegisterValue)
        {
            StatusRegisterValue = statusRegisterValue;
        }

        public byte StatusRegisterValue { get; set; }

        /// <summary>
        /// Gets set to true when a new event occurs.
        /// Must be manually set to false to clear the flag.
        /// </summary>
        public bool EventAvailable
        {
            get { return FlagsHelper.IsSet((StatusRegisterBits)StatusRegisterValue, StatusRegisterBits.EventAvailable); }
            set { StatusRegisterValue = (byte)FlagsHelper.SetValue((StatusRegisterBits)StatusRegisterValue, StatusRegisterBits.EventAvailable, value); }
        }

        /// <summary>
        /// Gets set to true if button is pushed.
        /// </summary>
        public bool IsPressed
        {
            get { return FlagsHelper.IsSet((StatusRegisterBits)StatusRegisterValue, StatusRegisterBits.IsPressed); }
            set { StatusRegisterValue = (byte)FlagsHelper.SetValue((StatusRegisterBits)StatusRegisterValue, StatusRegisterBits.IsPressed, value); }
        }

        /// <summary>
        /// Gets set to true when the button gets clicked.
        /// Must be manually set to false to clear the flag.
        /// </summary>
        public bool HasBeenClicked
        {
            get { return FlagsHelper.IsSet((StatusRegisterBits)StatusRegisterValue, StatusRegisterBits.HasBeenClicked); }
            set { StatusRegisterValue = (byte)FlagsHelper.SetValue((StatusRegisterBits)StatusRegisterValue, StatusRegisterBits.HasBeenClicked, value); }
        }
    }
}
