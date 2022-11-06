// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
using UnitsNet.Units;

namespace ArduinoCsCompiler.Runtime.UnitsNet
{
    [ArduinoReplacement(typeof(QuantityInfo), false, IncludingPrivates = true)]
    internal class MiniQuantityInfo
    {
        // This member is a dictionary in more recent versions of UnitsNet 4.X, and gone altogether in V5.X
        // With V5.X, it seems that all of the reflection to load unit types is gone, making the use of the library
        // in our context much easier.
        private static readonly Type[] UnitEnumTypes;

        [ArduinoImplementation]
        static MiniQuantityInfo()
        {
            UnitEnumTypes = new Type[]
            {
                typeof(RatioUnit),
                typeof(RelativeHumidityUnit),
                typeof(LengthUnit),
            };
        }
    }
}
