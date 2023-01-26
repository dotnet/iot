// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display.Internal
{
    /// <summary>
    /// Register addresses for the Function Register.
    /// </summary>
    internal static class Is31fl3730FunctionRegister
    {
        /// <summary>
        /// Address for Configuration Register.
        /// </summary>
        internal static byte Configuration = 0x0;

        /// <summary>
        /// Address for Matrix 1 Data Register.
        /// </summary>
        internal static byte Matrix1 = 0x01;

        /// <summary>
        /// Address for Update Column Register.
        /// </summary>
        internal static byte UpdateColumn = 0x0C;

        /// <summary>
        /// Address for Lighting Effect Register.
        /// </summary>
        internal static byte LightingEffect = 0x0D;

        /// <summary>
        /// Address for Matrix 2 Data Register.
        /// </summary>
        internal static byte Matrix2 = 0x0E;

        /// <summary>
        /// Address for PWM Register.
        /// </summary>
        internal static byte Pwm = 0x19;

        /// <summary>
        /// Address for Reset Register.
        /// </summary>
        internal static byte Reset = 0xFF;
    }
}
