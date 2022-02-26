// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnitsNet;
using UnitsNet.Units;

// We have a check on all constructors to ensure that we throw when not on Windows, so disabling warning on calling windows-only apis.
#pragma warning disable CA1416 // Validate platform compatibility

namespace Iot.Device.HardwareMonitor
{
    /// <summary>
    /// This class connects to a running instance of OpenHardwareMonitor and reads out all available values using WMI.
    /// This works only if OpenHardwareMonitor (https://openhardwaremonitor.org/) is currently running.
    /// While the tool needs to be run with elevated permissions, the application using this binding does not.
    /// The connection using WMI has proven to be unreliable (the WMI classes are sometimes just not visible), therefore
    /// OHM versions greater > 0.10.0 use a rest api instead.
    /// </summary>
    internal sealed class OpenHardwareMonitorWmi : IDisposable, IOpenHardwareMonitorInternal
    {
        private OpenHardwareMonitor.Hardware? _cpu;

        /// <summary>
        /// Constructs a new instance of this class.
        /// The class can be constructed even if no sensors are available or OpenHardwareMonitor is not running (yet).
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">The operating system is not Windows.</exception>
        public OpenHardwareMonitorWmi()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("This class is only supported on Windows operating systems");
            }

            InitHardwareMonitor();
        }

        public SensorUpdateStrategy UpdateStrategy { get; set; }

        public TimeSpan UpdateInterval { get; set; }

        public void UpdateSensors(bool refreshSensorList)
        {
            if (refreshSensorList)
            {
                _cpu = null;
                InitHardwareMonitor();
            }
        }

        public bool HasHardware()
        {
            return _cpu != null;
        }

        private void InitHardwareMonitor()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "SELECT * FROM Sensor");
                foreach (var hardware in GetHardwareComponents())
                {
                    if (hardware.Type != null && hardware.Type.Equals("CPU", StringComparison.OrdinalIgnoreCase))
                    {
                        _cpu = hardware;
                    }
                }

                searcher.Dispose();
            }
            catch (Exception x) when (x is IOException || x is UnauthorizedAccessException || x is ManagementException)
            {
                // Nothing to do - WMI not available for this element or missing permissions.
                // WMI enumeration may require elevated rights.
            }
        }

        /// <summary>
        /// Query the list of all available sensors.
        /// </summary>
        /// <returns>A list of <see cref="OpenHardwareMonitor.Sensor"/> instances. May be empty.</returns>
        /// <exception cref="ManagementException">The WMI objects required are not available. Is OpenHardwareMonitor running?</exception>
        public IList<OpenHardwareMonitor.Sensor> GetSensorList()
        {
            List<OpenHardwareMonitor.Sensor> ret = new List<OpenHardwareMonitor.Sensor>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "SELECT * FROM Sensor");
            foreach (ManagementObject sensor in searcher.Get())
            {
                string? name = Convert.ToString(sensor["Name"]);
                string? identifier = Convert.ToString(sensor["Identifier"]);

                // This is not expected to really happen
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(identifier))
                {
                    continue;
                }

                string? parent = Convert.ToString(sensor["Parent"]);
                string? type = Convert.ToString(sensor["SensorType"]);
                SensorType typeEnum;
                if (!Enum.TryParse(type, true, out typeEnum))
                {
                    typeEnum = SensorType.Unknown;
                }

                ret.Add(new SensorWmi(sensor, name, identifier, parent, typeEnum));
            }

            return ret;
        }

        /// <summary>
        /// Returns a list of hardware components, such as "CPU", "GPU" or "Mainboard"
        /// </summary>
        public IList<OpenHardwareMonitor.Hardware> GetHardwareComponents()
        {
            IList<OpenHardwareMonitor.Hardware> ret = new List<OpenHardwareMonitor.Hardware>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "SELECT * FROM Hardware");
            if (searcher.Get().Count > 0)
            {
                foreach (ManagementObject sensor in searcher.Get())
                {
                    string? name = Convert.ToString(sensor["Name"]);
                    string? identifier = Convert.ToString(sensor["Identifier"]);
                    if (name == null || identifier == null)
                    {
                        continue;
                    }

                    string? parent = Convert.ToString(sensor["Parent"]);
                    string? type = Convert.ToString(sensor["HardwareType"]);
                    ret.Add(new OpenHardwareMonitor.Hardware(name, identifier, parent, type));
                }
            }

            return ret;
        }

        /// <summary>
        /// Get the list of sensors for a specific piece of hardware
        /// </summary>
        /// <param name="forHardware">The module that should be queried</param>
        /// <returns>A list of sensors</returns>
        public IEnumerable<OpenHardwareMonitor.Sensor> GetSensorList(OpenHardwareMonitor.Hardware? forHardware)
        {
            if (forHardware == null)
            {
                throw new ArgumentNullException(nameof(forHardware));
            }

            return GetSensorList().Where(x => x.Identifier != null && x.Identifier.StartsWith(forHardware.Identifier ?? string.Empty)).OrderBy(y => y.Identifier);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _cpu = null;
        }

        /// <summary>
        /// Represents a single Wmi sensor
        /// </summary>
        public class SensorWmi : OpenHardwareMonitor.Sensor, IDisposable
        {
            private readonly ManagementObject _instance;
            private bool _valueRead;

            /// <summary>
            /// Creates a sensor instance
            /// </summary>
            public SensorWmi(ManagementObject instance, string name, string identifier, string? parent, SensorType typeEnum)
            : base(name, identifier, parent, typeEnum)
            {
                _instance = instance;
                InstanceId = 0;
                _valueRead = false;
                if (!string.IsNullOrWhiteSpace(instance.Path.RelativePath))
                {
                    int instanceBegin = instance.Path.RelativePath.IndexOf("InstanceId=\"", StringComparison.OrdinalIgnoreCase) + 12;
                    int instanceEnd = instance.Path.RelativePath.IndexOf('\"', instanceBegin);
                    if (Int32.TryParse(instance.Path.RelativePath.Substring(instanceBegin, instanceEnd - instanceBegin), out int id))
                    {
                        InstanceId = id;
                    }
                }
            }

            public int InstanceId { get; private set; }

            private ManagementObjectSearcher? ActiveCollection
            {
                get;
                set;
            }

            protected override bool UpdateValue(out double value)
            {
                if (_valueRead && InstanceId != 0)
                {
                    // Cache the searcher. We have to re-query the instances each time, or the value stays the same.
                    ActiveCollection = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", $"SELECT Value FROM Sensor WHERE InstanceId='{InstanceId}'");
                }

                if (_valueRead && InstanceId != 0 && ActiveCollection != null)
                {
                    value = 0;
                    // We expect exactly one instance, but unfortunately, the returned ManagementObjectCollection doesn't implement IEnumerable or IList
                    foreach (var inst in ActiveCollection.Get())
                    {
                        value = Convert.ToSingle(inst.GetPropertyValue("Value"));
                    }
                }
                else
                {
                    // The artificial instances auto-update. And if the object is new, we also don't need a refresh
                    value = Convert.ToSingle(_instance.GetPropertyValue("Value"));
                    _valueRead = true;
                }

                return true;
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                _instance.Dispose();
                if (ActiveCollection != null)
                {
                    ActiveCollection.Dispose();
                    ActiveCollection = null;
                }
            }
        }
    }
}

#pragma warning restore CA1416 // Validate platform compatibility
