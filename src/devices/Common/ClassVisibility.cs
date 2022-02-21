// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SA1403 // File may only contain a single namespace

// These instructions make the classes public for building the individual projects.
// The default visibility is "internal" so they stay hidden if this section is removed for the final library build.
#if !BUILDING_IOT_DEVICE_BINDINGS

public partial class Interop
{
}

namespace Iot.Device.Common
{
    public partial class NumberHelper
    {
    }
}

namespace System.Device
{
    public partial class DelayHelper
    {
    }
}

namespace System.Device.Gpio
{
    public partial struct PinVector32
    {
    }

    public partial struct PinVector64
    {
    }
}

#else

internal partial class Interop
{
}

namespace Iot.Device.Common
{
    internal partial class NumberHelper
    {
    }
}

namespace System.Device
{
    internal partial class DelayHelper
    {
    }
}

namespace System.Device.Gpio
{
    internal partial struct PinVector32
    {
    }

    internal partial struct PinVector64
    {
    }
}


#endif
