// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532.AsTarget
{
    public enum TargetFramingType
    {
        Mifare = 0b0000_0000,
        ActiveMode = 0b0000_0001,
        FeliCa = 0b0000_0010
    }
}
