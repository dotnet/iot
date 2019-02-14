// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Collections.Generic;

namespace Iot.Device.Mfrc522
{
    public class Mfrc522Exception : Exception
    {
        public Mfrc522Exception(string message) 
            : base(message)
        {

        }
    }
}
