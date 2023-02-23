// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public abstract class MessageTestBase
    {
        internal readonly AisParser Parser;

        protected MessageTestBase()
        {
            Parser = new AisParser();
        }
    }
}
