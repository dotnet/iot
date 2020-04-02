// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Rfid;

namespace Iot.Device.Pn5180
{
    internal class SelectedPiccInformation
    {
        public Data106kbpsTypeB Card { get; set; }
        public bool LastBlockMark { get; set; }
    }
}
