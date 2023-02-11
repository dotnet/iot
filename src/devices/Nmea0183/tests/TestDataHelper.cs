// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;

namespace Iot.Device.Nmea0183.Tests
{
    internal static class TestDataHelper
    {
        public static Stream GetResourceStream(string name)
        {
            var assembly = typeof(TestDataHelper).Assembly;
            var names = assembly.GetManifestResourceNames();
            var correctStreamName = names.First(x => x.EndsWith(name));
            return assembly.GetManifestResourceStream(correctStreamName) ?? throw new MissingManifestResourceException($"Couldn't find required test resource {name}");
        }
    }
}
