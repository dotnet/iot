// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.Gpio.Drivers.Libgpiod.V1;
using System.Device.Gpio.Drivers.Libgpiod.V2;
using System.Device.Gpio.Libgpiod;
using System.Device.Gpio.Libgpiod.V2;
using System.IO;
using System.Linq;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// Driver factory for different versions of libgpiod.
/// </summary>
internal static class LibGpiodDriverFactory
{
    private const string DriverVersionEnvVar = "DOTNET_IOT_LIBGPIOD_DRIVER_VERSION";
    private const string LibrarySearchPattern = "libgpiod.so*";

    private static readonly Dictionary<string, LibGpiodDriverVersion> _libraryToDriverVersionMap = new()
    {
        { "libgpiod.so.0", LibGpiodDriverVersion.V1 },
        { "libgpiod.so.1", LibGpiodDriverVersion.V1 },
        { "libgpiod.so.2", LibGpiodDriverVersion.V1 },
        { "libgpiod.so.3", LibGpiodDriverVersion.V2 }
    };

    private static readonly Dictionary<LibGpiodDriverVersion, string[]> _driverVersionToLibrariesMap = new()
    {
        { LibGpiodDriverVersion.V1, new[] { "libgpiod.so.0", "libgpiod.so.1", "libgpiod.so.2" } },
        { LibGpiodDriverVersion.V2, new[] { "libgpiod.so.3" } }
    };

    private static readonly string? _driverVersionEnvVarValue;
    private static readonly LibGpiodDriverVersion? _driverVersionSetByEnvVar;

    /// <remarks>
    /// Driver version that is picked by this factory when no version is explicitly specified. Null when libpgiod is not installed.
    /// </remarks>
    private static readonly LibGpiodDriverVersion? _automaticallySelectedDriverVersion;

    private static readonly string[] _librarySearchPaths = { "/lib", "/usr/lib", "/usr/local/lib" }; // Based on Linux FHS standard

    public static readonly LibGpiodDriverVersion[] DriverCandidates;

    static LibGpiodDriverFactory()
    {
        var installedLibraries = GetInstalledLibraries();

        DriverCandidates = installedLibraries.Where(installedVersion => _libraryToDriverVersionMap.ContainsKey(installedVersion))
                                              .Select(installedVersion => _libraryToDriverVersionMap[installedVersion]).ToArray();

        _automaticallySelectedDriverVersion = DriverCandidates.Any() ? DriverCandidates.Max() : null;

        _driverVersionEnvVarValue = Environment.GetEnvironmentVariable(DriverVersionEnvVar);

        if (_driverVersionEnvVarValue != null)
        {
            if (_driverVersionEnvVarValue == LibGpiodDriverVersion.V1.ToString())
            {
                _driverVersionSetByEnvVar = LibGpiodDriverVersion.V1;
            }
            else if (_driverVersionEnvVarValue == LibGpiodDriverVersion.V2.ToString())
            {
                _driverVersionSetByEnvVar = LibGpiodDriverVersion.V2;
            }
        }
    }

    public static VersionedLibgpiodDriver Create(int chipNumber)
    {
        if (_driverVersionEnvVarValue != null)
        {
            if (_driverVersionSetByEnvVar == null)
            {
                throw new GpiodException($"Can not create libgpiod driver due to invalid specified value in environment variable" +
                    $" {DriverVersionEnvVar}: '{_driverVersionEnvVarValue}'" +
                    $". Valid values: {string.Join(", ", _libraryToDriverVersionMap.Values.Distinct())}");
            }

            var version = _driverVersionSetByEnvVar.Value;
            var driver = CreateInternal(_driverVersionSetByEnvVar.Value, chipNumber);

            return new VersionedLibgpiodDriver(version, driver);
        }

        return CreateAutomaticallyChosenDriver(chipNumber);
    }

    public static VersionedLibgpiodDriver Create(int chipNumber, LibGpiodDriverVersion driverVersion)
    {
        return new VersionedLibgpiodDriver(driverVersion, CreateInternal(driverVersion, chipNumber));
    }

    private static VersionedLibgpiodDriver CreateAutomaticallyChosenDriver(int chipNumber)
    {
        if (_automaticallySelectedDriverVersion == null)
        {
            throw new GpiodException($"No supported libgpiod library file found.\n" +
                $"Supported versions: {string.Join(", ", _libraryToDriverVersionMap.Keys)}\n" +
                $"Searched paths: {string.Join(", ", _librarySearchPaths)}");
        }

        var version = _automaticallySelectedDriverVersion.Value;
        var driver = CreateInternal(_automaticallySelectedDriverVersion.Value, chipNumber);

        return new VersionedLibgpiodDriver(version, driver);
    }

    private static GpioDriver CreateInternal(LibGpiodDriverVersion version, int chipNumber)
    {
        if (!DriverCandidates.Contains(version))
        {
            throw new GpiodException($"No suitable libgpiod library found for {nameof(LibGpiodDriverVersion)}.{version}. " +
                $"Supported versions: {string.Join(", ", _driverVersionToLibrariesMap[version])}\n" +
                $"Searched paths: {string.Join(", ", _librarySearchPaths)}");
        }

        return version switch
        {
            LibGpiodDriverVersion.V1 => new LibGpiodV1Driver(chipNumber),
            LibGpiodDriverVersion.V2 => new LibGpiodV2Driver(LibGpiodProxyFactory.CreateChip(chipNumber)),
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };
    }

    private static IEnumerable<string> GetInstalledLibraries()
    {
        HashSet<string> foundLibrariesFileName = new();

        foreach (string searchPath in _librarySearchPaths)
        {
            if (!Directory.Exists(searchPath))
            {
                continue;
            }

            foundLibrariesFileName.UnionWith(Directory.GetFiles(searchPath, LibrarySearchPattern));
        }

        HashSet<string> supportedLibraryVersions = new();

        foreach (string libraryFileName in foundLibrariesFileName)
        {
            foreach (string knownLibraryName in _libraryToDriverVersionMap.Keys)
            {
                if (libraryFileName.Contains(knownLibraryName))
                {
                    supportedLibraryVersions.Add(knownLibraryName);
                    break;
                }
            }
        }

        return supportedLibraryVersions;
    }

    public sealed record VersionedLibgpiodDriver(LibGpiodDriverVersion DriverVersion, GpioDriver LibGpiodDriver);
}
