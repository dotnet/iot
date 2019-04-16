// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio.Drivers;
using System.Runtime.CompilerServices;

namespace Iot.Device.LEDMatrix
{
    internal sealed class Gpio : RaspberryPi3Driver
    {
        internal ulong AMask { get; set;}
        internal ulong BMask { get; set;}
        internal ulong CMask { get; set;}
        internal ulong DMask { get; set;}
        internal ulong EMask { get; set;}
        internal ulong ABCDEMask { get; set;}
        internal ulong OEMask { get; set;}
        internal ulong ClockMask { get; set;}
        internal ulong LatchMask { get; set;}

        internal ulong R1Mask { get; set;}
        internal ulong G1Mask { get; set;}
        internal ulong B1Mask { get; set;}

        internal ulong R2Mask { get; set;}
        internal ulong G2Mask { get; set;}
        internal ulong B2Mask { get; set;}
        internal ulong AllColorsMask { get; set;}

        internal Gpio(PinMapping mapping, int rows)
        {
            AMask = 1U << mapping.A;
            BMask = 1U << mapping.B;
            CMask = 1U << mapping.C;

            ABCDEMask = AMask | BMask | CMask;

            if (rows > 16)
            {
                DMask = 1U << mapping.D;
                ABCDEMask |= DMask;
            }

            if (rows > 32)
            {
                EMask = 1U << mapping.E;
                ABCDEMask |= EMask;
            }

            OEMask = 1U << mapping.OE;
            ClockMask = 1U << mapping.Clock;
            LatchMask = 1U << mapping.Latch;

            R1Mask = 1U << mapping.R1;
            G1Mask = 1U << mapping.G1;
            B1Mask = 1U << mapping.B1;
            R2Mask = 1U << mapping.R2;
            G2Mask = 1U << mapping.G2;
            B2Mask = 1U << mapping.B2;

            AllColorsMask = R1Mask | G1Mask | B1Mask | R2Mask | G2Mask | B2Mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteSet(ulong mask)   => SetRegister = mask;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteClear(ulong mask) => ClearRegister = mask;
    }
}
