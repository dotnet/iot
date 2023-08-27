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

        private readonly OpenHardwareMonitorTransport _transport;
        private readonly string _host;
        private readonly int _port;

        /// <summary>
        /// A delegate that crates an instance of a quantity from a value
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>A Quantity instance</returns>
        public delegate IQuantity UnitCreator(double value);

        /// <summary>
        /// Event that gets invoked when a value is updated
        /// </summary>
        /// <param name="sensorToMonitor">Sensor that has an updated value</param>
        /// <param name="value">New value for the sensor</param>
        /// <param name="timeSinceUpdate">Time since the last update of this sensor</param>
        public delegate void OnNewValue(Sensor sensorToMonitor, IQuantity value, TimeSpan timeSinceUpdate);

        private static Dictionary<SensorType, (Type Type, UnitCreator Creator)> _typeMap;
        private Hardware? _cpu;
        private Hardware? _gpu;

        private Thread? _monitorThread;
        private object _lock;
        private List<MonitoringJob> _monitoredElements;
        private List<Sensor> _derivedSensors;
        private DateTimeOffset _lastMonitorLoop;
        private TimeSpan _monitoringInterval;

        private IOpenHardwareMonitorInternal? _openHardwareMonitorInternal;
        private SensorUpdateStrategy _updateStrategy;

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
            _typeMap.Add(SensorType.Data, (typeof(Information), x => Information.FromGigabytes(x)));
            _typeMap.Add(SensorType.RawValue, (typeof(Ratio), x => Ratio.FromDecimalFractions(x)));
            _typeMap.Add(SensorType.Throughput, (typeof(BitRate), x => BitRate.FromMegabytesPerSecond(x)));
            _typeMap.Add(SensorType.TimeSpan, (typeof(Duration), x => Duration.FromSeconds(x)));
        }

        /// <summary>
        /// Constructs a new instance of this class.
        /// The class can be constructed even if no sensors are available or OpenHardwareMonitor is not running (yet).
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">The operating system is not Windows.</exception>
        public OpenHardwareMonitor()
            : this(OpenHardwareMonitorTransport.Auto)
        {
        }

        /// <summary>
        /// Constructs a new instance of this class using a specific transport protocol
        /// The class can be constructed even if no sensors are available or OpenHardwareMonitor is not running (yet).
        /// </summary>
        /// <param name="transport">The transport protocol to use. WMI is for OpenHardwareMonitor 0.9 and below, from OpenHardwareMonitor 0.10 and above,
        /// HTTP is used.</param>
        /// <param name="host">Optional host name for connection</param>
        /// <param name="port">Network port</param>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public OpenHardwareMonitor(OpenHardwareMonitorTransport transport, string host = "localhost", int port = 8086)
        {
            _transport = transport;
            _host = host;
            _port = port;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("This class is only supported on Windows operating systems");
            }

            _derivedSensors = new List<Sensor>();

            _monitorThread = null;
            _lock = new object();
            _monitoredElements = new List<MonitoringJob>();
            _lastMonitorLoop = DateTimeOffset.UtcNow;
            MonitoringInterval = DefaultMonitorInterval;
            TryConnectToOhm();
        }

        /// <summary>
        /// Selects the sensor update strategy.
        /// Default is <see cref="SensorUpdateStrategy.PerSensor"/> for WMI, <see cref="SensorUpdateStrategy.SynchronousAfterTimeout"/> for HTTP.
        /// </summary>
        public SensorUpdateStrategy UpdateStrategy
        {
            get
            {
                return _updateStrategy;
            }
            set
            {
                _updateStrategy = value;
                if (_openHardwareMonitorInternal != null)
                {
                    _openHardwareMonitorInternal.UpdateStrategy = value;
                }
            }
        }

        /// <summary>
        /// Re-reads the sensor tree. Updates all values and the list of sensors.
        /// You should invalidate all cached <see cref="Sensor"/> and <see cref="Hardware"/> instances after using this with <paramref name="refreshSensorList"/>=true
        /// </summary>
        /// <paramref name="refreshSensorList">True to also update the list of sensors. False to only update the values. If false, new sensors will not be visible
        /// (e.g. after inserting a thumb drive)</paramref>
        public void UpdateSensors(bool refreshSensorList)
        {
            if (_openHardwareMonitorInternal != null)
            {
                _openHardwareMonitorInternal.UpdateSensors(refreshSensorList);
                ExtractCpuNode(_openHardwareMonitorInternal);
            }
        }

        private bool TryConnectToOhm()
        {
            if (_openHardwareMonitorInternal != null)
            {
                return true;
            }

            IOpenHardwareMonitorInternal? monitor = null;
            if (_transport == OpenHardwareMonitorTransport.Wmi)
            {
                monitor = new OpenHardwareMonitorWmi();
                UpdateStrategy = SensorUpdateStrategy.PerSensor;
            }
            else if (_transport == OpenHardwareMonitorTransport.Http)
            {
                monitor = new OpenHardwareMonitorHttp(_host, _port);
                UpdateStrategy = SensorUpdateStrategy.SynchronousAfterTimeout;
            }
            else if (_transport == OpenHardwareMonitorTransport.Auto)
            {
                monitor = new OpenHardwareMonitorWmi();
                UpdateStrategy = SensorUpdateStrategy.PerSensor;
                if (!monitor.HasHardware())
                {
                    monitor.Dispose();
                    monitor = new OpenHardwareMonitorHttp(_host, _port);
                    UpdateStrategy = SensorUpdateStrategy.SynchronousAfterTimeout;
                }

                if (!monitor.HasHardware())
                {
                    monitor.Dispose();
                    monitor = null;
                }
            }
            else
            {
                throw new ArgumentException("Unsupported transport protocol selected");
            }

            if (monitor != null)
            {
                monitor.UpdateInterval = MonitoringInterval;
                monitor.UpdateStrategy = UpdateStrategy;
                ExtractCpuNode(monitor);

                if (_cpu == null && _gpu == null)
                {
                    monitor.Dispose();
                    monitor = null;
                }
            }

            _openHardwareMonitorInternal = monitor;
            return _openHardwareMonitorInternal != null;
        }

        private void ExtractCpuNode(IOpenHardwareMonitorInternal monitor)
        {
            Hardware? newCpu = null;
            Hardware? newGpu = null;
            foreach (var hardware in monitor.GetHardwareComponents())
            {
                if (hardware.Type != null && hardware.Type.Equals("CPU", StringComparison.OrdinalIgnoreCase))
                {
                    newCpu = hardware;
                }
                else if (hardware.Type != null && hardware.Type.StartsWith("GPU", StringComparison.OrdinalIgnoreCase))
                {
                    newGpu = hardware;
                }
            }

            _cpu = newCpu;
            _gpu = newGpu;
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

                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(MonitoringInterval));
                }

                _monitoringInterval = value;
                if (_openHardwareMonitorInternal != null)
                {
                    _openHardwareMonitorInternal.UpdateInterval = value;
                }
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
            TryConnectToOhm();
            List<Sensor> ret = new List<Sensor>();
            if (_openHardwareMonitorInternal != null)
            {
                ret.AddRange(_openHardwareMonitorInternal.GetSensorList());
            }

            ret.AddRange(_derivedSensors);

            return ret;
        }

        /// <summary>
        /// Returns a list of hardware components, such as "CPU", "GPU" or "Mainboard"
        /// </summary>
        public IList<Hardware> GetHardwareComponents()
        {
            TryConnectToOhm();
            if (_openHardwareMonitorInternal != null)
            {
                return _openHardwareMonitorInternal.GetHardwareComponents();
            }

            return new List<Hardware>();
        }

        /// <summary>
        /// Get the list of sensors for a specific piece of hardware
        /// </summary>
        /// <param name="forHardware">The module that should be queried</param>
        /// <returns>A list of sensors</returns>
        public IEnumerable<Sensor> GetSensorList(Hardware? forHardware)
        {
            if (forHardware == null)
            {
                throw new ArgumentNullException(nameof(forHardware));
            }

            TryConnectToOhm();

            return GetSensorList().Where(x => x.Identifier != null && x.Identifier.StartsWith(forHardware.Identifier ?? string.Empty)).OrderBy(y => y.Identifier);
        }

        // Some well-known properties have their own method

        /// <summary>
        /// Gets the average CPU temperature (averaged over all CPU sensors / cores)
        /// </summary>
        public bool TryGetAverageCpuTemperature(out Temperature temperature)
        {
            temperature = default;
            if (TryConnectToOhm() == false)
            {
                return false;
            }

            if (_cpu == null)
            {
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
            temperature = default;
            if (TryConnectToOhm() == false)
            {
                return false;
            }

            if (_gpu == null)
            {
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
        public bool TryGetAverage<T>(Hardware hardware,
#if NET5_0_OR_GREATER
        [NotNullWhen(true)]
#endif
        out T? average)
            where T : IQuantity
        {
            double value = 0;
            int count = 0;
            Enum? unitThatWasUsed = null;
            foreach (var s in GetSensorList(hardware))
            {
                if (s.TryGetValue(out T? singleValue))
                {
                    if (unitThatWasUsed == null)
                    {
                        unitThatWasUsed = singleValue!.Unit;
                    }
                    else if (!unitThatWasUsed.Equals(singleValue!.Unit))
                    {
                        throw new NotSupportedException($"The different sensors for {hardware.Name} deliver values in different units");
                    }

                    value += (double)singleValue!.Value;
                    count++;
                }
            }

            if (count == 0)
            {
                average = default(T);
                return false;
            }

            value = value / count;

            average = (T)Quantity.From(value, unitThatWasUsed!);
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
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

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
                            if (elem.Sensor.TryGetValue(out IQuantity? value))
                            {
#if !NET5_0_OR_GREATER
                                elem.OnNewValue(elem.Sensor, value!, timeSinceLastUpdate);
#else
                                elem.OnNewValue(elem.Sensor, value, timeSinceLastUpdate);
#endif
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
            if (cpuDieSize.Equals(Area.Zero, Area.Zero))
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

            Sensor? cpuPower = null;

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
                    Sensor newSensor = new Sensor(sensor.Name + " Energy", sensor.Identifier + "/energy", sensor.Identifier, SensorType.Energy);
                    // This lambda is a bit confusing: But we add a monitoring for the sensor we derive from and update the new
                    newSensor.Job = StartMonitoring(sensor, interval, (s, value, timeSinceUpdate) =>
                    {
                        double previousEnergy = newSensor.Value;
                        // Value is in watts, so increment is in watts-hours, which is an unit we can later convert from
                        double increment = (double)value.Value * timeSinceUpdate.TotalHours;
                        double newEnergy = previousEnergy + increment;
                        newSensor.Value = newEnergy;
                    });
                    _derivedSensors.Add(newSensor);
                }

                // For CPU package, calculate heat flux
                if (sensor.SensorType == SensorType.Power && sensor.Name != null && sensor.Name.IndexOf("CPU Package", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var newSensor = new Sensor(sensor.Name + " HeatFlux", sensor.Identifier + "/heatflux", sensor.Identifier, SensorType.HeatFlux);
                    newSensor.Job = StartMonitoring(sensor, interval, (s, value, timeSinceUpdate) =>
                    {
                        Power p = Power.FromWatts(value.Value); // Current power usage in Watts
                        HeatFlux hf = p / cpuDieSize;
                        newSensor.Value = hf.KilowattsPerSquareMeter;
                    });
                    _derivedSensors.Add(newSensor);
                    cpuPower = sensor;
                }
            }

            Sensor? vcore = GetSensorList().FirstOrDefault(x => x.SensorType == SensorType.Voltage && x.Name != null && x.Name.IndexOf("CPU VCore", StringComparison.OrdinalIgnoreCase) >= 0);
            // From VCore and CPU package power calculate amperes into CPU
            if (cpuPower != null && vcore != null)
            {
                var newSensor = new Sensor(vcore.Name + " Current", vcore.Identifier + "/current", vcore.Identifier, SensorType.Current);
                newSensor.Job = StartMonitoring(vcore, interval, (s, value, timeSinceUpdate) =>
                {
                    if (cpuPower.TryGetValue(out Power power))
                    {
                        ElectricPotential potential = ElectricPotential.FromVolts(value.Value);
                        if (potential.Volts > 0)
                        {
                            ElectricCurrent current = power / potential;
                            s.Value = current.Amperes;
                        }
                        else
                        {
                            s.Value = 0;
                        }
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
                if (s.Job is object)
                {
                    StopMonitoring(s.Job);
                }

                s.Dispose();
            }

            _derivedSensors.Clear();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            StopAllMonitoring();
            if (_openHardwareMonitorInternal != null)
            {
                _openHardwareMonitorInternal.Dispose();
                _openHardwareMonitorInternal = null;
            }

            _gpu = null;
            _cpu = null;
        }

        /// <summary>
        /// Represents a single sensor
        /// </summary>
        public class Sensor : IDisposable
        {
            private readonly string _name;
            private readonly string _identifier;
            private readonly string? _parent;
            private readonly SensorType _sensorType;
            private MonitoringJob? _job;
            private double _value;

            /// <summary>
            /// Create a sensor instance from a management object.
            /// This member is obsolete, use another constructor instead or a derived class.
            /// </summary>
            [Obsolete("Use Sensor(string name, string identifier, string? parent, SensorType typeEnum) instead")]
            public Sensor(ManagementObject dummy, string name, string identifier, string? parent, SensorType typeEnum)
            {
                _name = name;
                _identifier = identifier;
                _parent = parent;
                _sensorType = typeEnum;
            }

            /// <summary>
            /// Creates a sensor instance
            /// </summary>
            public Sensor(string name, string identifier, string? parent, SensorType typeEnum)
            {
                _name = name;
                _identifier = identifier;
                _parent = parent;
                _sensorType = typeEnum;
            }

            /// <summary>
            /// Name of the sensor
            /// </summary>
            public string Name => _name;

            /// <summary>
            /// Sensor identifier (device path)
            /// </summary>
            public string Identifier => _identifier;

            /// <summary>
            /// Sensor parent
            /// </summary>
            public string? Parent => _parent;

            /// <summary>
            /// Sets or gets the last value of the sensor. To get an updated value, use <see cref="TryGetValue"/> instead.
            /// The setter is intended for implementations of derived sensors only.
            /// </summary>
            public double Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    _value = value;
                }
            }

            /// <summary>
            /// Kind of sensor
            /// </summary>
            public SensorType SensorType => _sensorType;

            /// <summary>
            /// Job associated with updating this value
            /// </summary>
            internal MonitoringJob? Job
            {
                get => _job;
                set => _job = value;
            }

            /// <summary>
            /// Attempt to query a value for the sensor
            /// </summary>
            /// <param name="value">Returned value</param>
            /// <returns>True if a value was available</returns>
            public bool TryGetValue(
#if NET5_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out IQuantity? value)
            {
                if (!_typeMap.TryGetValue(SensorType, out var elem))
                {
                    value = null;
                    return false;
                }

                if (!UpdateValue(out double realValue))
                {
                    value = null;
                    return false;
                }

                Value = realValue;

                IQuantity newValueAsUnitInstance = elem.Creator(realValue);

                value = newValueAsUnitInstance;
                return true;
            }

            /// <summary>
            /// Read the value from the underlying transport. This is expected to be overridden by derived classes, unless they use
            /// <see cref="Value"/> to set the content.
            /// </summary>
            protected virtual bool UpdateValue(out double value)
            {
                value = _value;
                return true;
            }

            /// <summary>
            /// Attempt to get a value of the provided type
            /// </summary>
            /// <typeparam name="T">The type of the quantity to return</typeparam>
            /// <param name="value">The returned value</param>
            /// <returns>True if a value of type T could be retrieved</returns>
            public bool TryGetValue<T>(
#if NET5_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out T? value)
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

                UpdateValue(out double newValue);
                Value = newValue;

                object newValueAsUnitInstance = elem.Creator(newValue);

                value = (T)newValueAsUnitInstance;
                return true;
            }

            /// <inheritdoc />
            public override string? ToString()
            {
                return Name ?? base.ToString();
            }

            /// <summary>
            /// Disposes this instance
            /// </summary>
            protected virtual void Dispose(bool disposing)
            {
                // nothing to do
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Represents a piece of hardware
        /// </summary>
        public sealed class Hardware
        {
            private readonly string _name;
            private readonly string _identifier;
            private readonly string? _parent;
            private readonly string? _type;

            /// <summary>
            /// Create an instance of this class
            /// </summary>
            public Hardware(string name, string identifier, string? parent, string? type)
            {
                _name = name;
                _identifier = identifier;
                _parent = parent;
                _type = type;
            }

            /// <summary>
            /// Name of the object
            /// </summary>
            public string Name => _name;

            /// <summary>
            /// Device path
            /// </summary>
            public string Identifier => _identifier;

            /// <summary>
            /// Parent in device path
            /// </summary>
            public string? Parent => _parent;

            /// <summary>
            /// Type of resource
            /// </summary>
            public string? Type => _type;

            /// <summary>
            /// Name of this instance
            /// </summary>
            public override string? ToString()
            {
                return Name ?? base.ToString();
            }
        }

        /// <summary>
        /// A job that monitors a particular sensor
        /// </summary>
        public sealed class MonitoringJob
        {
            private readonly Sensor _sensor;
            private readonly TimeSpan _interval;
            private readonly OnNewValue _onNewValue;
            private DateTimeOffset _lastUpdated;

            internal MonitoringJob(Sensor sensor, TimeSpan timeSpan, OnNewValue onNewValue)
            {
                _sensor = sensor;
                _interval = timeSpan;
                _onNewValue = onNewValue;
                LastUpdated = DateTimeOffset.UtcNow;
            }

            /// <summary>
            /// Sensor this job operates on
            /// </summary>
            public Sensor Sensor => _sensor;

            /// <summary>
            /// Update interval
            /// </summary>
            public TimeSpan Interval => _interval;

            internal OnNewValue OnNewValue => _onNewValue;

            internal DateTimeOffset LastUpdated
            {
                get => _lastUpdated;
                set => _lastUpdated = value;
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

#pragma warning restore CA1416 // Validate platform compatibility
