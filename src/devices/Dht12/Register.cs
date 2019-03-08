// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Dht12
{
    internal enum Register : byte
    {
        DHT_HUMI_INT = 0x00,
        DHT_HUMI_DECIMAL = 0x01,
        DHT_TEMP_INT = 0x02,
        DHT_TEMP_DECIMAL = 0x03
    }
}
