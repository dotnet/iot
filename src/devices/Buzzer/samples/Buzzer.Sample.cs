// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Buzzer.Samples
{
    class Program
    {
        static IList<MelodyElement> AbcSong = new List<MelodyElement>()
            {
                new NoteElement(Note.C, Octave.Fourth, Duration.Quarter),   // A
                new NoteElement(Note.C, Octave.Fourth, Duration.Quarter),   // B
                new NoteElement(Note.G, Octave.Fourth, Duration.Quarter),   // C
                new NoteElement(Note.G, Octave.Fourth, Duration.Quarter),   // D

                new NoteElement(Note.A, Octave.Fourth, Duration.Quarter),   // E
                new NoteElement(Note.A, Octave.Fourth, Duration.Quarter),   // F
                new NoteElement(Note.G, Octave.Fourth, Duration.Half),      // G

                new NoteElement(Note.F, Octave.Fourth, Duration.Quarter),   // H
                new NoteElement(Note.F, Octave.Fourth, Duration.Quarter),   // I
                new NoteElement(Note.E, Octave.Fourth, Duration.Quarter),   // J
                new NoteElement(Note.E, Octave.Fourth, Duration.Quarter),   // K

                new NoteElement(Note.D, Octave.Fourth, Duration.Eighth),    // L
                new NoteElement(Note.D, Octave.Fourth, Duration.Eighth),    // M
                new NoteElement(Note.D, Octave.Fourth, Duration.Eighth),    // N
                new NoteElement(Note.D, Octave.Fourth, Duration.Eighth),    // O
                new NoteElement(Note.C, Octave.Fourth, Duration.Half),      // P
                
                new NoteElement(Note.G, Octave.Fourth, Duration.Quarter),   // Q
                new NoteElement(Note.G, Octave.Fourth, Duration.Quarter),   // R
                new NoteElement(Note.F, Octave.Fourth, Duration.Half),      // S
                
                new NoteElement(Note.E, Octave.Fourth, Duration.Quarter),   // T
                new NoteElement(Note.E, Octave.Fourth, Duration.Quarter),   // U
                new NoteElement(Note.D, Octave.Fourth, Duration.Half),      // V

                new NoteElement(Note.G, Octave.Fourth, Duration.Eighth),    // Dou-
                new NoteElement(Note.G, Octave.Fourth, Duration.Eighth),    // ble
                new NoteElement(Note.G, Octave.Fourth, Duration.Quarter),   // U
                new NoteElement(Note.F, Octave.Fourth, Duration.Half),      // X

                new NoteElement(Note.E, Octave.Fourth, Duration.Quarter),   // Y
                new NoteElement(Note.E, Octave.Fourth, Duration.Quarter),   // and
                new NoteElement(Note.D, Octave.Fourth, Duration.Half)       // Z
            };

        static void Main(string[] args)
        {
            using (var player1 = new MelodyPlayer(new Buzzer(21, -1)))
            using (var player2 = new MelodyPlayer(new Buzzer(26, -1)))
            {
                Task.WaitAll(
                    Task.Run(() => player1.Play(AbcSong, 100, -12)),
                    Task.Run(() => player2.Play(AbcSong, 100)));
            }
        }
    }
}
