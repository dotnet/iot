// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    internal enum SystemVariableError
    {
        Success = 0,
        FieldReadOnly = 1,
        FieldWriteOnly = 2,
        UnknownDataType = 3,
        UnknownVariableId = 4,
        GenericError = 5,
    }
}
