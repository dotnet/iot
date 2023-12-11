// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.Gpio.Drivers.Libgpiod.V1;
using System.Device.Gpio.Drivers.Libgpiod.V2;
using System.Device.Gpio.Libgpiod.V2;
using System.IO;
using System.Linq;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// Driver factory for different versions of the libgpiod driver.
/// </summary>
public static class LibGpiodDriverFactory
{
    private const int _defaultDriverVersion = 1;

    private const string _librarySearchPattern = "libgpiod.so*";
    private const string _libgpiod_0 = "libgpiod.so.0";
    private const string _libgpiod_1_0 = "libgpiod.so.1";
    private const string _libgpiod_1_1 = "libgpiod.so.2";
    private const string _libgpiod_2 = "libgpiod.so.3";

    private static readonly string[] _librarySearchPaths = { "/lib", "/usr/lib", "/usr/local/lib" };

    private static readonly int _driverVersionToLoad;

    static LibGpiodDriverFactory()
    {
        try
        {
            _driverVersionToLoad = GetDriverVersionToLoad();
        }
        catch (Exception)
        {
            _driverVersionToLoad = 1;
        }
    }

    /// <summary>
    /// Tries with best effort to find installed libgpiod libraries and loads the latest version.
    /// </summary>
    public static UnixDriver Create(int chipNumber)
    {
        return InstantiateDriver(_driverVersionToLoad, chipNumber);
    }

    /// <summary>
    /// Creates and returns an instance of the libgpiodv1 driver.
    /// </summary>
    /// <param name="chipNumber">The number of the GPIO chip to drive.</param>
    public static UnixDriver CreateV1Driver(int chipNumber = 0)
    {
        return CreateLibgpiodV1DriverInstance(chipNumber);
    }

    /// <summary>
    /// Creates and returns an instance of the libgpiodv1 driver.
    /// </summary>
    /// <param name="chipNumber">The number of the GPIO chip to drive.</param>
    /// <param name="waitEdgeEventsTimeout">Timeout to wait for edge events. Primarily used for testing.</param>
    public static UnixDriver CreateV2Driver(int chipNumber, TimeSpan? waitEdgeEventsTimeout = null)
    {
        return CreateLibgpiodV2DriverInstance(chipNumber, waitEdgeEventsTimeout);
    }

    private static UnixDriver InstantiateDriver(int driverVersionToLoad, int chipNumber)
    {
        return driverVersionToLoad switch
        {
            1 => CreateLibgpiodV1DriverInstance(chipNumber),
            2 => CreateLibgpiodV2DriverInstance(chipNumber),
            _ => throw new ArgumentOutOfRangeException(nameof(driverVersionToLoad), $"Unexpected libgpiod driver version: '{driverVersionToLoad}'")
        };
    }

    private static LibGpiodV1Driver CreateLibgpiodV1DriverInstance(int chipNumber)
    {
        return new LibGpiodV1Driver(chipNumber);
    }

    private static LibGpiodV2Driver CreateLibgpiodV2DriverInstance(int chipNumber, TimeSpan? waitEdgeEventsTimeout = null)
    {
        Chip chip = LibGpiodProxyFactory.CreateChip(chipNumber);
        return new LibGpiodV2Driver(chip);
    }

    private static int GetDriverVersionToLoad()
    {
        HashSet<string> foundLibraryFiles = new();

        foreach (string searchPath in _librarySearchPaths)
        {
            if (!Directory.Exists(searchPath))
            {
                continue;
            }

            foundLibraryFiles.UnionWith(Directory.GetFiles(searchPath, _librarySearchPattern));
        }

        if (!foundLibraryFiles.Any())
        {
            return _defaultDriverVersion;
        }

        if (foundLibraryFiles.Any(x => x.Contains(_libgpiod_2)))
        {
            return 2;
        }

        if (foundLibraryFiles.Any(x => x.Contains(_libgpiod_1_1)))
        {
            return 1;
        }

        if (foundLibraryFiles.Any(x => x.Contains(_libgpiod_1_0)))
        {
            return 1;
        }

        if (foundLibraryFiles.Any(x => x.Contains(_libgpiod_0)))
        {
            return 1;
        }

        return _defaultDriverVersion;
    }
}
