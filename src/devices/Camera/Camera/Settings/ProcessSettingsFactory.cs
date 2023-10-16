// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// A factory for the ProcessSettings
/// </summary>
public static class ProcessSettingsFactory
{
    /// <summary>
    /// Create a ProcessSettings instance targeting raspistill
    /// </summary>
    /// <returns></returns>
    public static ProcessSettings CreateForRaspistill()
    {
        var settings = new ProcessSettings();
        settings.Filename = "raspistill";
        return settings;
    }

    /// <summary>
    /// Create a ProcessSettings instance targeting raspivid
    /// </summary>
    /// <returns></returns>
    public static ProcessSettings CreateForRaspivid()
    {
        var settings = new ProcessSettings();
        settings.Filename = "raspivid";
        return settings;
    }

    /// <summary>
    /// Create a ProcessSettings instance targeting libcamera-still
    /// </summary>
    /// <returns></returns>
    public static ProcessSettings CreateForLibcamerastill()
    {
        var settings = new ProcessSettings();
        settings.Filename = "libcamera-still";
        return settings;
    }

    /// <summary>
    /// Create a ProcessSettings instance targeting raspivid
    /// </summary>
    /// <returns></returns>
    public static ProcessSettings CreateForLibcameravid()
    {
        var settings = new ProcessSettings();
        settings.Filename = "libcamera-vid";
        return settings;
    }
}
