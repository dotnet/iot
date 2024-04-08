// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;
using UnitsNet.Units;

namespace ArduinoCsCompiler.Runtime.UnitsNet
{
    [ArduinoReplacement(typeof(Duration), true)]
    internal struct MiniDuration
    {
        private double _duration;
        private DurationUnit? _unit;

        private MiniDuration(double value, DurationUnit unit)
        {
            _duration = value;
            _unit = unit;
        }

        public double Milliseconds
        {
            get
            {
                if (_unit == DurationUnit.Millisecond)
                {
                    return _duration;
                }

                throw new NotSupportedException();
            }
        }

        public static MiniDuration FromMilliseconds(QuantityValue value)
        {
            return new MiniDuration((double)value, DurationUnit.Millisecond);
        }
    }
}
