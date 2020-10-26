// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Serialization;

namespace Iot.Device.MettlerToledo.Exceptions
{
    /// <summary>
    /// Occurs when invalid data is received from the balance.
    /// </summary>
    public class InvalidDataReceivedException : MettlerToledoException
    {
        /// <summary>
        /// Data received.
        /// </summary>
        public string Response { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDataReceivedException"/> class.
        /// </summary>
        /// <param name="command">Command that triggered this exception</param>
        /// <param name="response">Response that was received</param>
        public InvalidDataReceivedException(string command, string response)
            : base(command) => Response = response;
    }
}
