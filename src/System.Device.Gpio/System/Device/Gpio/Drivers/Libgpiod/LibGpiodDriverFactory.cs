// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.Gpio.System.Device.Gpio.Drivers.Libgpiod.V1;
using System.Device.Gpio.System.Device.Gpio.Drivers.Libgpiod.V2;
using System.IO;
using System.Linq;

namespace System.Device.Gpio.System.Device.Gpio.Drivers.Libgpiod;

internal static class LibGpiodDriverFactory
{
    private const int _defaultDriverVersion = 1;

    private const string _librarySearchPattern = "libgpiod.so*";
    private const string _v0 = "libgpiod.so.0";
    private const string _v1_0 = "libgpiod.so.1";
    private const string _v1_1 = "libgpiod.so.2";
    private const string _v2 = "libgpiod.so.3";

    private static readonly string[] _librarySearchPaths = { "/lib", "/usr/lib", "/usr/local/lib" };

    private static readonly object _lockObject = new();
    private static int? _driverVersionToLoad;

    /// <summary>
    /// Tries with best effort to find installed libgpiod libraries and loads the latest version.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/standard/native-interop/native-library-loading#custom-import-resolver"/>
    /// </summary>
    /// <remarks> On NET Core 3.1 or NET5+ SetDllImportResolver would be a better solution</remarks>
    public static GpioDriver Create(int chipNumber)
    {
        lock (_lockObject)
        {
            if (_driverVersionToLoad.HasValue)
            {
                return InstantiateDriver(_driverVersionToLoad.Value, chipNumber);
            }

            _driverVersionToLoad = GetDriverVersionToLoad();
            return InstantiateDriver(_driverVersionToLoad.Value, chipNumber);
        }
    }

    private static GpioDriver InstantiateDriver(int driverVersionToLoad, int chipNumber)
    {
        return driverVersionToLoad switch
        {
            1 => new LibGpiodV1Driver(chipNumber),
            2 => new LibGpiodV2Driver(chipNumber),
            _ => throw new ArgumentOutOfRangeException(nameof(driverVersionToLoad), $"Unexpected libgpiod driver version: '{driverVersionToLoad}'")
        };
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

        if (foundLibraryFiles.Any(x => x.Contains(_v2)))
        {
            return 2;
        }

        if (foundLibraryFiles.Any(x => x.Contains(_v1_1)))
        {
            return 1;
        }

        if (foundLibraryFiles.Any(x => x.Contains(_v1_0)))
        {
            return 1;
        }

        if (foundLibraryFiles.Any(x => x.Contains(_v0)))
        {
            return 1;
        }

        return _defaultDriverVersion;
    }
}
