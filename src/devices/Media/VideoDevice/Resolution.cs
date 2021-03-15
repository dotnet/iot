// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Media
{
    /// <summary>
    /// The resolution of a video device.
    /// </summary>
    public class Resolution
    {
        /// <summary>
        /// Resolution's type
        /// </summary>
        public ResolutionType Type { get; set; }

        /// <summary>
        /// Resolution's minimum height
        /// </summary>
        public uint MinHeight { get; set; }

        /// <summary>
        /// Resolution's maximum height
        /// </summary>
        public uint MaxHeight { get; set; }

        /// <summary>
        /// Resolution's step for height
        /// </summary>
        public uint StepHeight { get; set; }

        /// <summary>
        /// Resolution's minimum width
        /// </summary>
        public uint MinWidth { get; set; }

        /// <summary>
        /// Resolution's maximum width
        /// </summary>
        public uint MaxWidth { get; set; }

        /// <summary>
        /// Resolution's step for width
        /// </summary>
        public uint StepWidth { get; set; }
    }
}
