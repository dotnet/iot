// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// Application data details
    /// </summary>
    public class ApplicationDataDetail
    {
        /// <summary>
        /// The Short File Identifier
        /// </summary>
        public byte Sfi { get; set; }

        /// <summary>
        /// The index of record to start reading
        /// </summary>
        public byte Start { get; set; }

        /// <summary>
        /// The index of last record to read
        /// </summary>
        public byte End { get; set; }

        /// <summary>
        /// The number of records 
        /// </summary>
        public byte NumberOfRecords { get; set; }
    }
}
