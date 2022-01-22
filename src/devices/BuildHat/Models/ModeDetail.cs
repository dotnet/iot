// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BuildHat.Models
{
    /// <summary>
    /// Mode details
    /// </summary>
    public struct ModeDetail
    {
        /// <summary>
        /// Gets the mode number.
        /// </summary>
        public int Number { get; internal set; }

        /// <summary>
        /// Gets the name of the mode
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the unit of the mode.
        /// </summary>
        public string Unit { get; internal set; }

        /// <summary>
        /// Gets the number of data items.
        /// </summary>
        public int NumberOfDataItems { get; internal set; }

        /// <summary>
        /// Gets the data type.
        /// </summary>
        public Type DataType { get; internal set; }

        /// <summary>
        /// Gets the number of chars to display the value
        /// </summary>
        public int NumberOfCharsToDisplay { get; internal set; }

        /// <summary>
        /// Gets the number of data in the mode
        /// </summary>
        public int NumberOfData { get; internal set; }

        /// <summary>
        /// Gets the decimal preciion (for float, 0 oterhwise)
        /// </summary>
        public int DecimalPrecision { get; internal set; }

        /// <summary>
        /// Gets the minimum and maximum values for the mode
        /// </summary>
        public MinimumMaximumValues[] MinimumMaximumValues { get; internal set; }
    }
}
