// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display
{
    /// <summary>
    /// Register addresses for the Function Register.
    /// </summary>
    public static class FunctionRegister
    {
        /// <summary>
        /// Address for Configuration Register.
        /// </summary>
        public static byte Configuration = 0x0;

        /// <summary>
        /// Address for Matrix 1 Data Register.
        /// </summary>
        public static byte Matrix1 = 0x1;

        /// <summary>
        /// Address for Matrix 2 Data Register.
        /// </summary>
        public static byte Matrix2 = 0x0E;

        /// <summary>
        /// Address for Update Column Register.
        /// </summary>
        public static byte UpdateColumn = 0x0C;

        /// <summary>
        /// Address for Lighting Effect Register.
        /// </summary>
        public static byte LightingEffect = 0x0D;

        /// <summary>
        /// Address for PWM Register.
        /// </summary>
        public static byte Pwm = 0x19;

        /// <summary>
        /// Address for Reset Register.
        /// </summary>
        public static byte Reset = 0x0C;
    }

    /// <summary>
    /// Register addresses for the Configuration Register.
    /// </summary>
    public static class ConfigurationRegister
    {
        /// <summary>
        /// Shutdown value.
        /// </summary>
        public static byte Shutdown = 0x80;
    }
}
