// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    /// <summary>
    /// Specifies the parity bit for a SerialPort object.
    /// </summary>
    public enum Parity
    {
        /// <summary>
        /// No parity check occurs.
        /// </summary>
        None = 0,

        /// <summary>
        /// Sets the parity bit so that the count of bits set is an odd number.
        /// </summary>
        Odd = 1,

        /// <summary>
        /// Sets the parity bit so that the count of bits set is an even number.
        /// </summary>
        Even = 2,

        /// <summary>
        /// Leaves the parity bit set to 1.
        /// </summary>
        Mark = 3,

        /// <summary>
        /// Leaves the parity bit set to 0.
        /// </summary>
        Space = 4
    }
}
