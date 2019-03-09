// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Buzzer.Samples
{
    /// <summary>
    /// Represents all twelve notes.
    /// </summary>
    public enum Note
    {
        C = 1,
        Db = 2,
        D = 3,
        Eb = 4,
        E = 5,
        F = 6,
        Gb = 7,
        G = 8,
        Ab = 9,
        A = 10,
        Bb = 11,
        B = 12,
    }

    /// <summary>
    /// Represents music octave.
    /// </summary>
    public enum Octave
    {
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Fifth = 5,
        Sixth = 6,
        Seventh = 7,
        Eighth = 8
    }

    /// <summary>
    /// Represents music note duration.
    /// </summary>
    public enum Duration
    {
        Whole = 1,
        Half = 2,
        Quarter = 4,
        Eighth = 8,
        Sixteenth = 16,
    }
}
