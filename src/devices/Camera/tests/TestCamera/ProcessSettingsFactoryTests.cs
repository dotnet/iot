// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

using Iot.Device.Camera.Settings;

namespace TestCamera;

/// <summary>
/// Tests for the ProcessSettingsFactory class, in particular the migration
/// from the libcamera-* applications to the new rpicam-* applications.
/// </summary>
public class ProcessSettingsFactoryTests
{
    /// <summary>
    /// The rpicam-* constants must use the new application naming.
    /// </summary>
    [Fact]
    public void RpicamConstantsUseTheNewNaming()
    {
        Assert.Equal("rpicam-still", ProcessSettingsFactory.RpicamStill);
        Assert.Equal("rpicam-vid", ProcessSettingsFactory.RpicamVid);
    }

    /// <summary>
    /// The rpicam factory methods must target the new application names.
    /// </summary>
    [Fact]
    public void RpicamFactoryMethodsTargetTheNewApplications()
    {
        Assert.Equal(ProcessSettingsFactory.RpicamStill, ProcessSettingsFactory.CreateForRpicamstill().Filename);
        Assert.Equal(ProcessSettingsFactory.RpicamVid, ProcessSettingsFactory.CreateForRpicamvid().Filename);

        var stderr = ProcessSettingsFactory.CreateForRpicamstillAndStderr();
        Assert.Equal(ProcessSettingsFactory.RpicamStill, stderr.Filename);
        Assert.True(stderr.CaptureStderrInsteadOfStdout);
    }

    /// <summary>
    /// When the rpicam-apps are installed, detection must report them as available.
    /// </summary>
    [Fact]
    public void IsRpicamAppsInstalledDetectsTheNewApplications()
    {
        Assert.True(ProcessSettingsFactory.IsRpicamAppsInstalled(
            name => name == ProcessSettingsFactory.RpicamStill));
        Assert.True(ProcessSettingsFactory.IsRpicamAppsInstalled(
            name => name == ProcessSettingsFactory.RpicamVid));
        Assert.False(ProcessSettingsFactory.IsRpicamAppsInstalled(_ => false));
    }

    /// <summary>
    /// The auto-detecting factory methods must prefer the rpicam-* applications
    /// when they are available, otherwise fall back to the libcamera-* names.
    /// </summary>
    [Fact]
    public void AutoFactoryMethodsPreferRpicamWhenAvailable()
    {
        // The auto methods rely on detection through the system PATH. The
        // test agents do not ship the rpicam-apps, so the libcamera-* names
        // are expected as the backward compatible fallback.
        Assert.Equal(ProcessSettingsFactory.LibcameraStill, ProcessSettingsFactory.CreateForStill().Filename);
        Assert.Equal(ProcessSettingsFactory.LibcameraVid, ProcessSettingsFactory.CreateForVid().Filename);

        var stderr = ProcessSettingsFactory.CreateForStillAndStderr();
        Assert.Equal(ProcessSettingsFactory.LibcameraStill, stderr.Filename);
        Assert.True(stderr.CaptureStderrInsteadOfStdout);
    }
}
