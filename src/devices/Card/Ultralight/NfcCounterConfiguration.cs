// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Card.Ultralight
{
    /// <summary>
    /// NFC counter configuration class
    /// </summary>
    public class NfcCounterConfiguration
    {
        /// <summary>
        /// Is the Counter enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Is the counter password protected
        /// </summary>
        public bool IsPasswordProtected { get; set; }
    }
}
