using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Arduino.Runtime.UnitsNet
{
    [ArduinoReplacement(typeof(Length), true)]
    internal struct MiniLength
    {
        private double _value;
        private LengthUnit _unit;

        private MiniLength(double value, LengthUnit unit)
        {
            _value = value;
            _unit = unit;
        }

        public double Meters
        {
            get
            {
                if (_unit == LengthUnit.Meter)
                {
                    return _value;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        public static Length FromMeters(QuantityValue value)
        {
            double v = (double)value;
            return new Length(v, LengthUnit.Meter);
        }
    }
}
