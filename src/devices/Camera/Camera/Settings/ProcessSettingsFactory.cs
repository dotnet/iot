// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Iot.Device.Common;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// A factory for the <see cref="ProcessSettings"/> class.
/// </summary>
public static class ProcessSettingsFactory
{
    /// <summary>
    /// The process name of the legacy application used to capture still pictures on the Raspbian OS
    /// </summary>
    public const string RaspiStill = "raspistill";

    /// <summary>
    /// The process name of the legacy application used to capture video streams on the Raspbian OS
    /// </summary>
    public const string RaspiVid = "raspivid";

    /// <summary>
    /// The process name of the libcamera application used to capture still pictures on the Raspbian OS
    /// </summary>
    public const string LibcameraStill = "libcamera-still";

    /// <summary>
    /// The process name of the libcamera application used to capture video streams on the Raspbian OS
    /// </summary>
    public const string LibcameraVid = "libcamera-vid";

    /// <summary>
    /// Creates a ProcessSettings instance targeting raspistill.
    /// </summary>
    /// <returns>An instance of the <see cref="ProcessSettings"/> class</returns>
    public static ProcessSettings CreateForRaspistill()
    {
        var settings = new ProcessSettings();
        settings.Filename = RaspiStill;
        return settings;
    }

    /// <summary>
    /// Creates a ProcessSettings instance targeting raspivid.
    /// </summary>
    /// <returns>An instance of the <see cref="ProcessSettings"/> class</returns>
    public static ProcessSettings CreateForRaspivid()
    {
        var settings = new ProcessSettings();
        settings.Filename = RaspiVid;
        return settings;
    }

    /// <summary>
    /// Creates a ProcessSettings instance targeting libcamera-still and capturing stderr.
    /// </summary>
    /// <returns>An instance of the <see cref="ProcessSettings"/> class</returns>
    public static ProcessSettings CreateForLibcamerastillAndStderr()
    {
        var settings = new ProcessSettings();
        settings.Filename = LibcameraStill;
        settings.CaptureStderrInsteadOfStdout = true;
        return settings;
    }

    /// <summary>
    /// Creates a ProcessSettings instance targeting libcamera-still.
    /// </summary>
    /// <returns>An instance of the <see cref="ProcessSettings"/> class</returns>
    public static ProcessSettings CreateForLibcamerastill()
    {
        var settings = new ProcessSettings();
        settings.Filename = LibcameraStill;
        return settings;
    }

    /// <summary>
    /// Creates a ProcessSettings instance targeting raspivid.
    /// </summary>
    /// <returns>An instance of the <see cref="ProcessSettings"/> class</returns>
    public static ProcessSettings CreateForLibcameravid()
    {
        var settings = new ProcessSettings();
        settings.Filename = LibcameraVid;
        return settings;
    }
}
