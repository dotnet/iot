// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183.Sentences
{
    public enum GroupFunction
    {
        Request = 0,
        Command = 1,
        Acknowledge = 2,
        ReadFields = 3,
        ReadFieldsReply = 4,
        WriteFields = 5,
        WriteFieldsReply = 6,
    }
}
