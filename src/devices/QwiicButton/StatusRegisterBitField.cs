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
        public bool EventAvailable
        {
            get { return FlagsHelper.IsSet(_statusRegisterValue, StatusRegisterBits.EventAvailable); }
            set { FlagsHelper.SetValue(ref _statusRegisterValue, StatusRegisterBits.EventAvailable, value); }
        }

        /// <summary>
        /// Gets set to true if button is pushed.
        /// </summary>
        public bool IsPressed
        {
            get { return FlagsHelper.IsSet(_statusRegisterValue, StatusRegisterBits.IsPressed); }
            set { FlagsHelper.SetValue(ref _statusRegisterValue, StatusRegisterBits.IsPressed, value); }
        }

        /// <summary>
        /// Gets set to true when the button gets clicked.
        /// Must be manually set to false to clear the flag.
        /// </summary>
        public bool HasBeenClicked
        {
            get { return FlagsHelper.IsSet(_statusRegisterValue, StatusRegisterBits.HasBeenClicked); }
            set { FlagsHelper.SetValue(ref _statusRegisterValue, StatusRegisterBits.HasBeenClicked, value); }
        }
    }
}
