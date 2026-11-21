// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// A field declaration, used when the message is used in a Request/Response exchange using
    /// PGN 126208
    /// </summary>
    public record FieldDeclaration(int FieldNumber, int FieldSize, string Description, int? Constant, Func<int, int>? Getter = null)
    {
        /// <summary>
        /// The value of a field, if bound by a request message
        /// </summary>
        public int? Value
        {
            get;
            set;
        }

        /// <summary>
        /// This field is only used on an Acknowledge-Reply. It is not 0 to indicate a parameter error
        /// on a specific parameter.
        /// </summary>
        public int? ParameterError
        {
            get;
            set;
        }
    }
}
