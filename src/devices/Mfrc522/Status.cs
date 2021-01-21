// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mfrc522
{
    /// <summary>
    /// Status for the functions
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// All happens perfectly
        /// </summary>
        Ok,

        /// <summary>
        /// A collision has been detected, this allows for example to use
        /// retry mechanism
        /// </summary>
        Collision,

        /// <summary>
        /// An error happened
        /// </summary>
        Error,

        /// <summary>
        /// Timeout in reading or writing to the card, it may means the card has been
        /// removed from the reader or is too far
        /// </summary>
        Timeout,
    }
}
