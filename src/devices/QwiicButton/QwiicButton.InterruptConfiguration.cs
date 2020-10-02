//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

namespace Iot.Device.QwiicButton
{
    public partial class QwiicButton
    {
        /// <summary>
        /// When called, the interrupt will be configured to trigger when the button is pressed.
        /// If <see cref="EnableClickedInterrupt"/> has also been called,
        /// then the interrupt will trigger on either a push or a click.
        /// </summary>
        public byte EnablePressedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_i2cBus.ReadSingleRegister(Register.InterruptConfig))
                {
                    PressedEnable = true
                };
            return _i2cBus.WriteSingleRegisterWithReadback(Register.InterruptConfig, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// When called, the interrupt will no longer be configured to trigger when the button is pressed.
        /// If <see cref="EnableClickedInterrupt"/> has also been called,
        /// then the interrupt will still trigger on the button click.
        /// </summary>
        public byte DisablePressedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_i2cBus.ReadSingleRegister(Register.InterruptConfig))
                {
                    PressedEnable = false
                };
            return _i2cBus.WriteSingleRegisterWithReadback(Register.InterruptConfig, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// When called, the interrupt will be configured to trigger when the button is clicked.
        /// If <see cref="EnablePressedInterrupt"/> has also been called,
        /// then the interrupt will trigger on either a push or a click.
        /// </summary>
        public byte EnableClickedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_i2cBus.ReadSingleRegister(Register.InterruptConfig))
                {
                    ClickedEnable = true
                };

            return _i2cBus.WriteSingleRegisterWithReadback(Register.InterruptConfig, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// When called, the interrupt will no longer be configured to trigger when the button is clicked.
        /// If <see cref="EnablePressedInterrupt"/> has also been called,
        /// then the interrupt will still trigger on the button press.
        /// </summary>
        public byte DisableClickedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_i2cBus.ReadSingleRegister(Register.InterruptConfig))
                {
                    ClickedEnable = false
                };

            return _i2cBus.WriteSingleRegisterWithReadback(Register.InterruptConfig, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// Resets the interrupt configuration back to defaults.
        /// </summary>
        public byte ResetInterruptConfig()
        {
            var interrupt = new InterruptConfigBitField
            {
                PressedEnable = true,
                ClickedEnable = true
            };
            var interruptValue = _i2cBus.WriteSingleRegisterWithReadback(Register.InterruptConfig, interrupt.InterruptConfigValue);

            var status = new StatusRegisterBitField
            {
                EventAvailable = false
            };
            _i2cBus.WriteSingleRegisterWithReadback(Register.ButtonStatus, status.StatusRegisterValue);

            return interruptValue;
        }
    }
}
