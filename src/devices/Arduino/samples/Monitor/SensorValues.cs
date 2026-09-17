// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace Iot.Device.Arduino.Sample
{
    internal record SensorValues(string Name)
    {
        public Temperature? Temperature { get; set; }
        public Pressure? Pressure { get; set; }
        public RelativeHumidity? Humidity { get; set; }
        public Ratio? Load { get; set; }
        public Power? Power { get; set; }
        public Energy? Energy { get; set; }

        public void Clear()
        {
            Temperature = null;
            Pressure = null;
            Humidity = null;
            Load = null;
            Power = null;
            Energy = null;
        }
    }
}
