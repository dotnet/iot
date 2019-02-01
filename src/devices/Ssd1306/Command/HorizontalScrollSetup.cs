// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ssd1306.Command
{
    public class HorizontalScrollSetup : ICommand
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

        public byte[] GetBytes()
        {
            return new byte[] { Id, 0x00, (byte)StartPageAddress, (byte)FrameFrequencyType, (byte)EndPageAddress, 0x00, 0xFF };
        }

        public enum HorizontalScrollType
        {
            Right = 0x26,
            Left = 0x27
        }
    }
}
