// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display
{
    /// <summary>
    /// Register addresses for top-level registers.
    /// </summary>
    public enum Register : byte
    {
        /// <summary>
        /// Address for Command Register.
        /// </summary>
        Command = 0xFD,

        /// <summary>
        /// Address for Function Register.
        /// </summary>
        Function = 0xB
    }

    /// <summary>
    /// Register addresses for the Frame Register.
    /// </summary>
    public enum FrameRegister : byte
    {
        /// <summary>
        /// Address for the LED Control Register.
        /// </summary>
        Led = 0x0,

        /// <summary>
        /// Address for the Blink Control Register.
        /// </summary>
        Blink = 0x12,

        /// <summary>
        /// Address for the PWM Register.
        /// </summary>
        Pwm = 0x24
    }

    /// <summary>
    /// Register addresses for the Function Register.
    /// </summary>
    public enum FunctionRegister : byte
    {
        /// <summary>
        /// Address for the LED Control Register.
        /// </summary>
        CONFIGURATION = 0x0,

        /// <summary>
        /// Address for the Display Option Register.
        /// </summary>
        DisplayOption = 0x5,

        /// <summary>
        /// Address for the Shutdown Register.
        /// </summary>
        Shutdown = 0xA
    }
}
