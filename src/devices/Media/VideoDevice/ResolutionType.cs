// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Media
{
    /// <summary>
    /// The resolution type of a video device.
    /// </summary>
    public enum ResolutionType : uint
    {
        /// <summary>
        /// Discrete
        /// </summary>
        Discrete = InteropVideodev2.v4l2_frmsizetypes.V4L2_FRMSIZE_TYPE_DISCRETE,

        /// <summary>
        /// Continuous
        /// </summary>
        Continuous = InteropVideodev2.v4l2_frmsizetypes.V4L2_FRMSIZE_TYPE_CONTINUOUS,

        /// <summary>
        /// Stepwise
        /// </summary>
        Stepwise = InteropVideodev2.v4l2_frmsizetypes.V4L2_FRMSIZE_TYPE_STEPWISE
    }
}
