// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// The base class for the Country and Currency correspondance
    /// </summary>
    public class CountryCurrency
    {
        /// <summary>
        /// The Country name in English
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// The Currency name in English
        /// </summary>
        public string CurrencyName { get; set; }

        /// <summary>
        /// The international Country code
        /// Note that non country like Europe Union has 0 as a code
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The international Country code number
        /// </summary>
        public int CurrencyNumber { get; set; }

        /// <summary>
        /// The international Currency code number
        /// </summary>
        public int CountryNumber { get; set; }
    }
}
