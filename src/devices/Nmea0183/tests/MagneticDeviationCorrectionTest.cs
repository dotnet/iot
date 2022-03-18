// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public sealed class MagneticDeviationCorrectionTest
    {
        [Fact]
        public void CreateDeviationTable()
        {
            MagneticDeviationCorrection dev = new MagneticDeviationCorrection();
            dev.CreateCorrectionTable(
                "../../../Nmea-2020-07-23-12-02.txt");

            dev.Save("Calibration_Cirrus.xml", "Cirrus", "HBY5127", "269110660");
            Assert.True(File.Exists("Calibration_Cirrus.xml"));
        }

        [Fact]
        public void CreateDeviationTable4()
        {
            using (new SetCultureForTest("de-DE"))
            {
                MagneticDeviationCorrection dev = new MagneticDeviationCorrection();
                dev.CreateCorrectionTable(new string[]
                    {
                        "../../../Nmea-2021-08-25-16-25.txt", "../../../Nmea-2021-08-25-16-49.txt"
                    },
                    DateTimeOffset.Parse("2021-08-25T16:47:00", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                    DateTimeOffset.Parse("2021-08-25T16:53:00", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal));

                dev.Save("Calibration_Cirrus_v4.xml", "Cirrus", "HBY5127", "269110660");

                var expected = new MagneticDeviationCorrection("../../../Calibration_Cirrus_v3.xml");
                var actual = new MagneticDeviationCorrection("Calibration_Cirrus_v4.xml");
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DifferentCalibrationsAreNotEqual()
        {
            var first = new MagneticDeviationCorrection("../../../Calibration_Cirrus_v3.xml");
            var second = new MagneticDeviationCorrection("../../../Calibration_Cirrus_v1.xml");
            Assert.NotEqual(first, second);
        }

        [Fact]
        public void ReadAndUseDeviationTable()
        {
            MagneticDeviationCorrection dev = new MagneticDeviationCorrection();
            dev.Load("../../../Calibration_Cirrus_v3.xml");

            Assert.True(dev.Identification != null);
            Assert.Equal("Cirrus", dev.Identification!.ShipName);
            Assert.Equal(323.47342376709, dev.ToMagneticHeading(Angle.FromDegrees(303.3)).Degrees, 3);
            Assert.Equal(297.955488967895, dev.FromMagneticHeading(Angle.FromDegrees(316.743820953369)).Degrees, 3);

            // For all angles, converting back and forth should result in a small delta (not exactly zero though, since the
            // operation is not exactly invertible)
            for (double d = 0.5; d < 361; d += 1.0)
            {
                Angle backAndForth = dev.FromMagneticHeading(dev.ToMagneticHeading(Angle.FromDegrees(d)));
                Angle delta = backAndForth - Angle.FromDegrees(d);
                Assert.True(Math.Abs(delta.Normalize(false).Degrees) < 8, $"Delta: {delta}");
            }
        }
    }
}
