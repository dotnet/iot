// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class NmeaParserTest
    {
        [Fact]
        public void StreamReaderDecodes8BitDataCorrectly()
        {
            var encoding = new Raw8BitEncoding();
            // This was received like this from a Garmin device.
            // The standard windows code page has the letter ä at char code 228. It is correct there and together with
            // the checksum, we know that this is what was intended to be there.
            string sentence = "$GPBOD,16.8,T,14.9,M,Grenze des Beschränkungsgebiete,*A9\r\n";
            byte[] bytes = encoding.GetBytes(sentence);

            // Check this does not contain any character values above 255
            Assert.DoesNotContain(bytes, x => ((int)x >= 256));

            MemoryStream memory = new MemoryStream(bytes);
            StreamReader r = new StreamReader(memory, encoding);

            string readString = r.ReadLine();
            Assert.Equal(sentence.TrimEnd(new char[] { '\r', '\n' }), readString);
        }
    }
}
