// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ina236.Tests
{
    internal class SimulatedIna236 : I2cSimulatedDeviceBase
    {
        public SimulatedIna236(I2cConnectionSettings settings)
            : base(settings)
        {
        }

        protected override int WriteRead(byte[] inputBuffer, byte[] outputBuffer)
        {
            throw new NotImplementedException();
        }
    }
}
