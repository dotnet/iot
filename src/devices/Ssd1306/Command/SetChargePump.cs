// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetChargePump : ICommand
    {
        /// <summary>
        /// This command controls the switching capacitor regulator circuit.
        /// </summary>
        /// <param name="enableChargePump">Determines if charge pump is enabled while the display is on.</param>
        public SetChargePump(bool enableChargePump = false)
        {
            EnableChargePump = enableChargePump;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0x8D;

        /// <summary>
        /// Determines if charge pump is enabled while the display is on.
        /// </summary>
        public bool EnableChargePump { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            byte enableChargePump = 0x10;

            if (EnableChargePump)
            {
                enableChargePump = 0x14;
            }

            return new byte[] { Id, enableChargePump };
        }
    }
}
