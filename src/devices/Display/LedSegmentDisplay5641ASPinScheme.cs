// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;

namespace Display
{
    /// <summary>
    /// Display mode of the segment display, representing current flow through the pins
    /// </summary>
    public enum Led7SegmentDisplayMode
    {
        /// <summary>
        /// A common anode display is ON when a high value is sent to the pin
        /// </summary>
        CommonAnode = 0,

        /// <summary>
        /// A common cathode display is OFF when a high value is sent to the pin. Common for LEDs.
        /// </summary>
        CommonCathode = 1
    }

    /// <summary>Default pin scheme for the 5641AS 4-digit 7-segment display</summary>
    /// <remarks>
    ///
    ///      AAA
    ///     F   B
    ///     F   B
    ///      GGG
    ///     E   C
    ///     E   C
    ///      DDD   DP
    ///
    ///       12 11 10  9  8  7
    ///  |~~~~ |  |  |  |  |  | ~~~~|
    ///  |                          |
    ///  |                          |
    ///  |~~~~ |  |  |  |  |  | ~~~~|
    ///        1  2  3  4  5  6
    ///
    ///  Segments
    ///  A = 11
    ///  B = 7
    ///  C = 4
    ///  D = 2
    ///  E = 1
    ///  F = 10
    ///  G = 5
    ///  DP = 3
    ///
    ///  Digits
    ///  Dig1 = 12
    ///  Dig2 = 9
    ///  Dig3 = 8
    ///  Dig4 = 6
    ///
    ///
    /// </remarks>
    public class LedSegmentDisplay5641ASPinScheme
    {
        /// <summary>
        /// Creates the pin scheme for the 5641AS display, indicating which pin on the board
        /// is mapped to each pin on the display. See data sheet and remarks for pin layout.
        /// </summary>
        public LedSegmentDisplay5641ASPinScheme(int boardPinA, int boardPinB, int boardPinC, int boardPinD,
            int boardPinE, int boardPinF, int boardPinG, int boardPinDP,
            int boardPinDig1, int boardPinDig2, int boardPinDig3, int boardPinDig4)
        {
            DisplayMode = Led7SegmentDisplayMode.CommonCathode;

            A = boardPinA;
            B = boardPinB;
            C = boardPinC;
            D = boardPinD;
            E = boardPinE;
            F = boardPinF;
            G = boardPinG;
            DP = boardPinDP;

            Dig1 = boardPinDig1;
            Dig2 = boardPinDig2;
            Dig3 = boardPinDig3;
            Dig4 = boardPinDig4;
        }

        /// <summary>
        /// Board pin number connected to Top segment 'A'
        /// </summary>
        public int A { get; set; }

        /// <summary>
        /// Board pin number connected to TopRight segment 'B'
        /// </summary>
        public int B { get; set; }

        /// <summary>
        /// Board pin number connected to BottomRight segment 'C'
        /// </summary>
        public int C { get; set; }

        /// <summary>
        /// Board pin number connected to Bottom segment 'D'
        /// </summary>
        public int D { get; set; }

        /// <summary>
        /// Board pin number connected to BottomLeft segment 'E'
        /// </summary>
        public int E { get; set; }

        /// <summary>
        /// Board pin number connected to TopLeft segment 'F'
        /// </summary>
        public int F { get; set; }

        /// <summary>
        /// Board pin number connected to Middle segment 'G'
        /// </summary>
        public int G { get; set; }

        /// <summary>
        /// Board pin number connected to Dot segment 'DP'
        /// </summary>
        public int DP { get; set; }

        /// <summary>
        /// Board pin number connected to Digit1 (leftmost)
        /// </summary>
        public int Dig1 { get; set; }

        /// <summary>
        /// Board pin number connected to Digit2 (second from left)
        /// </summary>
        public int Dig2 { get; set; }

        /// <summary>
        /// Board pin number connected to Digit3 (second from right)
        /// </summary>
        public int Dig3 { get; set; }

        /// <summary>
        /// Board pin number connected to Digit4 (rightmost)
        /// </summary>
        public int Dig4 { get; set; }

        /// <summary>
        /// Collection of all segment pin assignments, shared across all digits
        /// </summary>
        public int[] Segments => new int[] { A, B, C, D, E, F, G, DP };

        /// <summary>
        /// Collection of all digit pin assignments
        /// </summary>
        public int[] Digits => new int[] { Dig1, Dig2, Dig3, Dig4 };

        /// <summary>
        /// Number of digit spaces on the display
        /// </summary>
        public int DigitCount => Digits.Length;

        /// <summary>
        /// Number of segments per digit
        /// </summary>
        public int SegmentCountPerDigit => Segments.Length;

        /// <summary>
        /// The configured display mode of the device
        /// </summary>
        public Led7SegmentDisplayMode DisplayMode { get; set; }

        /// <summary>
        /// A common cathode display is OFF when a high value is sent to the pin. Applies to the digit only
        /// </summary>
        public PinValue DigitDisplayOff => DisplayMode == Led7SegmentDisplayMode.CommonCathode ? PinValue.High : PinValue.Low;
    }
}
