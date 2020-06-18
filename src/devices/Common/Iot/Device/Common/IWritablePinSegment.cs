// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Common
{
    /// <summary>
    /// Interface that abstracts pin multiplexing.
    /// </summary>
    public interface IWritablePinSegment
    {
        /// <summary>
        /// Length of segment (number of pins)
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Writes a high or low value to pin
        /// </summary>
        void Write(int pin, int value);
    }
}
