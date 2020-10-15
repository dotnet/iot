// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.QwiicButton.RegisterMapping;

namespace Iot.Device.QwiicButton
{
    public sealed partial class QwiicButton
    {
        /// <summary>
        /// When called, the interrupt will be configured to trigger when the button is pressed.
        /// If <see cref="EnableClickedInterrupt"/> has also been called,
        /// then the interrupt will trigger on either a push or a click.
        /// </summary>
        public void EnablePressedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_registerAccess.ReadRegister<byte>(Register.InterruptConfig))
                {
                    PressedEnable = true
                };
            _registerAccess.WriteRegister(Register.InterruptConfig, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// When called, the interrupt will no longer be configured to trigger when the button is pressed.
        /// If <see cref="EnableClickedInterrupt"/> has also been called,
        /// then the interrupt will still trigger on the button click.
        /// </summary>
        public void DisablePressedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_registerAccess.ReadRegister<byte>(Register.InterruptConfig))
                {
                    PressedEnable = false
                };
            _registerAccess.WriteRegister(Register.InterruptConfig, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// When called, the interrupt will be configured to trigger when the button is clicked.
        /// If <see cref="EnablePressedInterrupt"/> has also been called,
        /// then the interrupt will trigger on either a push or a click.
        /// </summary>
        public void EnableClickedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_registerAccess.ReadRegister<byte>(Register.InterruptConfig))
                {
                    ClickedEnable = true
                };

            _registerAccess.WriteRegister(Register.InterruptConfig, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// When called, the interrupt will no longer be configured to trigger when the button is clicked.
        /// If <see cref="EnablePressedInterrupt"/> has also been called,
        /// then the interrupt will still trigger on the button press.
        /// </summary>
        public void DisableClickedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_registerAccess.ReadRegister<byte>(Register.InterruptConfig))
                {
                    ClickedEnable = false
                };

            _registerAccess.WriteRegister(Register.InterruptConfig, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// Resets the interrupt configuration back to defaults.
        /// </summary>
        public void ResetInterruptConfig()
        {
            var interrupt = new InterruptConfigBitField
            {
                PressedEnable = true,
                ClickedEnable = true
            };
            _registerAccess.WriteRegister(Register.InterruptConfig, interrupt.InterruptConfigValue);

            var status = new StatusRegisterBitField
            {
                EventAvailable = false
            };
            _registerAccess.WriteRegister(Register.ButtonStatus, status.StatusRegisterValue);
        }
    }
}
