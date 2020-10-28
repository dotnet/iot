using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnitsNet;

#pragma warning disable CS1591
namespace Iot.Device.OpenHardwareMonitor
{
    /// <summary>
    /// This class connects to a running instance of OpenHardwareMonitor and reads out all available values.
    /// This works only if OpenHardwareMonitor (https://openhardwaremonitor.org/) is currently running.
    /// While the tool needs to be run with elevated permissions, the application using this binding does not.
    /// </summary>
    public sealed class OpenHardwareMonitor : IDisposable
    {
        private static readonly TimeSpan MonitorInterval = TimeSpan.FromMilliseconds(100);

        private bool _isAvalable;

        private delegate IQuantity UnitCreator(float value);

        public delegate void OnNewValue(Sensor sensor, IQuantity value, TimeSpan timeSinceUpdate);

        private static Dictionary<SensorType, (Type Type, UnitCreator Creator)> _typeMap;
        private Hardware _cpu;
        private Hardware _gpu;

        private Thread _monitorThread;
        private object _lock;
        private List<MonitoringJob> _monitoredElements;
        private List<Sensor> _derivedSensors;
        private DateTimeOffset _lastMonitorLoop;

        static OpenHardwareMonitor()
        {
            _typeMap = new Dictionary<SensorType, (Type Type, UnitCreator Creator)>();
            _typeMap.Add(SensorType.Temperature, (typeof(Temperature), (x) => Temperature.FromDegreesCelsius(x)));
            _typeMap.Add(SensorType.Voltage, (typeof(ElectricPotential), x => ElectricPotential.FromVolts(x)));
            _typeMap.Add(SensorType.Load, (typeof(Ratio), x => Ratio.FromPercent(x)));
            _typeMap.Add(SensorType.Fan, (typeof(RotationalSpeed), x => RotationalSpeed.FromRevolutionsPerMinute(x)));
            _typeMap.Add(SensorType.Flow, (typeof(VolumeFlow), x => VolumeFlow.FromLitersPerHour(x)));
            _typeMap.Add(SensorType.Control, (typeof(Ratio), x => Ratio.FromPercent(x)));
            _typeMap.Add(SensorType.Level, (typeof(Ratio), x => Ratio.FromPercent(x)));
            _typeMap.Add(SensorType.Power, (typeof(Power), x => Power.FromWatts(x)));
            _typeMap.Add(SensorType.Clock, (typeof(Frequency), x => Frequency.FromMegahertz(x)));
            _typeMap.Add(SensorType.Energy, (typeof(Energy), x => Energy.FromWattHours(x)));
            _typeMap.Add(SensorType.HeatFlux, (typeof(HeatFlux), x => HeatFlux.FromKilowattsPerSquareMeter(x)));
            _typeMap.Add(SensorType.Current, (typeof(ElectricCurrent), x => ElectricCurrent.FromAmperes(x)));
        }

        public OpenHardwareMonitor()
        {
            _isAvalable = false;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("This class is only supported on Windows operating systems");
            }

            InitHardwareMonitor();

            _derivedSensors = new List<Sensor>();

            _monitorThread = null;
            _lock = new object();
            _monitoredElements = new List<MonitoringJob>();
            _lastMonitorLoop = DateTimeOffset.UtcNow;
        }

        private void InitHardwareMonitor()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "SELECT * FROM Sensor");
                if (searcher.Get().Count > 0)
                {
                    _isAvalable = true;
                    foreach (var hardware in GetHardwareComponents())
                    {
                        if (hardware.Type.Equals("CPU", StringComparison.OrdinalIgnoreCase))
                        {
                            _cpu = hardware;
                        }
                        else if (hardware.Type.StartsWith("GPU", StringComparison.OrdinalIgnoreCase))
                        {
                            _gpu = hardware;
                        }
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

        public bool IsAvailable => _isAvalable;

        public int LogicalProcessors
        {
            get
            {
                return Environment.ProcessorCount;
            }
        }

        /// <summary>
        /// Query the list of all available sensors.
        /// </summary>
        /// <returns>A list of <see cref="Sensor"/> instances. May be empty.</returns>
        /// <exception cref="ManagementException">The WMI objects required are not available. Is OpenHardwareMonitor running?</exception>
        public IList<Sensor> GetSensorList()
        {
            List<Sensor> ret = new List<Sensor>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "SELECT * FROM Sensor");
            if (searcher.Get().Count > 0)
            {
                foreach (ManagementObject sensor in searcher.Get())
                {
                    string name = Convert.ToString(sensor["Name"]);
                    string identifier = Convert.ToString(sensor["Identifier"]);
                    string parent = Convert.ToString(sensor["Parent"]);
                    string type = Convert.ToString(sensor["SensorType"]);
                    SensorType typeEnum;
                    if (!Enum.TryParse(type, true, out typeEnum))
                    {
                        typeEnum = SensorType.Unknown;
                    }

                    ret.Add(new Sensor(sensor, name, identifier, parent, typeEnum));
                }
            }

            ret.AddRange(_derivedSensors);

            return ret;
        }

        public IList<Hardware> GetHardwareComponents()
        {
            IList<Hardware> ret = new List<Hardware>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "SELECT * FROM Hardware");
            if (searcher.Get().Count > 0)
            {
                foreach (ManagementObject sensor in searcher.Get())
                {
                    string name = Convert.ToString(sensor["Name"]);
                    string identifier = Convert.ToString(sensor["Identifier"]);
                    string parent = Convert.ToString(sensor["Parent"]);
                    string type = Convert.ToString(sensor["HardwareType"]);
                    ret.Add(new Hardware(name, identifier, parent, type));
                }
            }

            return ret;
        }

        public IEnumerable<Sensor> GetSensorList(Hardware forHardware)
        {
            if (forHardware == null)
            {
                throw new ArgumentNullException(nameof(forHardware));
            }

            return GetSensorList().Where(x => x.Identifier.StartsWith(forHardware.Identifier)).OrderBy(y => y.Identifier);
        }

        // Some well-known properties have their own method

        /// <summary>
        /// Gets the average CPU temperature (averaged over all CPU sensors / cores)
        /// </summary>
        public bool TryGetAverageCpuTemperature(out Temperature temperature)
        {
            if (_cpu == null)
            {
                InitHardwareMonitor();
            }

            if (_cpu == null)
            {
                temperature = default;
                return false;
            }

            if (TryGetAverage(_cpu, out temperature))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the average CPU temperature (averaged over all CPU sensors / cores)
        /// </summary>
        /// <param name="temperature">The average GPU temperature</param>
        public bool TryGetAverageGpuTemperature(out Temperature temperature)
        {
            if (_gpu == null)
            {
                InitHardwareMonitor();
            }

            if (_gpu == null)
            {
                temperature = default;
                return false;
            }

            if (TryGetAverage(_gpu, out temperature))
            {
                return true;
            }

            return false;
        }

        public Ratio GetCpuLoad()
        {
            foreach (var s in GetSensorList(_cpu).OrderBy(x => x.Identifier))
            {
                if (s.SensorType == SensorType.Load && s.TryGetValue(out Ratio load))
                {
                    return load;
                }
            }

            return default(Ratio);
        }

        public bool TryGetAverage<T>(Hardware hardware, out T average)
            where T : IQuantity
        {
            double value = 0;
            int count = 0;
            Enum unitThatWasUsed = null;
            foreach (var s in GetSensorList(hardware))
            {
                if (s.TryGetValue(out T singleValue))
                {
                    if (unitThatWasUsed == null)
                    {
                        unitThatWasUsed = singleValue.Unit;
                    }
                    else if (!unitThatWasUsed.Equals(singleValue.Unit))
                    {
                        throw new NotSupportedException($"The different sensors for {hardware.Name} deliver values in different units");
                    }

                    value += singleValue.Value;
                    count++;
                }
            }

            if (count == 0)
            {
                average = default(T);
                return false;
            }

            value = value / count;

            average = (T)Quantity.From(value, unitThatWasUsed);
            return true;
        }

        /// <summary>
        /// Starts monitoring a sensor.
        /// This will internally start a thread that calls the provided action each time the TimeSpan elapses.
        /// </summary>
        /// <param name="sensorToMonitor">The sensor to monitor. The same sensor may be registered multiple times</param>
        /// <param name="monitoringInterval">The monitoring interval. Will be rounded to the next 0.1s.</param>
        /// <param name="onNewValue">Action to perform each time</param>
        /// <returns>An identifier for the monitoring job</returns>
        public MonitoringJob StartMonitoring(Sensor sensorToMonitor, TimeSpan monitoringInterval, OnNewValue onNewValue)
        {
            if (sensorToMonitor == null)
            {
                throw new ArgumentNullException(nameof(sensorToMonitor));
            }

            if (onNewValue == null)
            {
                throw new ArgumentNullException(nameof(onNewValue));
            }

            if (_monitorThread == null || _monitorThread.IsAlive == false)
            {
                _monitorThread = new Thread(MonitorThread);
                _monitorThread.IsBackground = true;
                _monitorThread.Start();
            }

            double roundedInterval = monitoringInterval.TotalSeconds;
            // round to the nearest 100ms
            roundedInterval = Math.Round(roundedInterval, 1, MidpointRounding.AwayFromZero);
            if (roundedInterval < 0.1)
            {
                roundedInterval = 0.1;
            }

            monitoringInterval = TimeSpan.FromSeconds(roundedInterval);
            lock (_lock)
            {
                MonitoringJob job = new MonitoringJob(sensorToMonitor, monitoringInterval, onNewValue);
                _monitoredElements.Add(job);
                return job;
            }
        }

        public void StopMonitoring(MonitoringJob job)
        {
            lock (_lock)
            {
                _monitoredElements.Remove(job);
            }
        }

        public void StopAllMonitoring()
        {
            lock (_lock)
            {
                _monitoredElements.Clear();
            }

            if (_monitorThread != null)
            {
                _monitorThread.Join();
                _monitorThread = null;
            }
        }

        private void MonitorThread()
        {
            // Stops the thread when the list of monitored elements becomes empty
            bool running = true;
            while (running)
            {
                lock (_lock)
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    if (_lastMonitorLoop < now - TimeSpan.FromMinutes(5))
                    {
                        // We were apparently in sleep mode - skip this loop, because it will falsify results
                        // (i.e. integrating power usage over a time where the computer was off will be incorrect)
                        foreach (var elem in _monitoredElements)
                        {
                            elem.LastUpdated = now;
                        }

                        _lastMonitorLoop = now;
                        continue;
                    }

                    foreach (var elem in _monitoredElements)
                    {
                        TimeSpan timeSinceLastUpdate = now - elem.LastUpdated;
                        if (timeSinceLastUpdate > elem.Interval)
                        {
                            if (elem.Sensor.TryGetValue(out IQuantity value))
                            {
                                elem.OnNewValue(elem.Sensor, value, timeSinceLastUpdate);
                            }

                            elem.LastUpdated = now;
                        }
                    }

                    _lastMonitorLoop = now;
                    running = _monitoredElements.Count > 0;
                }

                if (running)
                {
                    Thread.Sleep(MonitorInterval);
                }
            }
        }

        /// <summary>
        /// Adds some special derived sensors.
        /// - For each power sensor, this adds another sensor that integrates power over time and so generated the energy used in W/h or
        /// more conveniently, Kilowatthours (this is the unit the electric bill bases on)
        /// - Gives the heat flux for the primary CPU, using the given CPU die size (or a default value)
        /// </summary>
        /// <param name="cpuDieSize">Die size of your CPU, optional. Find your CPU on https://en.wikichip.org/ to find out. Note: This
        /// value is usually much smaller than the size of the physical CPU.</param>
        public void EnableDerivedSensors(Area cpuDieSize = default)
        {
            if (cpuDieSize.Value == 0)
            {
                // Values for some recent intel chips (coffee lake)
                if (LogicalProcessors <= 4)
                {
                    cpuDieSize = Area.FromSquareMillimeters(126);
                }
                else
                {
                    cpuDieSize = Area.FromSquareMillimeters(149.6);
                }
            }

            if (_derivedSensors.Count != 0)
            {
                // Already set up
                return;
            }

            Sensor cpuPower = null;

            foreach (var sensor in GetSensorList())
            {
                if (sensor.SensorType == SensorType.Power)
                {
                    // Energy usage (integration of power over time)
                    var managementInstance = new EnergyManagementInstance();
                    Sensor newSensor = new Sensor(managementInstance, sensor.Name + " Energy", sensor.Identifier + "/energy", sensor.Identifier, SensorType.Energy);
                    newSensor.Tag = StartMonitoring(sensor, TimeSpan.FromMilliseconds(500), (s, value, timeSinceUpdate) =>
                    {
                        double previousEnergy = managementInstance.Value;
                        // Value is in watts, so increment is in watts-hours, which is an unit we can later convert from
                        double increment = value.Value * timeSinceUpdate.TotalHours;
                        double newEnergy = previousEnergy + increment;
                        managementInstance.Value = newEnergy;
                    });
                    _derivedSensors.Add(newSensor);
                }

                // For CPU package, calculate heat flux
                if (sensor.SensorType == SensorType.Power && sensor.Name.IndexOf("CPU Package", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var managementInstance = new EnergyManagementInstance();
                    var newSensor = new Sensor(managementInstance, sensor.Name + " HeatFlux", sensor.Identifier + "/heatflux", sensor.Identifier, SensorType.HeatFlux);
                    newSensor.Tag = StartMonitoring(sensor, TimeSpan.FromMilliseconds(500), (s, value, timeSinceUpdate) =>
                    {
                        Power p = Power.FromWatts(value.Value); // Current power usage in Watts
                        HeatFlux hf = p / cpuDieSize;
                        managementInstance.Value = hf.KilowattsPerSquareMeter;
                    });
                    _derivedSensors.Add(newSensor);
                    cpuPower = sensor;
                }
            }

            Sensor vcore = GetSensorList().FirstOrDefault(x => x.SensorType == SensorType.Voltage && x.Name.IndexOf("CPU VCore", StringComparison.OrdinalIgnoreCase) >= 0);
            // From VCore and CPU package power calculate amperes into CPU
            if (cpuPower != null && vcore != null)
            {
                var managementInstance = new EnergyManagementInstance();
                var newSensor = new Sensor(managementInstance, vcore.Name + " Current", vcore.Identifier + "/current", vcore.Identifier, SensorType.Current);
                newSensor.Tag = StartMonitoring(vcore, TimeSpan.FromMilliseconds(500), (s, value, timeSinceUpdate) =>
                {
                    if (cpuPower.TryGetValue(out Power power))
                    {
                        ElectricPotential potential = ElectricPotential.FromVolts(value.Value);
                        // This function is missing in the library
                        ElectricCurrent current = ElectricCurrent.FromAmperes(power.Watts / potential.Volts);
                        managementInstance.Value = current.Amperes;
                    }
                });
                _derivedSensors.Add(newSensor);
            }
        }

        public void DisableDerivedSensors()
        {
            foreach (var s in _derivedSensors)
            {
                StopMonitoring((MonitoringJob)s.Tag);
            }

            _derivedSensors.Clear();
        }

        public void Dispose()
        {
            StopAllMonitoring();
            _gpu = null;
            _cpu = null;
        }

        public sealed class Sensor : IDisposable
        {
            private readonly ManagementObject _instance;

            public Sensor(ManagementObject instance, string name, string identifier, string parent, SensorType typeEnum)
            {
                _instance = instance;
                Name = name;
                Identifier = identifier;
                Parent = parent;
                SensorType = typeEnum;
            }

            public string Name { get; }
            public string Identifier { get; }
            public string Parent { get; }
            public SensorType SensorType { get; }

            /// <summary>
            /// An user-defined marker that is attached to this sensor instance
            /// </summary>
            public object Tag
            {
                get;
                set;
            }

            public bool TryGetValue(out IQuantity value)
            {
                if (!_typeMap.TryGetValue(SensorType, out var elem))
                {
                    value = null;
                    return false;
                }

                float newValue = Convert.ToSingle(_instance["Value"]);
                IQuantity newValueAsUnitInstance = elem.Creator(newValue);

                value = newValueAsUnitInstance;
                return true;
            }

            public bool TryGetValue<T>(out T value)
                where T : IQuantity
            {
                if (!_typeMap.TryGetValue(SensorType, out var elem))
                {
                    value = default(T);
                    return false;
                }

                if (typeof(T) != elem.Type)
                {
                    value = default(T);
                    return false;
                }

                float newValue = Convert.ToSingle(_instance["Value"]);
                object newValueAsUnitInstance = elem.Creator(newValue);

                value = (T)newValueAsUnitInstance;
                return true;
            }

            public override string ToString()
            {
                return Name;
            }

            public void Dispose()
            {
                _instance.Dispose();
            }
        }

        public sealed class Hardware
        {
            public Hardware(string name, string identifier, string parent, string type)
            {
                Name = name;
                Identifier = identifier;
                Parent = parent;
                Type = type;
            }

            public string Name { get; }
            public string Identifier { get; }
            public string Parent { get; }
            public string Type { get; }

            public override string ToString()
            {
                return Name;
            }
        }

        public sealed class MonitoringJob
        {
            internal MonitoringJob(Sensor sensor, TimeSpan timeSpan, OnNewValue onNewValue)
            {
                Sensor = sensor;
                Interval = timeSpan;
                OnNewValue = onNewValue;
                LastUpdated = DateTimeOffset.UtcNow;
            }

            public Sensor Sensor { get; }
            public TimeSpan Interval { get; }
            internal OnNewValue OnNewValue { get; }

            internal DateTimeOffset LastUpdated
            {
                get;
                set;
            }
        }

        private sealed class EnergyManagementInstance : ManagementObject
        {
            // Current value, high precision
            private double _value;

            public EnergyManagementInstance()
            {
                _value = 0;
                Properties.Add("Value", 0.0f);
            }

            public double Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    SetPropertyValue("Value", (float)value);
                    _value = value;
                }
            }
        }
    }
}
