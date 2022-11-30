// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display
{
    /// <summary>
    /// Register addresses for the Command Register.
    /// </summary>
    // Enables pages one to nine
    // Table 2 in datasheet
    public static class CommandRegister
    {
        /// <summary>
        /// Address for Command Register.
        /// </summary>
        public static byte Command = 0xFD;

        /// <summary>
        /// Address for Function Register.
        /// </summary>
        public static byte Function = 0xB;
    }

    /// <summary>
    /// Register addresses for the Frame Register.
    /// </summary>
    // Writes data to pages one to eight
    // Table 3 in datasheet
    public static class FrameRegister
    {
        /// <summary>
        /// Address for the LED Control Register.
        /// </summary>
        public static byte Led = 0x0;

        /// <summary>
        /// Address for the Blink Control Register.
        /// </summary>
        public static byte Blink = 0x12;

        /// <summary>
        /// Address for the PWM Register.
        /// </summary>
        public static byte Pwm = 0x24;
    }

    /// <summary>
    /// Register addresses for the Function Register.
    /// </summary>
    // Writes data to page nine
    // Table 3 in datasheet
    public static class FunctionRegister
    {
        /// <summary>
        /// Address for the LED Control Register.
        /// </summary>
        public static byte Configuration = 0x0;

        /// <summary>
        /// Address for the Display Option Register.
        /// </summary>
        public static byte DisplayOption = 0x5;

        /// <summary>
        /// Address for the Shutdown Register.
        /// </summary>
        public static byte Shutdown = 0xA;
    }
}
