// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// The type of fluid represented by this message
    /// </summary>
    public enum FluidType
    {
        /// <summary>
        /// The tank contains fuel. Normally, this will be diesel fuel (gas oil), as this is the
        /// most used fuel type for boats. See also <seealso cref="FuelGasoline"/>
        /// </summary>
        Fuel = 0,

        /// <summary>
        /// The tank contains fresh water
        /// </summary>
        Water = 1,

        /// <summary>
        /// The tanks contains waste water, e.g. from sinks or showers.
        /// </summary>
        GrayWater = 2,

        /// <summary>
        /// The tank is a live well (e.g. for caught fish)
        /// </summary>
        LiveWell = 3,

        /// <summary>
        /// The tank contains lubrication oil.
        /// </summary>
        Oil = 4,

        /// <summary>
        /// The tank contains black water (toilet waste)
        /// </summary>
        BlackWater = 5,

        /// <summary>
        /// The tank contains gasoline.
        /// </summary>
        FuelGasoline = 6,

        /// <summary>
        /// Indicates an error condition
        /// </summary>
        Error = 14,

        /// <summary>
        /// The tank is unavailable
        /// </summary>
        Unavailable = 15,
    }
}
