using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// These instructions make the classes public for building the individual projects.
// The default visibility is "internal" so they stay hidden if this section is removed for the final library build.
#if !BUILDING_IOT_DEVICE_BINDINGS

#pragma warning disable SA1403 // File may only contain a single namespace

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


#endif
