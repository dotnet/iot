// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO.Ports;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Gps
{
    public sealed class NeoM8 : IDisposable
    {
        private SerialPort _sp;

        /// <summary>
        /// Constructs object representing Neo-M8
        /// </summary>
        /// <param name="serialPortName">Name of the serial port to use for communication</param>
        public NeoM8(string serialPortName)
        {
            _sp = new SerialPort(serialPortName);
            _sp.NewLine = "\r\n";
            _sp.Open();

            // Device streams continuously and therefore most of the time we would end up in the middle of the line
            // therefore ignore first line so that we align correctly
            _sp.ReadLine();
        }

        /// <summary>
        /// Reads NMEA0183 sentence
        /// </summary>
        /// <returns>TalkerSentence instance</returns>
        public TalkerSentence Read()
        {
            string sentence = _sp.ReadLine();
            return TalkerSentence.FromSentenceString(sentence);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _sp?.Dispose();
            _sp = null;
        }
    }
}
