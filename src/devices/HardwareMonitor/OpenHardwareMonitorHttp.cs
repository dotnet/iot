// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.HardwareMonitor;
using Iot.Device.HardwareMonitor.JsonSchema;
using Microsoft.Extensions.Logging;

namespace Iot.Device.HardwareMonitor
{
    internal sealed class OpenHardwareMonitorHttp : IOpenHardwareMonitorInternal
    {
        private readonly string _host;
        private readonly int _port;
        private readonly Uri _uri;
        private readonly ILogger _logger;
        private readonly object _lock = new object();

        private HttpClient _httpClient;
        private string? _version;
        private List<OpenHardwareMonitor.Hardware> _hardware;
        private List<SensorHttp> _sensors;
        private DateTime _lastUpdateTime;

        public OpenHardwareMonitorHttp(string host, int port)
        {
            _host = host;
            _port = port;
            _lastUpdateTime = DateTime.MinValue;
            _hardware = new List<OpenHardwareMonitor.Hardware>();
            _sensors = new List<SensorHttp>();
            _uri = new Uri("http://" + host + ":" + port + "/");
            _httpClient = new HttpClient();
            _logger = this.GetCurrentClassLogger();
            TryConnectToApi();
        }

        public SensorUpdateStrategy UpdateStrategy
        {
            get;
            set;
        }

        public TimeSpan UpdateInterval
        {
            get;
            set;
        }

        public void UpdateSensors(bool refreshSensorList)
        {
            GetFullDataAndDecode(refreshSensorList);
        }

        public bool TryConnectToApi()
        {
            if (!string.IsNullOrEmpty(_version))
            {
                return true;
            }

            _version = string.Empty;
            try
            {
                _version = _httpClient.GetStringAsync(_uri + "api/version").Result;
            }
            catch (AggregateException ex)
            {
                _logger.LogWarning(ex, $"Unable to read version from OpenHardwareMonitor REST api: {ex.Message}");
                _version = String.Empty;
            }

            return !string.IsNullOrEmpty(_version);
        }

        public bool HasHardware()
        {
            return !string.IsNullOrEmpty(_version);
        }

        private void GetFullDataAndDecode(bool refreshSensorList)
        {
            if (!TryConnectToApi())
            {
                return;
            }

            try
            {
                string fullJson = _httpClient.GetStringAsync(_uri + "api/rootnode").Result;
                fullJson = fullJson.Replace("\"Value\": NaN", "\"Value\": 0"); // this fixes an error I encounter where a sensors value is NaN causing the entire thing to crash.
                lock (_lock)
                {
                    ComputerJson? rootNode = JsonSerializer.Deserialize<ComputerJson>(fullJson);
                    if (rootNode == null)
                    {
                        return;
                    }

                    if (_hardware.Count == 0 || _sensors.Count == 0 || refreshSensorList)
                    {
                        var (newHardware, newSensors) = CreateSensorTree(rootNode);
                        _hardware = newHardware;
                        _sensors = newSensors;
                    }
                    else
                    {
                        // Instead of updating the sensor list, update only the values for each sensor
                        List<SensorJson> sensorsFromJson = new();
                        foreach (var hw in rootNode.Hardware)
                        {
                            sensorsFromJson.AddRange(hw.Sensors);
                        }

                        foreach (var sensor in sensorsFromJson)
                        {
                            var sensorToUpdate = _sensors.FirstOrDefault(x => x.Identifier == sensor.NodeId);
                            if (sensorToUpdate != null)
                            {
                                sensorToUpdate.Value = sensor.Value;
                            }
                        }
                    }

                    _lastUpdateTime = DateTime.UtcNow;
                }
            }
            catch (AggregateException ex)
            {
                _logger.LogWarning(ex, $"Unable to read /api/rootnode: {ex.Message}");
                // If we failed to retrieve updates for some time, reset the fields.
                if ((DateTime.UtcNow - _lastUpdateTime).Duration().TotalSeconds > UpdateInterval.TotalSeconds * 10) // (netstandard doesn't support operator* on TimeSpan)
                {
                    _hardware = new List<OpenHardwareMonitor.Hardware>();
                    _sensors = new List<SensorHttp>();
                }
            }
        }

        private (List<OpenHardwareMonitor.Hardware> Hardware, List<SensorHttp> Sensors) CreateSensorTree(ComputerJson rootNode)
        {
            var newHardware = new List<OpenHardwareMonitor.Hardware>();
            var newSensors = new List<SensorHttp>();

            foreach (var hardware in rootNode.Hardware)
            {
                var hw = new OpenHardwareMonitor.Hardware(hardware.Name, hardware.NodeId, hardware.Parent, hardware.HardwareType);
                newHardware.Add(hw);

                foreach (var sensor in hardware.Sensors)
                {
                    SensorType sensorType;
                    if (Enum.TryParse<SensorType>(sensor.Type, true, out sensorType))
                    {
                        var s = new SensorHttp(this, sensor.Name, sensor.NodeId, sensor.Parent, sensorType, sensor.Value);
                        newSensors.Add(s);
                    }
                }
            }

            return (newHardware, newSensors);
        }

        public IList<OpenHardwareMonitor.Sensor> GetSensorList()
        {
            if (!_sensors.Any())
            {
                GetFullDataAndDecode(false);
            }

            return _sensors.Cast<OpenHardwareMonitor.Sensor>().ToList();
        }

        public IList<OpenHardwareMonitor.Hardware> GetHardwareComponents()
        {
            if (!_hardware.Any())
            {
                GetFullDataAndDecode(false);
            }

            return _hardware;
        }

        public void Dispose()
        {
            _version = string.Empty;
            _httpClient.Dispose();
        }

        private Uri Combine(Uri root, params String[] moreParts)
        {
            if (!moreParts.Any())
            {
                return root;
            }

            Uri combined = root;
            foreach (var part in moreParts)
            {
                Uri? temp;
                // The second argument to TryCreate must not be rooted, or the newly added part overwrites any existing parts.
                var trimmed = part.TrimStart('/');
                if (!Uri.TryCreate(combined, trimmed, out temp))
                {
                    throw new InvalidOperationException($"{part} is not a valid URL component");
                }

                combined = temp;
            }

            return combined;
        }

        private bool TryUpdateSensor(SensorHttp sensor, out double value)
        {
            value = 0;
            if (UpdateStrategy == SensorUpdateStrategy.PerSensor || UpdateStrategy == SensorUpdateStrategy.Unspecified)
            {
                try
                {
                    var apiUrl = Combine(_uri, "/api/nodes/", sensor.Identifier);

                    string fullJson = _httpClient.GetStringAsync(apiUrl).Result;
                    lock (_lock)
                    {
                        SensorJson? rootNode = JsonSerializer.Deserialize<SensorJson>(fullJson);
                        if (rootNode == null)
                        {
                            _logger.LogWarning($"Unable to parse json: {fullJson}");
                            return false;
                        }

                        value = rootNode.Value;
                        return true;
                    }
                }
                catch (AggregateException ex)
                {
                    _logger.LogWarning(ex, $"Unable to read update value for node {sensor.Identifier}: {ex.Message}");
                    return false;
                }
            }
            else if (UpdateStrategy == SensorUpdateStrategy.SynchronousAfterTimeout && (DateTime.UtcNow - _lastUpdateTime).Duration() > UpdateInterval)
            {
                UpdateSensors(false);
            }

            value = sensor.Value;
            return true;
        }

        private sealed class SensorHttp : OpenHardwareMonitor.Sensor
        {
            private readonly OpenHardwareMonitorHttp _monitor;

            public SensorHttp(OpenHardwareMonitorHttp monitor, string name, string identifier, string? parent, SensorType typeEnum, double initialValue)
                : base(name, identifier, parent, typeEnum)
            {
                _monitor = monitor;
                Value = initialValue;
            }

            protected override bool UpdateValue(out double value)
            {
                return _monitor.TryUpdateSensor(this, out value);
            }
        }
    }
}
