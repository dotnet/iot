using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareMonitor;

namespace Iot.Device.HardwareMonitor
{
    internal interface IOpenHardwareMonitorInternal : IDisposable
    {
        public SensorUpdateStrategy UpdateStrategy { get; set; }
        public TimeSpan UpdateInterval { get; set; }
        bool HasHardware();
        IList<OpenHardwareMonitor.Sensor> GetSensorList();
        IList<OpenHardwareMonitor.Hardware> GetHardwareComponents();
        void UpdateSensors();
    }
}
