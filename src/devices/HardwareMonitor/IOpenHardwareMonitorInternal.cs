using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.HardwareMonitor
{
    internal interface IOpenHardwareMonitorInternal : IDisposable
    {
        bool HasHardware();
        IList<OpenHardwareMonitor.Sensor> GetSensorList();
        IList<OpenHardwareMonitor.Hardware> GetHardwareComponents();
    }
}
