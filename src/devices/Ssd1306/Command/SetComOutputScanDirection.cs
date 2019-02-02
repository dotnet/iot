// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class SetComOutputScanDirection : ICommand
    {
        /// <summary>
        /// This command sets the scan direction of the COM output, allowing layout flexibility
        /// in the OLED module design. Additionally, the display will show once this command is
        /// issued. For example, if this command is sent during normal display then the graphic
        /// display will be vertically flipped immediately.
        /// </summary>
        /// <param name="normalMode">
        /// Scan from COM0 to COM[N –1] when TRUE.
        /// Scan from COM[N - 1] to COM0 when FALSE.
        /// </param>
        public SetComOutputScanDirection(bool normalMode = true)
        {
            NormalMode = normalMode;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => (byte)(NormalMode ? 0xC0 : 0xC8);

        /// <summary>
        /// Scan from COM0 to COM[N –1] when TRUE.
        /// Scan from COM[N - 1] to COM0 when FALSE.
        /// </summary>
        public bool NormalMode { get; set; }

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
