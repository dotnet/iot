// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// Additional data that can be extracted from the Credit Card
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Application transaction counter
        /// </summary>
        ApplicationTransactionCounter,
        /// <summary>
        /// Number of pin try left
        /// </summary>
        PinTryCounter,
        /// <summary>
        /// Last online ATC register
        /// </summary>
        LastOnlineAtcRegister,
        /// <summary>
        /// Log format
        /// </summary>
        LogFormat
    }
}
