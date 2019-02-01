// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

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
            switch (scrollType)
            {
                case HorizontalScrollType.Right:
                case HorizontalScrollType.Left:
                    break;
                default:
                    throw new ArgumentException("The horizontal scroll type is invalid.", nameof(scrollType));
            }

            switch (startPageAddress)
            {
                case PageAddress.Page0:
                case PageAddress.Page1:
                case PageAddress.Page2:
                case PageAddress.Page3:
                case PageAddress.Page4:
                case PageAddress.Page5:
                case PageAddress.Page6:
                case PageAddress.Page7:
                    break;
                default:
                    throw new ArgumentException("The start page address is invalid.", nameof(startPageAddress));
            }

            switch (frameFrequencyType)
            {
                case FrameFrequencyType.Frames5:
                case FrameFrequencyType.Frames64:
                case FrameFrequencyType.Frames128:
                case FrameFrequencyType.Frames256:
                case FrameFrequencyType.Frames3:
                case FrameFrequencyType.Frames4:
                case FrameFrequencyType.Frames25:
                case FrameFrequencyType.Frames2:
                    break;
                default:
                    throw new ArgumentException("The frame frequency type is invalid.", nameof(frameFrequencyType));
            }

            switch (endPageAddress)
            {
                case PageAddress.Page0:
                case PageAddress.Page1:
                case PageAddress.Page2:
                case PageAddress.Page3:
                case PageAddress.Page4:
                case PageAddress.Page5:
                case PageAddress.Page6:
                case PageAddress.Page7:
                    break;
                default:
                    throw new ArgumentException("The end page address is invalid.", nameof(endPageAddress));
            }

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
