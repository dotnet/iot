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
    /// <summary>
    /// Manufacturer code used for proprietary messages
    /// Note: This enum is incomplete
    /// </summary>
    public enum ManufacturerCode
    {
        Unknown = 0,
        Airmar = 135,
        Lowrance = 140,
        MercuryMarine = 144,
        YanmarMarine = 172,
        VolvoPenta = 174,
        HondaMarine = 175,
        Garmin = 229,
        BepMarine2 = 295,
        Icom = 315,
        BandG = 381,
        FusionElectronics = 419,
        Raymarine = 1851,
    }
}
