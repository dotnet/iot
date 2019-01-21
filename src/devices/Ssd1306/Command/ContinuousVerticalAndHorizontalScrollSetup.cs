// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Ssd1306.Command
{
    public class ContinuousVerticalAndHorizontalScrollSetup : ICommand
    {
        /// <summary>
        /// This command consists of 6 consecutive bytes to set up the continuous vertical
        /// scroll parameters and determines the scrolling start page, end page, scrolling
        /// speed and vertical scrolling offset.
        /// </summary>
        /// <param name="scrollType">Vertical/Horizontal scroll type.</param>
        /// <param name="startPageAddress">Start page address with a range of 0-7.</param>
        /// <param name="frameFrequencyType">Frame frequency type with a range of 0-7.</param>
        /// <param name="endPageAddress">End page address with a range of 0-7.</param>
        /// <param name="verticalScrollingOffset">Vertical scrolling offset with a range of 0-63.</param>
        public ContinuousVerticalAndHorizontalScrollSetup(
            VerticalHorizontalScrollType scrollType,
            PageAddress startPageAddress,
            FrameFrequencyType frameFrequencyType,
            PageAddress endPageAddress,
            byte verticalScrollingOffset)
        {
            switch (scrollType)
            {
                case VerticalHorizontalScrollType.Right:
                case VerticalHorizontalScrollType.Left:
                    break;
                default:
                    throw new ArgumentException("The vertical/horizontal scroll type is invalid.", nameof(scrollType));
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

            if (verticalScrollingOffset > 0x3F)
            {
                throw new ArgumentException("The vertical scrolling offset is invalid.", nameof(verticalScrollingOffset));
            }

            ScrollType = scrollType;
            StartPageAddress = startPageAddress;
            FrameFrequencyType = frameFrequencyType;
            EndPageAddress = endPageAddress;
            VerticalScrollingOffset = verticalScrollingOffset;
        }

        public byte Value => (byte)ScrollType;

        /// <summary>
        /// Vertical/Horizontal scroll type.
        /// </summary>
        public VerticalHorizontalScrollType ScrollType { get; }

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
        /// Vertical scrolling offset with a range of 0-63.
        /// </summary>
        public byte VerticalScrollingOffset { get; }

        public byte[] GetBytes()
        {
            return new byte[] { Value, 0x00, (byte)StartPageAddress, (byte)FrameFrequencyType, (byte)EndPageAddress, VerticalScrollingOffset };
        }

        public enum VerticalHorizontalScrollType
        {
            Right = 0x29,
            Left = 0x2A
        }
    }
}
