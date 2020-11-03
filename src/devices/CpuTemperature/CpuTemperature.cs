// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Iot.Device.HardwareMonitor;
using UnitsNet;

namespace Iot.Device.CpuTemperature
{
    /// <summary>
    /// CPU temperature.
    /// On Windows, the value returned is driver dependent and may not represent actual CPU temperature, but more one
    /// of the case sensors. Use OpenHardwareMonitor for better environmental representation in Windows.
    /// </summary>
    public sealed class CpuTemperature : IDisposable
    {
        private bool _isAvailable;
        private bool _checkedIfAvailable;
        private bool _windows;
        private List<ManagementObjectSearcher> _managementObjectSearchers;
        private OpenHardwareMonitor _hardwareMonitorInUse;

        /// <summary>
        /// Creates an instance of the CpuTemperature class
        /// </summary>
        public CpuTemperature()
        {
            _isAvailable = false;
            _checkedIfAvailable = false;
            _windows = false;
            _managementObjectSearchers = new List<ManagementObjectSearcher>();
            _hardwareMonitorInUse = null;

            CheckAvailable();
        }

        /// <summary>
        /// Gets CPU temperature
        /// </summary>
        public Temperature Temperature
        {
            get
            {
                if (!_windows)
                {
                    return Temperature.FromDegreesCelsius(ReadTemperatureUnix());
                }
                else
                {
                    List<(string, Temperature)> tempList = ReadTemperatures();
                    return tempList.FirstOrDefault().Item2;
                }
            }
        }

        /// <summary>
        /// Is CPU temperature available
        /// </summary>
        public bool IsAvailable => _isAvailable;

        private bool CheckAvailable()
        {
            if (!_checkedIfAvailable)
            {
                _checkedIfAvailable = true;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && File.Exists("/sys/class/thermal/thermal_zone0/temp"))
                {
                    _isAvailable = true;
                    _windows = false;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    OpenHardwareMonitor ohw = new OpenHardwareMonitor();
                    if (ohw.TryGetAverageCpuTemperature(out _))
                    {
                        _windows = true;
                        _isAvailable = true;
                        _hardwareMonitorInUse = ohw;
                        return true;
                    }

                    try
                    {
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                        if (searcher.Get().Count > 0)
                        {
                            _managementObjectSearchers.Add(searcher);
                            _isAvailable = true;
                            _windows = true;
                        }
                    }
                    catch (Exception x) when (x is IOException || x is UnauthorizedAccessException || x is ManagementException)
                    {
                        // Nothing to do - WMI not available for this element or missing permissions.
                        // WMI enumeration may require elevated rights.
                    }

                    try
                    {
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM Win32_TemperatureProbe");
                        if (searcher.Get().Count > 0)
                        {
                            _managementObjectSearchers.Add(searcher);
                            _isAvailable = true;
                            _windows = true;
                        }
                    }
                    catch (Exception x) when (x is IOException || x is UnauthorizedAccessException || x is ManagementException)
                    {
                        // Nothing to do - WMI not available for this element or missing permissions.
                        // WMI enumeration may require elevated rights.
                    }

                }
            }

            return _isAvailable;
        }

        /// <summary>
        /// Returns all known temperature sensor values.
        /// </summary>
        /// <returns>A list of name/value pairs for temperature sensors</returns>
        public List<(string, Temperature)> ReadTemperatures()
        {
            if (!_windows)
            {
                var ret = new List<(string, Temperature)>();
                ret.Add(("CPU", Temperature.FromDegreesCelsius(ReadTemperatureUnix())));
                return ret;
            }

            // Windows code below
            List<(string, Temperature)> result = new List<(string, Temperature)>();

            if (_hardwareMonitorInUse != null)
            {
                if (_hardwareMonitorInUse.TryGetAverageCpuTemperature(out Temperature temp))
                {
                    result.Add(("CPU", temp));
                }

                return result;
            }

            foreach (var searcher in _managementObjectSearchers)
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    Double temp = Convert.ToDouble(string.Format(CultureInfo.InvariantCulture, "{0}", obj["CurrentTemperature"]), CultureInfo.InvariantCulture);
                    temp = (temp - 2732) / 10.0;
                    result.Add((obj["InstanceName"].ToString(), Temperature.FromDegreesCelsius(temp)));
                }
            }

            return result;
        }

        private double ReadTemperatureUnix()
        {
            double temperature = double.NaN;

            if (CheckAvailable())
            {
                using FileStream fileStream = new FileStream("/sys/class/thermal/thermal_zone0/temp", FileMode.Open, FileAccess.Read);
                if (fileStream is null)
                {
                    throw new Exception("Cannot read CPU temperature");
                }

                using StreamReader reader = new StreamReader(fileStream);
                string? data = reader.ReadLine();
                if (data is { Length: > 0 } &&
                    int.TryParse(data, out int temp))
                {
                    temperature = temp / 1000F;
                }
            }

            return temperature;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_hardwareMonitorInUse != null)
            {
                _hardwareMonitorInUse.Dispose();
                _hardwareMonitorInUse = null;
            }

            foreach (var elem in _managementObjectSearchers)
            {
                elem.Dispose();
            }

            _managementObjectSearchers.Clear();

            // Any further calls will fail
            _isAvailable = false;
        }
    }
}
