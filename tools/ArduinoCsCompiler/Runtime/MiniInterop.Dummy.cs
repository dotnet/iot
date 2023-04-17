// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    internal partial class MiniInterop
    {
        /// <summary>
        /// This class contains a few fields we need to take tokens from but they're not really part of the runtime
        /// </summary>
        internal partial class Dummy
        {
            public Dummy()
            {
                TimeDynamicZoneInformation = default;
                TimeZoneInformation = default;
                TZI = default;
            }

            public TIME_DYNAMIC_ZONE_INFORMATION TimeDynamicZoneInformation;

            public TIME_ZONE_INFORMATION TimeZoneInformation;

            public byte[]? TZI;
        }
    }
}
