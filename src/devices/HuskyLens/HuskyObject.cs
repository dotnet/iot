// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// Represent the base type for object detected by the Huskylens camera
    /// </summary>
    public abstract class HuskyObject
    {
        /// <summary>
        /// Learned id
        /// </summary>
        /// <value>the id of the value learned, 0 if unknown</value>
        public int Id { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The learned id</param>
        protected HuskyObject(int id)
        {
            Id = id;
        }
    }
}
