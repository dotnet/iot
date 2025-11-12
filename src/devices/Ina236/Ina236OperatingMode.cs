using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Adc
{
    /// <summary>
    /// Current device operating mode
    /// </summary>
    public enum Ina236OperatingMode
    {
        Shutdown = 0,
        SingeShuntVoltage = 0b001,
        SingleBusVoltage = 0b010,
        SingleShuntAndBusVoltage = 0b011,
        Shutdown2 = 0b100,
        ContinuousShuntVoltage = 0b101,
        ContinuousBusVoltage = 0b110,
        ContinuousShuntAndBusVoltage = 0b111, // This is the default setting
        ModeMask = 0b111
    }
}
