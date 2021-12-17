using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.HardwareMonitor;
using Microsoft.Extensions.Logging;

namespace HardwareMonitor
{
    internal sealed class OpenHardwareMonitorHttp : IOpenHardwareMonitorInternal
    {
        private readonly string _host;
        private readonly int _port;
        private readonly Uri _uri;
        private readonly ILogger _logger;

        private HttpClient _httpClient;
        private string? _version;
        private JsonParser _jsonParser;
        private List<OpenHardwareMonitor.Hardware> _hardware;
        private List<SensorHttp> _sensors;

        public OpenHardwareMonitorHttp(string host, int port)
        {
            _host = host;
            _port = port;
            _hardware = new List<OpenHardwareMonitor.Hardware>();
            _sensors = new List<SensorHttp>();
            _jsonParser = new JsonParser();
            _uri = new Uri("http://" + host + ":" + port + "/");
            _httpClient = new HttpClient();
            _logger = this.GetCurrentClassLogger();
            TryConnectToApi();
        }

        public void Reset()
        {
            _version = string.Empty;
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

        private void GetFullDataAndDecode()
        {
            if (!TryConnectToApi())
            {
                return;
            }

            try
            {
                string fullJson = _httpClient.GetStringAsync(_uri + "api/rootnode").Result;
                ComputerJson? rootNode = _jsonParser.FromJson<ComputerJson>(fullJson);
                if (rootNode == null)
                {
                    return;
                }

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
                            var s = new SensorHttp(sensor.Name, sensor.NodeId, sensor.Parent, sensorType);
                            newSensors.Add(s);
                        }
                    }
                }

                _hardware = newHardware;
                _sensors = newSensors;
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, $"Unable to read /api/rootnode: {ex.Message}");
            }
        }

        public IList<OpenHardwareMonitor.Sensor> GetSensorList()
        {
            GetFullDataAndDecode();
            return _sensors.Cast<OpenHardwareMonitor.Sensor>().ToList();
        }

        public IList<OpenHardwareMonitor.Hardware> GetHardwareComponents()
        {
            GetFullDataAndDecode();

            return _hardware;
        }

        public void Dispose()
        {
            _version = string.Empty;
            _httpClient.Dispose();
        }

        private sealed class SensorHttp : OpenHardwareMonitor.Sensor
        {
            public SensorHttp(string name, string identifier, string? parent, SensorType typeEnum)
                : base(name, identifier, parent, typeEnum)
            {
            }
        }
    }
}
