// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Nmea0183;
using UnitsNet;

namespace Iot.Device.Ili934x.Samples
{
    internal abstract class NmeaDataSet
    {
        public NmeaDataSet(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The name of the data item
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// The current value of the data item
        /// </summary>
        public abstract string Value
        {
            get;
        }

        /// <summary>
        /// The printable unit string of the data item
        /// </summary>
        public abstract string Unit
        {
            get;
        }

        /// <summary>
        /// Updates the value from the cache.
        /// </summary>
        /// <param name="cache">The data cache</param>
        /// <param name="tolerance">Allowed data tolerance (for values that will be truncated before display, it's not meaningful
        /// to refresh them if only the 9th digit has changed)</param>
        /// <returns>True if the value has changed</returns>
        public abstract bool Update(SentenceCache cache, double tolerance);

        /// <summary>
        /// Updates the value from the cache.
        /// </summary>
        /// <param name="cache">The data cache</param>
        /// <returns>True if the value has changed</returns>
        public bool Update(SentenceCache cache)
        {
            return Update(cache, 0);
        }
    }
}
