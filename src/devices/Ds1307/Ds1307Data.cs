// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ds1307
{
    /// <summary>
    /// DS1307 Raw Data
    /// </summary>
    internal class Ds1307Data
    {
        public int Sec { get; set; }
        public int Min { get; set; }
        public int Hour { get; set; }
        public int Day { get; set; }
        public int Date { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}