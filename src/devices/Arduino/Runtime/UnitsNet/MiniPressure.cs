using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Arduino.Runtime.UnitsNet
{
    [ArduinoReplacement(typeof(Pressure), true)]
    internal struct MiniPressure
    {
        private const double DEGREE_TO_KELVIN = 273.15;
        private double _value;
        private PressureUnit _unit;

        private MiniPressure(double value, PressureUnit unit)
        {
            _value = value;
            _unit = unit;
        }

        public double Pascals
        {
            get
            {
                if (_unit == PressureUnit.Pascal)
                {
                    return _value;
                }
                else if (_unit == PressureUnit.Hectopascal)
                {
                    return _value * 100;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        public static Pressure FromPascals(QuantityValue value)
        {
            double v = (double)value;
            return new Pressure(v, PressureUnit.Hectopascal);
        }
    }
}
