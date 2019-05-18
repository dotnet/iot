// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.LEDMatrix
{
    public struct PinMapping
    {
        public PinMapping(int r1, int g1, int b1, int r2, int g2, int b2, int oe, int clock, int latch, int a, int b, int c, int d = 0, int e = 0)
        {
            R1 = r1;
            G1 = g1;
            B1 = b1;

            R2 = r2;
            G2 = g2;
            B2 = b2;

            OE = oe;
            Latch = latch;
            Clock = clock;

            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
        }

        public static PinMapping MatrixBonnetMapping32 =>
            new PinMapping(r1: 5, g1: 13, b1:6, r2: 12, g2: 16, b2:23, oe: 4, clock: 17, latch: 21, a: 22, b: 26, c: 27, d: 20, e: 24);

        public static PinMapping MatrixBonnetMapping64 =>
            new PinMapping(r1: 5, g1: 6, b1:13, r2: 12, g2: 23, b2:16, oe: 4, clock: 17, latch: 21, a: 22, b: 26, c: 27, d: 20, e: 24);

        // Color Pins
        public int R1 { get; set; }
        public int G1 { get; set; }
        public int B1 { get; set; }
        public int R2 { get; set; }
        public int G2 { get; set; }
        public int B2 { get; set; }

        // Control Pins
        public int OE { get; set; } // Output Enable
        public int Clock { get; set; }
        public int Latch { get; set; }

        // Address Pins
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }
        public int E { get; set; }
    }
}


