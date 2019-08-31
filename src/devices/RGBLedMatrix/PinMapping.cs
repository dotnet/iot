// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.LEDMatrix
{
    /// <summary>
    /// Represents pin mapping for LED matrix
    /// </summary>
    public struct PinMapping
    {
        /// <summary>
        /// Constructs PinMapping instance
        /// </summary>
        /// <param name="r1">First pin of the red channel</param>
        /// <param name="g1">First pin of the green channel</param>
        /// <param name="b1">First pin of the blue channel</param>
        /// <param name="r2">Second pin of the red channel</param>
        /// <param name="g2">Second pin of the green channel</param>
        /// <param name="b2">Second pin of the blue channel</param>
        /// <param name="oe">Output enable pin</param>
        /// <param name="clock">Clock pin</param>
        /// <param name="latch">Latch pin</param>
        /// <param name="a">Address pin A</param>
        /// <param name="b">Address pin B</param>
        /// <param name="c">Address pin C</param>
        /// <param name="d">Address pin D</param>
        /// <param name="e">Address pin E</param>
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

        /// <summary>
        /// Default mapping for 32x32 matrix using bonnet
        /// </summary>
        public static PinMapping MatrixBonnetMapping32 =>
            new PinMapping(r1: 5, g1: 13, b1:6, r2: 12, g2: 16, b2:23, oe: 4, clock: 17, latch: 21, a: 22, b: 26, c: 27, d: 20, e: 24);

        /// <summary>
        /// Default mapping for 64x64 matrix using bonnet
        /// </summary>
        public static PinMapping MatrixBonnetMapping64 =>
            new PinMapping(r1: 5, g1: 6, b1:13, r2: 12, g2: 23, b2:16, oe: 4, clock: 17, latch: 21, a: 22, b: 26, c: 27, d: 20, e: 24);

        // Color Pins

        /// <summary>
        /// First pin of the red channel
        /// </summary>
        public int R1 { get; set; }

        /// <summary>
        /// First pin of the green channel
        /// </summary>
        public int G1 { get; set; }

        /// <summary>
        /// First pin of the blue channel
        /// </summary>
        public int B1 { get; set; }

        /// <summary>
        /// Second pin of the red channel
        /// </summary>
        public int R2 { get; set; }

        /// <summary>
        /// Second pin of the green channel
        /// </summary>
        public int G2 { get; set; }

        /// <summary>
        /// Second pin of the blue channel
        /// </summary>
        public int B2 { get; set; }

        // Control Pins

        /// <summary>
        /// Output enable pin
        /// </summary>
        public int OE { get; set; }

        /// <summary>
        /// Clock pin
        /// </summary>
        public int Clock { get; set; }

        /// <summary>
        /// Latch pin
        /// </summary>
        public int Latch { get; set; }

        // Address Pins

        /// <summary>
        /// Address pin A
        /// </summary>
        public int A { get; set; }

        /// <summary>
        /// Address pin B
        /// </summary>
        public int B { get; set; }

        /// <summary>
        /// Address pin C
        /// </summary>
        public int C { get; set; }

        /// <summary>
        /// Address pin D
        /// </summary>
        public int D { get; set; }

        /// <summary>
        /// Address pin E
        /// </summary>
        public int E { get; set; }
    }
}
