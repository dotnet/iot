using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Arduino.Runtime.UnitsNet
{
    [ArduinoReplacement(typeof(RelativeHumidity), true)]
    internal struct MiniRelativeHumidity
    {
        private double _value;
        private RelativeHumidityUnit _unit;

        private MiniRelativeHumidity(double value, RelativeHumidityUnit unit)
        {
            _value = value;
            _unit = unit;
        }

        public double Percent
        {
            get
            {
                if (_unit == RelativeHumidityUnit.Percent)
                {
                    return _value;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        public static RelativeHumidity FromPercent(QuantityValue value)
        {
            double v = (double)value;
            return new RelativeHumidity(v, RelativeHumidityUnit.Percent);
        }
    }
}
