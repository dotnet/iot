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

namespace Iot.Device.OpenHardwareMonitor
{
    /// <summary>
    /// This class connects to a running instance of OpenHardwareMonitor and reads out all available values.
    /// This works only if OpenHardwareMonitor (https://openhardwaremonitor.org/) is currently running.
    /// While the tool needs to be run with elevated permissions, the application using this binding does not.
    /// </summary>
    public sealed class OpenHardwareMonitor : IDisposable
    {
        /// <summary>
        /// This is the monitoring thread interval. All updates will be done in a multiple of this value.
        /// </summary>
        private static readonly TimeSpan DefaultMonitorInterval = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan DefaultDerivedSensorsInterval = TimeSpan.FromMilliseconds(500);

        private delegate IQuantity UnitCreator(float value);

        /// <summary>
        /// Event that gets invoked when a value is updated
        /// </summary>
        /// <param name="sensor">Sensor that has an updated value</param>
        /// <param name="value">New value for the sensor</param>
        /// <param name="timeSinceUpdate">Time since the last update of this sensor</param>
        public delegate void OnNewValue(Sensor sensor, IQuantity value, TimeSpan timeSinceUpdate);

        private static Dictionary<SensorType, (Type Type, UnitCreator Creator)> _typeMap;
        private Hardware _cpu;
        private Hardware _gpu;

        private Thread _monitorThread;
        private object _lock;
        private List<MonitoringJob> _monitoredElements;
        private List<Sensor> _derivedSensors;
        private DateTimeOffset _lastMonitorLoop;
        private TimeSpan _monitoringInterval;

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

        /// <summary>
        /// Constructs a new instance of this class.
        /// The class can be constructed even if no sensors are available or OpenHardwareMonitor is not running (yet).
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">The operating system is not Windows.</exception>
        public OpenHardwareMonitor()
        {
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
            MonitoringInterval = DefaultMonitorInterval;
        }

        /// <summary>
        /// The minimum monitoring interval.
        /// </summary>
        public TimeSpan MonitoringInterval
        {
            get
            {
                return _monitoringInterval;
            }

            set
            {
                if (_monitorThread != null)
                {
                    throw new InvalidOperationException($"{nameof(MonitoringInterval)} can only be changed while monitoring is disabled.");
                }

                _monitoringInterval = value;
            }
        }

        private void InitHardwareMonitor()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "SELECT * FROM Sensor");
                if (searcher.Get().Count > 0)
                {
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

        /// <summary>
        /// Number of logical processors in the system
        /// </summary>
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

        /// <summary>
        /// Returns a list of hardware components, such as "CPU", "GPU" or "Mainboard"
        /// </summary>
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

        /// <summary>
        /// Get the list of sensors for a specific piece of hardware
        /// </summary>
        /// <param name="forHardware">The module that should be queried</param>
        /// <returns>A list of sensors</returns>
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
        /// Gets the average GPU temperature (averaged over all GPU sensors / cores)
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

        /// <summary>
        /// Gets the overall CPU Load
        /// </summary>
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

        /// <summary>
        /// Tries to calculate the average of a set of sensors.
        /// </summary>
        /// <typeparam name="T">Type of value to query (i.e. Load, Power)</typeparam>
        /// <param name="hardware">The hardware type (i.e. CPU)</param>
        /// <param name="average">Gets the returned quantity</param>
        /// <returns>True if at least one matching quantity was found</returns>
        /// <exception cref="NotSupportedException">There were multiple sensors found, but they return different units (i.e. CPU temperature is
        /// reported as Celsius for some cores and Fahrenheit for others)</exception>
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
            // round to the nearest multiple of the thread interval
            double fract = roundedInterval % MonitoringInterval.TotalSeconds;
            if (fract > 0)
            {
                // If fract is > 0, step to the next multiple (+ make sure we're really greater)
                roundedInterval = roundedInterval - fract + MonitoringInterval.TotalSeconds + 1E-6;
            }

            if (roundedInterval < MonitoringInterval.TotalSeconds)
            {
                roundedInterval = MonitoringInterval.TotalSeconds;
            }

            monitoringInterval = TimeSpan.FromSeconds(roundedInterval);
            lock (_lock)
            {
                MonitoringJob job = new MonitoringJob(sensorToMonitor, monitoringInterval, onNewValue);
                _monitoredElements.Add(job);
                return job;
            }
        }

        /// <summary>
        /// Stops monitoring of the given job.
        /// </summary>
        /// <param name="job">Monitoring job</param>
        public void StopMonitoring(MonitoringJob job)
        {
            lock (_lock)
            {
                _monitoredElements.Remove(job);
            }
        }

        /// <summary>
        /// Stops all monitoring.
        /// </summary>
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
                    Thread.Sleep(MonitoringInterval);
                }
            }
        }

        /// <summary>
        /// Adds some special derived sensors.
        /// - For each power sensor, this adds another sensor that integrates power over time and so generated the energy used in W/h or
        /// more conveniently, Kilowatthours (this is the unit the electricity bill bases on)
        /// - Gives the heat flux for the primary CPU, using the given CPU die size (or a default value)
        /// </summary>
        /// <param name="cpuDieSize">Die size of your CPU, optional. Find your CPU on https://en.wikichip.org/ to find out. Note: This
        /// value is usually much smaller than the size of the physical CPU.</param>
        /// <param name="monitoringInterval">Monitoring interval for the derived sensors. Defaults to 500ms.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="monitoringInterval"/> is less than 0.</exception>
        public void EnableDerivedSensors(Area cpuDieSize = default, TimeSpan monitoringInterval = default)
        {
            if (cpuDieSize == default)
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

            TimeSpan interval = monitoringInterval;
            if (interval == default)
            {
                interval = DefaultDerivedSensorsInterval;
            }
            else if (interval.TotalSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(monitoringInterval));
            }

            foreach (var sensor in GetSensorList())
            {
                if (sensor.SensorType == SensorType.Power)
                {
                    // Energy usage (integration of power over time)
                    var managementInstance = new EnergyManagementInstance();
                    Sensor newSensor = new Sensor(managementInstance, sensor.Name + " Energy", sensor.Identifier + "/energy", sensor.Identifier, SensorType.Energy);
                    newSensor.Job = StartMonitoring(sensor, interval, (s, value, timeSinceUpdate) =>
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
                    newSensor.Job = StartMonitoring(sensor, interval, (s, value, timeSinceUpdate) =>
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
                newSensor.Job = StartMonitoring(vcore, interval, (s, value, timeSinceUpdate) =>
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

        /// <summary>
        /// Remove the derived sensors from the active list.
        /// </summary>
        public void DisableDerivedSensors()
        {
            foreach (var s in _derivedSensors)
            {
                StopMonitoring(s.Job);
            }

            _derivedSensors.Clear();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            StopAllMonitoring();
            _gpu = null;
            _cpu = null;
        }

        /// <summary>
        /// Represents a single sensor
        /// </summary>
        public sealed class Sensor : IDisposable
        {
            private readonly ManagementObject _instance;

            /// <summary>
            /// Creates a sensor instance
            /// </summary>
            public Sensor(ManagementObject instance, string name, string identifier, string parent, SensorType typeEnum)
            {
                _instance = instance;
                Name = name;
                Identifier = identifier;
                Parent = parent;
                SensorType = typeEnum;
            }

            /// <summary>
            /// Name of the sensor
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Sensor identifier (device path)
            /// </summary>
            public string Identifier { get; }

            /// <summary>
            /// Sensor parent
            /// </summary>
            public string Parent { get; }

            /// <summary>
            /// Kind of sensor
            /// </summary>
            public SensorType SensorType { get; }

            /// <summary>
            /// Job associated with updating this value
            /// </summary>
            internal MonitoringJob Job
            {
                get;
                set;
            }

            /// <summary>
            /// Attempt to query a value for the sensor
            /// </summary>
            /// <param name="value">Returned value</param>
            /// <returns>True if a value was available</returns>
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

            /// <summary>
            /// Attempt to get a value of the provided type
            /// </summary>
            /// <typeparam name="T">The type of the quantity to return</typeparam>
            /// <param name="value">The returned value</param>
            /// <returns>True if a value of type T could be retrieved</returns>
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

            /// <inheritdoc />
            public override string ToString()
            {
                return Name;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                _instance.Dispose();
            }
        }

        /// <summary>
        /// Represents a piece of hardware
        /// </summary>
        public sealed class Hardware
        {
            /// <summary>
            /// Create an instance of this class
            /// </summary>
            public Hardware(string name, string identifier, string parent, string type)
            {
                Name = name;
                Identifier = identifier;
                Parent = parent;
                Type = type;
            }

            /// <summary>
            /// Name of the object
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Device path
            /// </summary>
            public string Identifier { get; }

            /// <summary>
            /// Parent in device path
            /// </summary>
            public string Parent { get; }

            /// <summary>
            /// Type of resource
            /// </summary>
            public string Type { get; }

            /// <summary>
            /// Name of this instance
            /// </summary>
            public override string ToString()
            {
                return Name;
            }
        }

        /// <summary>
        /// A job that monitors a particular sensor
        /// </summary>
        public sealed class MonitoringJob
        {
            internal MonitoringJob(Sensor sensor, TimeSpan timeSpan, OnNewValue onNewValue)
            {
                Sensor = sensor;
                Interval = timeSpan;
                OnNewValue = onNewValue;
                LastUpdated = DateTimeOffset.UtcNow;
            }

            /// <summary>
            /// Sensor this job operates on
            /// </summary>
            public Sensor Sensor { get; }

            /// <summary>
            /// Update interval
            /// </summary>
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
