// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// Qualifies the category of the commands used by the libcamera-apps according to:
/// https://www.raspberrypi.com/documentation/computers/camera_software.html#common-command-line-options
/// </summary>
public enum CommandCategory
{
    /// <summary>
    /// Common options
    /// </summary>
    Common,

    /// <summary>
    /// Options related to the preview window
    /// </summary>
    Preview,

    /// <summary>
    /// Options related to the camera resolution
    /// </summary>
    CameraResolution,

    /// <summary>
    /// Options used to control the camera
    /// </summary>
    CameraControl,

    /// <summary>
    /// Options to redirect the image or video output
    /// </summary>
    Output,

    /// <summary>
    /// Options to process the image or video after the acquisition but before the output
    /// </summary>
    PostProcessing,

    /// <summary>
    /// Options to control the image acquisition
    /// </summary>
    Still,

    /// <summary>
    /// Options to control the video acquisition
    /// </summary>
    Video,
}
