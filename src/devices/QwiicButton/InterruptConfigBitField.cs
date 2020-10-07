//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Common;

namespace Iot.Device.QwiicButton
{
    internal struct InterruptConfigBitField
    {
        [Flags]
        private enum InterruptConfigBits
        {
            ClickedEnable = 1,
            PressedEnable = 2
        }

        public InterruptConfigBitField(byte interruptConfigValue)
        {
            InterruptConfigValue = interruptConfigValue;
        }

        public byte InterruptConfigValue { get; set; }

        /// <summary>
        /// Set to true to enable an interrupt when the button is clicked.
        /// Defaults to false.
        /// </summary>
        public bool ClickedEnable
        {
            get { return FlagsHelper.IsSet((InterruptConfigBits)InterruptConfigValue, InterruptConfigBits.ClickedEnable); }
            set { InterruptConfigValue = (byte)FlagsHelper.SetValue((InterruptConfigBits)InterruptConfigValue, InterruptConfigBits.ClickedEnable, value); }
        }

        /// <summary>
        /// Set to true to enable an interrupt when the button is pressed.
        /// Defaults to false.
        /// </summary>
        public bool PressedEnable
        {
            get { return FlagsHelper.IsSet((InterruptConfigBits)InterruptConfigValue, InterruptConfigBits.PressedEnable); }
            set { InterruptConfigValue = (byte)FlagsHelper.SetValue((InterruptConfigBits)InterruptConfigValue, InterruptConfigBits.PressedEnable, value); }
        }
    }
}
