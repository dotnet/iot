// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
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
    /// The process name of the rpicam application used to capture still pictures on the Raspbian OS.
    /// This is the new name for the <see cref="LibcameraStill"/> application introduced with Raspberry Pi OS Bookworm.
    /// </summary>
    public const string RpicamStill = "rpicam-still";

    /// <summary>
    /// The process name of the rpicam application used to capture video streams on the Raspbian OS.
    /// This is the new name for the <see cref="LibcameraVid"/> application introduced with Raspberry Pi OS Bookworm.
    /// </summary>
    public const string RpicamVid = "rpicam-vid";

    /// <summary>
    /// Returns true when the new rpicam-apps (rpicam-still / rpicam-vid) are available on the system path.
    /// Starting with Raspberry Pi OS Bookworm the libcamera-* applications have been renamed to rpicam-*.
    /// </summary>
    /// <returns>True if the rpicam-apps are installed, otherwise false.</returns>
    public static bool IsRpicamAppsInstalled()
        => IsRpicamAppsInstalled(IsApplicationOnPath);

    internal static bool IsRpicamAppsInstalled(Func<string, bool> applicationExists)
        => applicationExists(RpicamStill) || applicationExists(RpicamVid);

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

    /// <summary>
    /// Creates a ProcessSettings instance targeting rpicam-still and capturing stderr.
    /// </summary>
    /// <returns>An instance of the <see cref="ProcessSettings"/> class</returns>
    public static ProcessSettings CreateForRpicamstillAndStderr()
    {
        var settings = new ProcessSettings();
        settings.Filename = RpicamStill;
        settings.CaptureStderrInsteadOfStdout = true;
        return settings;
    }

    /// <summary>
    /// Creates a ProcessSettings instance targeting rpicam-still.
    /// </summary>
    /// <returns>An instance of the <see cref="ProcessSettings"/> class</returns>
    public static ProcessSettings CreateForRpicamstill()
    {
        var settings = new ProcessSettings();
        settings.Filename = RpicamStill;
        return settings;
    }

    /// <summary>
    /// Creates a ProcessSettings instance targeting rpicam-vid.
    /// </summary>
    /// <returns>An instance of the <see cref="ProcessSettings"/> class</returns>
    public static ProcessSettings CreateForRpicamvid()
    {
        var settings = new ProcessSettings();
        settings.Filename = RpicamVid;
        return settings;
    }

    /// <summary>
    /// Creates a ProcessSettings instance for capturing still pictures using the libcamera/rpicam stack
    /// and capturing stderr. The new rpicam-still application is used when available, otherwise the
    /// legacy libcamera-still name is used (which remains available as a symbolic link on Bookworm).
    /// </summary>
    /// <returns>An instance of the <see cref="ProcessSettings"/> class</returns>
    public static ProcessSettings CreateForStillAndStderr()
        => IsApplicationOnPath(RpicamStill) ? CreateForRpicamstillAndStderr() : CreateForLibcamerastillAndStderr();

    /// <summary>
    /// Creates a ProcessSettings instance for capturing still pictures using the libcamera/rpicam stack.
    /// The new rpicam-still application is used when available, otherwise the legacy libcamera-still name
    /// is used (which remains available as a symbolic link on Bookworm).
    /// </summary>
    /// <returns>An instance of the <see cref="ProcessSettings"/> class</returns>
    public static ProcessSettings CreateForStill()
        => IsApplicationOnPath(RpicamStill) ? CreateForRpicamstill() : CreateForLibcamerastill();

    /// <summary>
    /// Creates a ProcessSettings instance for capturing video streams using the libcamera/rpicam stack.
    /// The new rpicam-vid application is used when available, otherwise the legacy libcamera-vid name
    /// is used (which remains available as a symbolic link on Bookworm).
    /// </summary>
    /// <returns>An instance of the <see cref="ProcessSettings"/> class</returns>
    public static ProcessSettings CreateForVid()
        => IsApplicationOnPath(RpicamVid) ? CreateForRpicamvid() : CreateForLibcameravid();

    private static bool IsApplicationOnPath(string fileName)
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
        {
            return false;
        }

        foreach (var directory in pathVariable.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            try
            {
                if (File.Exists(Path.Combine(directory.Trim(), fileName)))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // Ignore invalid or inaccessible entries in the PATH variable
            }
        }

        return false;
    }
}
