// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// The source of a Tag
    /// </summary>
    [Flags]
    public enum Source
    {
        /// <summary>
        /// Card
        /// </summary>
        Icc = 0b0000_0001,

        /// <summary>
        /// Terminal
        /// </summary>
        Terminal = 0b0000_0010,

        /// <summary>
        /// Issuer
        /// </summary>
        Issuer = 0b0000_0100
    }
}