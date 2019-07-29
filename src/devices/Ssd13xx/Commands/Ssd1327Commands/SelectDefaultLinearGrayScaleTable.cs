// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    public class SelectDefaultLinearGrayScaleTable : ISsd1327Command
    {
        /// <summary>
        /// This command reloads the preset linear Gray Scale table.
        /// </summary>
        public SelectDefaultLinearGrayScaleTable()
        {
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xB9;

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id };
        }
    }
}
