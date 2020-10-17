// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.QwiicButton.RegisterMapping;

namespace Iot.Device.QwiicButton
{
    public sealed partial class QwiicButton
    {
        /// <summary>
        /// When called, the interrupt will be configured to trigger when the button is pressed down.
        /// If <see cref="EnableClickedInterrupt"/> has also been called,
        /// then the interrupt will trigger on either a press down or a click (press down + release).
        /// Interrupt is enabled by default.
        /// </summary>
        public void EnablePressedDownInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_registerAccess.ReadRegister<byte>(Register.InterruptConfig))
                {
                    PressedEnable = true
                };
            _registerAccess.WriteRegister(Register.InterruptConfig, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// When called, the interrupt will no longer be configured to trigger when the button is pressed down.
        /// If <see cref="EnableClickedInterrupt"/> has also been called,
        /// then the interrupt will still trigger on button click (press down + release).
        /// Interrupt is enabled by default.
        /// </summary>
        public void DisablePressedDownInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_registerAccess.ReadRegister<byte>(Register.InterruptConfig))
                {
                    PressedEnable = false
                };
            _registerAccess.WriteRegister(Register.InterruptConfig, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// When called, the interrupt will be configured to trigger when the button is clicked (pressed down + released).
        /// If <see cref="EnablePressedDownInterrupt"/> has also been called,
        /// then the interrupt will trigger on either a press down or a click.
        /// Interrupt is enabled by default.
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
        /// When called, the interrupt will no longer be configured to trigger when the button is clicked (pressed down + released).
        /// If <see cref="EnablePressedDownInterrupt"/> has also been called,
        /// then the interrupt will still trigger on button pressed down.
        /// Interrupt is enabled by default.
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
        /// Resets the interrupt configuration back to defaults,
        /// i.e. the interrupt will trigger on either a button press down or click (press down + release).
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
                IsEventAvailable = false
            };
            _registerAccess.WriteRegister(Register.ButtonStatus, status.StatusRegisterValue);
        }
    }
}
