// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;
using UnitsNet.Units;

namespace ArduinoCsCompiler.Runtime.UnitsNet
{
    [ArduinoReplacement(typeof(RelativeHumidity), true)]
    internal struct MiniRelativeHumidity
    {
        private double _value;
        private RelativeHumidityUnit? _unit;

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

        [ArduinoImplementation(CompareByParameterNames = true)]
        public static bool operator <(MiniRelativeHumidity left, MiniRelativeHumidity right)
        {
            return left._value < right._value;
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        public static bool operator >(MiniRelativeHumidity left, MiniRelativeHumidity right)
        {
            return left._value > right._value;
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        public static bool operator <=(MiniRelativeHumidity left, MiniRelativeHumidity right)
        {
            return left._value <= right._value;
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        public static bool operator >=(MiniRelativeHumidity left, MiniRelativeHumidity right)
        {
            return left._value >= right._value;
        }
    }
}
