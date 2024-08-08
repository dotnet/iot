// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.Gpio.Drivers.Libgpiod.V1;
using System.Device.Gpio.Drivers.Libgpiod.V2;
using System.Device.Gpio.Libgpiod.V2;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// Driver factory for different versions of libgpiod.
/// </summary>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal sealed class LibGpiodDriverFactory
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

    private static readonly string[] _librarySearchPaths = { "/lib", "/usr/lib", "/usr/local/lib" }; // Based on Linux FHS standard

    private static readonly object _initLock = new();
    private static LibGpiodDriverFactory? _instance;

    /// <summary>
    /// The value set by DOTNET_IOT_LIBGPIOD_DRIVER_VERSION. Null when not set.
    /// </summary>
    private readonly string? _driverVersionEnvVarValue;

    /// <summary>
    /// The driver version that was resolved based on the value of DOTNET_IOT_LIBGPIOD_DRIVER_VERSION. Null when env var not set.
    /// </summary>
    private readonly LibGpiodDriverVersion? _driverVersionSetByEnvVar;

    /// <remarks>
    /// Driver version that is picked by this factory when no version is explicitly specified. Null when libpgiod is not installed.
    /// </remarks>
    private readonly LibGpiodDriverVersion? _automaticallySelectedDriverVersion;

    private readonly LibGpiodDriverVersion[] _driverCandidates;

    /// <summary>
    /// Collection of installed libgpiod libraries (their file name).
    /// </summary>
    private readonly IEnumerable<string> _installedLibraries;

    private LibGpiodDriverFactory()
    {
        _installedLibraries = GetInstalledLibraries();
        _driverCandidates = GetDriverCandidates();
        _automaticallySelectedDriverVersion = GetAutomaticallySelectedDriverVersion();
        _driverVersionEnvVarValue = GetDriverVersionEnvVarValue();
        _driverVersionSetByEnvVar = GetDriverVersionSetByEnvVar();
    }

    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static LibGpiodDriverFactory Instance
    {
        get
        {
            lock (_initLock)
            {
                _instance ??= new LibGpiodDriverFactory();
            }

            return _instance;
        }
    }

    /// <summary>
    /// A collection of driver versions that correspond to the installed versions of libgpiod on this system. Each driver is dependent
    /// on specific libgpiod version/s. If the collection is empty, it indicates that libgpiod might not be installed or could not be detected.
    /// </summary>
    public LibGpiodDriverVersion[] DriverCandidates => _driverCandidates;

    public VersionedLibgpiodDriver Create(int chipNumber)
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

    public VersionedLibgpiodDriver Create(int chipNumber, LibGpiodDriverVersion driverVersion)
    {
        return new VersionedLibgpiodDriver(driverVersion, CreateInternal(driverVersion, chipNumber));
    }

    private static IEnumerable<string> GetInstalledLibraries()
    {
        HashSet<string> foundLibrariesFileName = new();

        foreach (string searchPath in _librarySearchPaths)
        {
            var files = GetFiles(new DirectoryInfo(searchPath), LibrarySearchPattern);
            var fileNames = files.Select(file => file.Name);
            foundLibrariesFileName.UnionWith(fileNames);
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

    private static IEnumerable<FileInfo> GetFiles(DirectoryInfo rootDirectory, string searchPattern)
    {
        List<FileInfo> files = new();
        Stack<DirectoryInfo> directoriesToProcess = new();

        directoriesToProcess.Push(rootDirectory);

        while (directoriesToProcess.Count > 0)
        {
            DirectoryInfo currentDirectory = directoriesToProcess.Pop();

            try
            {
                if (currentDirectory.Exists)
                {
                    files.AddRange(currentDirectory.GetFiles(searchPattern));

                    DirectoryInfo[] subdirectories = currentDirectory.GetDirectories();
                    foreach (var subdirectory in subdirectories)
                    {
                        var depth = subdirectory.FullName.Count(separator => separator == Path.DirectorySeparatorChar);
                        if (depth < 10)
                        {
                            directoriesToProcess.Push(subdirectory);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // unauthorized access or any other exception is only to be skipped and not to be handled
            }
        }

        return files;
    }

    private VersionedLibgpiodDriver CreateAutomaticallyChosenDriver(int chipNumber)
    {
        if (_automaticallySelectedDriverVersion == null)
        {
            throw new GpiodException($"No supported libgpiod library file found.\n" +
                $"Supported library files: {string.Join(", ", _libraryToDriverVersionMap.Keys)}\n" +
                $"Searched paths: {string.Join(", ", _librarySearchPaths)}");
        }

        var version = _automaticallySelectedDriverVersion.Value;
        var driver = CreateInternal(_automaticallySelectedDriverVersion.Value, chipNumber);

        return new VersionedLibgpiodDriver(version, driver);
    }

    private GpioDriver CreateInternal(LibGpiodDriverVersion version, int chipNumber)
    {
        if (!DriverCandidates.Contains(version))
        {
            string installedLibraryFiles = _installedLibraries.Any() ? string.Join(", ", _installedLibraries) : "None";
            throw new GpiodException($"No suitable libgpiod library file found for {nameof(LibGpiodDriverVersion)}.{version} " +
                $"which requires one of: {string.Join(", ", _driverVersionToLibrariesMap[version])}\n" +
                $"Installed library files: {installedLibraryFiles}\n" + $"Searched paths: {string.Join(", ", _librarySearchPaths)}");
        }

        return version switch
        {
            LibGpiodDriverVersion.V1 => new LibGpiodV1Driver(chipNumber),
            LibGpiodDriverVersion.V2 => new LibGpiodV2Driver(LibGpiodProxyFactory.CreateChip(chipNumber)),
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };
    }

    private LibGpiodDriverVersion[] GetDriverCandidates()
    {
        return _installedLibraries.Where(installedVersion => _libraryToDriverVersionMap.ContainsKey(installedVersion))
                                  .Select(installedVersion => _libraryToDriverVersionMap[installedVersion]).ToArray();
    }

    private LibGpiodDriverVersion? GetAutomaticallySelectedDriverVersion()
    {
        return DriverCandidates.Any() ? DriverCandidates.Max() : null;
    }

    private string? GetDriverVersionEnvVarValue()
    {
        return Environment.GetEnvironmentVariable(DriverVersionEnvVar);
    }

    private LibGpiodDriverVersion? GetDriverVersionSetByEnvVar()
    {
        if (_driverVersionEnvVarValue != null)
        {
            if (_driverVersionEnvVarValue == LibGpiodDriverVersion.V1.ToString())
            {
                return LibGpiodDriverVersion.V1;
            }

            if (_driverVersionEnvVarValue == LibGpiodDriverVersion.V2.ToString())
            {
                return LibGpiodDriverVersion.V2;
            }
        }

        return null;
    }

    public sealed record VersionedLibgpiodDriver(LibGpiodDriverVersion DriverVersion, GpioDriver LibGpiodDriver);
}
