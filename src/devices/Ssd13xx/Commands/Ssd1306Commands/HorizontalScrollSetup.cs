// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    public enum FrameFrequencyType
    {
        Frames2 = 0x07,
        Frames3 = 0x04,
        Frames4 = 0x05,
        Frames5 = 0x00,
        Frames25 = 0x06,
        Frames64 = 0x01,
        Frames128 = 0x02,
        Frames256 = 0x03
    }

    public class HorizontalScrollSetup : ISsd1306Command
    {
        /// <summary>
        /// This command consists of consecutive bytes to set up the horizontal scroll parameters
        /// and determines the scrolling start page, end page and scrolling speed.
        /// </summary>
        /// <param name="scrollType">Horizontal scroll type.</param>
        /// <param name="startPageAddress">Start page address with a range of 0-7.</param>
        /// <param name="frameFrequencyType">Frame frequency type with a range of 0-7.</param>
        /// <param name="endPageAddress">End page address with a range of 0-7.</param>
        public HorizontalScrollSetup(
            HorizontalScrollType scrollType,
            PageAddress startPageAddress,
            FrameFrequencyType frameFrequencyType,
            PageAddress endPageAddress)
        {
            ScrollType = scrollType;
            StartPageAddress = startPageAddress;
            FrameFrequencyType = frameFrequencyType;
            EndPageAddress = endPageAddress;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => (byte)ScrollType;

        /// <summary>
        /// Horizontal scroll type.
        /// </summary>
        public HorizontalScrollType ScrollType { get; }

        /// <summary>
        /// Start page address with a range of 0-7.
        /// </summary>
        public PageAddress StartPageAddress { get; }

        /// <summary>
        /// Frame frequency type with a range of 0-7.
        /// </summary>
        public FrameFrequencyType FrameFrequencyType { get; }

        /// <summary>
        /// End page address with a range of 0-7.
        /// </summary>
        public PageAddress EndPageAddress { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, 0x00, (byte)StartPageAddress, (byte)FrameFrequencyType, (byte)EndPageAddress, 0x00, 0xFF };
        }

        public enum HorizontalScrollType
        {
            /// <summary>
            /// Right horizontal scroll.
            /// </summary>
            Right = 0x26,
            /// <summary>
            /// Left horizontal scroll.
            /// </summary>
            Left = 0x27
        }
    }
}
