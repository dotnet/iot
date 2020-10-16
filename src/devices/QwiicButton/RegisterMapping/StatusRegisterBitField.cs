// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.QwiicButton.RegisterMapping
{
    internal struct StatusRegisterBitField
    {
        [Flags]
        private enum StatusRegisterBits
        {
            IsEventAvailable = 1,
            HasBeenClicked = 2,
            IsPressed = 4,
        }

        private StatusRegisterBits _statusRegisterValue;

        public StatusRegisterBitField(byte statusRegisterValue)
        {
            _statusRegisterValue = (StatusRegisterBits)statusRegisterValue;
        }

        public byte StatusRegisterValue => (byte)_statusRegisterValue;

        /// <summary>
        /// Gets set to true when a new event occurs.
        /// Must be manually set to false to clear the flag.
        /// </summary>
        public bool IsEventAvailable
        {
            get { return FlagsHelper.IsSet(_statusRegisterValue, StatusRegisterBits.IsEventAvailable); }
            set { FlagsHelper.SetValue(ref _statusRegisterValue, StatusRegisterBits.IsEventAvailable, value); }
        }

        /// <summary>
        /// Gets set to true if button is pressed, i.e. pushed in.
        /// </summary>
        public bool IsPressed
        {
            get { return FlagsHelper.IsSet(_statusRegisterValue, StatusRegisterBits.IsPressed); }
            set { FlagsHelper.SetValue(ref _statusRegisterValue, StatusRegisterBits.IsPressed, value); }
        }

        /// <summary>
        /// Gets set to true when the button gets clicked, i.e. pressed and released.
        /// Must be manually set to false to clear the flag.
        /// </summary>
        public bool HasBeenClicked
        {
            get { return FlagsHelper.IsSet(_statusRegisterValue, StatusRegisterBits.HasBeenClicked); }
            set { FlagsHelper.SetValue(ref _statusRegisterValue, StatusRegisterBits.HasBeenClicked, value); }
        }
    }
}
