// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Corrects the magnetic deviation of an electronic compass.
    /// This calculates the corrected magnetic heading from the measurement of an actual instrument and vice-versa.
    /// Neither input nor output of the calculation are true headings! The magnetic heading needs to still be converted to
    /// a true heading by adding the magnetic declination at the point of observation.
    /// </summary>
    public class MagneticDeviationCorrection : IEquatable<MagneticDeviationCorrection>
    {
        // Only used temporarily during build of the deviation table
        private List<NmeaSentence> _interestingSentences;
        private Angle _magneticVariation;
        private DeviationPoint[] _deviationPointsToCompassReading;
        private DeviationPoint[] _deviationPointsFromCompassReading;
        private Identification? _identification;
        private RawData? _rawData;

        /// <summary>
        /// Create an instance of this class
        /// </summary>
        public MagneticDeviationCorrection()
        {
            _interestingSentences = new List<NmeaSentence>();
            _magneticVariation = Angle.Zero;
            _identification = null;
            _deviationPointsToCompassReading = Array.Empty<DeviationPoint>();
            _deviationPointsFromCompassReading = Array.Empty<DeviationPoint>();
        }

        /// <summary>
        /// Creates a magnetic deviation correction from the given XML file
        /// </summary>
        /// <param name="fileName">The file name to parse</param>
        public MagneticDeviationCorrection(string fileName)
            : this()
        {
            Load(fileName);
        }

        /// <summary>
        /// Creates a magnetic deviation correction from the given XML stream
        /// </summary>
        /// <param name="stream">The stream to parse</param>
        public MagneticDeviationCorrection(Stream stream)
            : this()
        {
            Load(stream);
        }

        /// <summary>
        /// Returns the identification of the vessel for which the loaded calibration is valid
        /// </summary>
        public Identification? Identification
        {
            get
            {
                return _identification;
            }
        }

        /// <summary>
        /// List of NMEA sentences used to perform the calibration.
        /// </summary>
        public List<NmeaSentence> SentencesUsed => _interestingSentences;

        /// <summary>
        /// Tries to calculate a correction from the given recorded NMEA logfile.
        /// The recorded file should contain a data set where the vessel is turning two slow circles, one with the clock and one against the clock,
        /// in calm conditions and with no current.
        /// </summary>
        /// <param name="file">The recorded nmea file (from a logged session)</param>
        public void CreateCorrectionTable(string file)
        {
            CreateCorrectionTable(new[] { file }, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        }

        /// <summary>
        /// Tries to calculate a correction from the given recorded file.
        /// The recorded file should contain a data set where the vessel is turning two slow circles, one with the clock and one against the clock,
        /// in calm conditions and with no current.
        /// </summary>
        /// <param name="stream">The recorded nmea stream (from a logged session)</param>
        public void CreateCorrectionTable(Stream stream)
        {
            CreateCorrectionTable(new[] { stream }, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        }

        /// <summary>
        /// Tries to calculate a correction from the given recorded files, indicating the timespan when the calibration loops were performed.
        /// The recorded file should contain a data set where the vessel is turning two slow circles, one with the clock and one against the clock,
        /// in calm conditions and with no current.
        /// </summary>
        /// <param name="fileSet">The recorded nmea files (from a logged session)</param>
        /// <param name="beginCalibration">The start time of the calibration loops</param>
        /// <param name="endCalibration">The end time of the calibration loops</param>
        public void CreateCorrectionTable(string[] fileSet, DateTimeOffset beginCalibration, DateTimeOffset endCalibration)
        {
            Stream[] streams = new Stream[fileSet.Length];
            for (int i = 0; i < fileSet.Length; i++)
            {
                streams[i] = new FileStream(fileSet[i], FileMode.Open);
            }

            CreateCorrectionTable(streams, beginCalibration, endCalibration);

            foreach (var s in streams)
            {
                s.Dispose();
            }
        }

        /// <summary>
        /// Tries to calculate a correction from the given recorded file strems, indicating the timespan where the calibration loops were performed.
        /// The recorded file should contain a data set where the vessel is turning two slow circles, one with the clock and one against the clock,
        /// in calm conditions and with no current. RMC and HDM messages must be available for this timespan.
        /// </summary>
        /// <param name="fileSet">The recorded nmea files (from a logged session)</param>
        /// <param name="beginCalibration">The start time of the calibration loops</param>
        /// <param name="endCalibration">The end time of the calibration loops</param>
        /// <returns>A list of observed warnings</returns>
        public List<string> CreateCorrectionTable(Stream[] fileSet, DateTimeOffset beginCalibration, DateTimeOffset endCalibration)
        {
            _interestingSentences.Clear();
            _magneticVariation = Angle.Zero;
            _rawData = new RawData();
            var rawCompass = new List<MagneticReading>();
            var rawTrack = new List<GnssReading>();
            List<string> warnings = new List<string>();

            void MessageFilter(NmeaSinkAndSource nmeaSinkAndSource, NmeaSentence nmeaSentence)
            {
                if (nmeaSentence.DateTime < beginCalibration || nmeaSentence.DateTime > endCalibration)
                {
                    return;
                }

                if (nmeaSentence is RecommendedMinimumNavigationInformation rmc)
                {
                    // Track over ground from GPS is useless if not moving
                    if (rmc.Valid && rmc.SpeedOverGround > Speed.FromKnots(0.3))
                    {
                        _interestingSentences.Add(rmc);
                        if (rmc.MagneticVariationInDegrees.HasValue)
                        {
                            _magneticVariation = rmc.MagneticVariationInDegrees.Value;
                        }

                        float delta = 0;
                        if (rawTrack.Count > 0)
                        {
                            delta = rawTrack[rawTrack.Count - 1].TrackReading - (float)rmc.TrackMadeGoodInDegreesTrue.Degrees;
                            // Need to do it the ugly way here - converting back to an angle is also not really nice
                            while (delta > 180)
                            {
                                delta -= 360;
                            }

                            while (delta < -180)
                            {
                                delta += 360;
                            }
                        }

                        rawTrack.Add(new GnssReading()
                        {
                            TimeStamp = rmc.DateTime.DateTime,
                            TrackReading = (float)rmc.TrackMadeGoodInDegreesTrue.Degrees,
                            DeltaToPrevious = delta
                        });
                    }
                }

                if (nmeaSentence is HeadingMagnetic hdm)
                {
                    if (hdm.Valid)
                    {
                        _interestingSentences.Add(hdm);
                        float delta = 0;
                        if (rawCompass.Count > 0)
                        {
                            delta = rawCompass[rawCompass.Count - 1].MagneticCompassReading - (float)hdm.Angle.Degrees;
                            while (delta > 180)
                            {
                                delta -= 360;
                            }

                            while (delta < -180)
                            {
                                delta += 360;
                            }
                        }

                        rawCompass.Add(new MagneticReading()
                        {
                            TimeStamp = hdm.DateTime.DateTime, MagneticCompassReading = (float)hdm.Angle.Degrees,
                            DeltaToPrevious = delta
                        });
                    }
                }
            }

            NmeaLogDataReader reader = new NmeaLogDataReader("Reader", fileSet);
            reader.DecodeInRealtime = false;
            reader.OnNewSequence += MessageFilter;
            reader.StartDecode();
            reader.StopDecode();
            reader.Dispose();
            _rawData.Compass = rawCompass.ToArray();
            _rawData.Track = rawTrack.ToArray();
            DeviationPoint[] circle = new DeviationPoint[360]; // One entry per degree
            // This will get the average offset, which is assumed to be orientation independent (i.e. if the magnetic compass's forward
            // direction doesn't properly align with the ship)
            double averageOffset = 0;
            for (int i = 0; i < 360; i++)
            {
                FindAllTracksWith(i, out var tracks, out var headings);
                if (tracks.Count > 0 && headings.Count > 0)
                {
                    Angle averageTrack; // Computed from COG (GPS course)
                    if (!tracks.TryAverageAngle(out averageTrack))
                    {
                        averageTrack = tracks[0]; // Should be a rare case - just use the first one then
                    }

                    Angle magneticTrack = averageTrack - _magneticVariation; // Now in degrees magnetic
                    magneticTrack = magneticTrack.Normalize(true);
                    // This should be i + 0.5 if the data is good
                    Angle averageHeading;
                    if (!headings.TryAverageAngle(out averageHeading))
                    {
                        averageHeading = headings[0];

                    }

                    double deviation = (averageHeading - magneticTrack).Normalize(false).Degrees;
                    var pt = new DeviationPoint()
                    {
                        // First is less "true" than second, so CompassReading + Deviation => MagneticHeading
                        CompassReading = (float)averageHeading.Normalize(true).Degrees,
                        MagneticHeading = (float)magneticTrack.Normalize(true).Degrees,
                        Deviation = (float)-deviation,
                    };

                    averageOffset += deviation;
                    circle[i] = pt;
                }
            }

            averageOffset /= circle.Count(x => x != null);
            int numberOfConsecutiveGaps = 0;
            const int maxConsecutiveGaps = 5;
            // Evaluate the quality of the result
            DeviationPoint? previous = null;
            for (int i = 0; i < 360; i++)
            {
                var pt = circle[i];
                if (pt == null)
                {
                    numberOfConsecutiveGaps++;
                    if (numberOfConsecutiveGaps > maxConsecutiveGaps)
                    {
                        throw new InvalidDataException($"Not enough data points. There is not enough data near heading {i} degrees. Total number of points {_interestingSentences.Count}");
                    }
                }
                else
                {
                    if (Math.Abs(pt.Deviation + averageOffset) > 30)
                    {
                        warnings.Add($"Your magnetic compass shows deviations of {pt.Deviation + averageOffset} degrees. Use a better installation location or buy a new one.");
                    }

                    numberOfConsecutiveGaps = 0;
                    previous = pt;
                }
            }

            // Validate again
            for (int i = 0; i < 360; i++)
            {
                var pt = circle[i];
                if (pt == null)
                {
                    numberOfConsecutiveGaps++;
                    if (numberOfConsecutiveGaps > maxConsecutiveGaps)
                    {
                        throw new InvalidDataException($"Not enough data points after cleanup. There is not enough data near heading {i} degrees");
                    }
                }
                else
                {
                    numberOfConsecutiveGaps = 0;
                }
            }

            CalculateSmoothing(circle);

            // Now create the inverse of the above map, to get from compass reading back to undisturbed magnetic heading
            _deviationPointsFromCompassReading = circle;
            _deviationPointsToCompassReading = Array.Empty<DeviationPoint>();

            circle = new DeviationPoint[360];
            for (int i = 0; i < 360; i++)
            {
                var ptToUse =
                    _deviationPointsFromCompassReading.FirstOrDefault(x => (int)x.MagneticHeading == i);

                int offs = 1;
                while (ptToUse == null)
                {
                    ptToUse =
                        _deviationPointsFromCompassReading.FirstOrDefault(x => (int)x.MagneticHeading == (i + offs) % 360 ||
                                                                             (int)x.MagneticHeading == (i + 360 - offs) % 360);
                    offs++;
                }

                circle[i] = new DeviationPoint()
                {
                    CompassReading = ptToUse.CompassReading,
                    CompassReadingSmooth = ptToUse.CompassReadingSmooth,
                    Deviation = ptToUse.Deviation,
                    DeviationSmooth = ptToUse.DeviationSmooth,
                    MagneticHeading = ptToUse.MagneticHeading
                };
            }

            _deviationPointsToCompassReading = circle;

            return warnings;
        }

        private static void CalculateSmoothing(DeviationPoint[] circle)
        {
            for (int i = 0; i < 360; i++)
            {
                const int smoothingPoints = 10; // each side
                double avgDeviation = 0;
                int usedPoints = 0;
                for (int k = i - smoothingPoints; k <= i + smoothingPoints; k++)
                {
                    var ptIn = circle[(k + 360) % 360];
                    if (ptIn != null)
                    {
                        avgDeviation += ptIn.Deviation;
                        usedPoints++;
                    }
                }

                avgDeviation /= usedPoints;
                if (circle[i] != null)
                {
                    circle[i].DeviationSmooth = (float)avgDeviation;
                    // The compass reading we get if we apply the smoothed deviation
                    circle[i].CompassReadingSmooth = (float)Angle.FromDegrees(circle[i].MagneticHeading - avgDeviation).Normalize(true).Degrees;
                }
                else
                {
                    float avgReading = i + 0.5f;
                    circle[i] = new DeviationPoint()
                    {
                        CompassReading = (float)avgReading, // Constructed from the result
                        CompassReadingSmooth = (float)avgReading,
                        MagneticHeading = (float)Angle.FromDegrees(avgReading + avgDeviation).Normalize(true).Degrees,
                        Deviation = (float)avgDeviation,
                        DeviationSmooth = (float)avgDeviation
                    };
                }
            }
        }

        /// <summary>
        /// Saves the calculated calibration set to a file
        /// </summary>
        /// <param name="file">The file name (should be ending in XML)</param>
        /// <param name="shipName">The name of the vessel</param>
        /// <param name="callSign">The callsign of the vessel</param>
        /// <param name="mmsi">The MMSI of the vessel</param>
        public void Save(string file, string shipName, string callSign, string mmsi)
        {
            CompassCalibration topLevel = new CompassCalibration();
            var calibTimeStamp = DateTime.UtcNow;
            var lastSentence = _interestingSentences.OrderBy(x => x.DateTime).LastOrDefault();
            if (lastSentence != null)
            {
                calibTimeStamp = lastSentence.DateTime.DateTime;
            }

            var id = new Identification
            {
                CalibrationDate = calibTimeStamp, ShipName = shipName, Callsign = callSign, MMSI = mmsi
            };
            topLevel.CalibrationDataToCompassReading = _deviationPointsToCompassReading;
            topLevel.CalibrationDataFromCompassReading = _deviationPointsFromCompassReading;
            topLevel.Identification = id;
            topLevel.RawDataReadings = _rawData;

            _identification = id;
            XmlSerializer ser = new XmlSerializer(topLevel.GetType());

            using (StreamWriter tw = new StreamWriter(file))
            {
                ser.Serialize(tw, topLevel);
                tw.Close();
                tw.Dispose();
            }
        }

        /// <summary>
        /// Loads a previously saved calibration set
        /// </summary>
        /// <param name="file">The file from which to load</param>
        public void Load(string file)
        {
            using (var fs = new FileStream(file, FileMode.Open))
            {
                Load(fs);
            }
        }

        /// <summary>
        /// Loads a previously saved calibration set
        /// </summary>
        /// <param name="file">The stream from which to load</param>
        public void Load(Stream file)
        {
            XmlSerializer ser = new XmlSerializer(typeof(CompassCalibration));

            CompassCalibration? topLevel = null;

            using (StreamReader tw = new StreamReader(file))
            {
                topLevel = (CompassCalibration)ser.Deserialize(tw)!;
                tw.Close();
                tw.Dispose();
            }

            _identification = topLevel.Identification;
            _deviationPointsToCompassReading = topLevel.CalibrationDataToCompassReading;
            _deviationPointsFromCompassReading = topLevel.CalibrationDataFromCompassReading;
        }

        private void FindAllTracksWith(double direction, out List<Angle> tracks, out List<Angle> headings)
        {
            tracks = new List<Angle>();
            headings = new List<Angle>();

            bool useNextTrack = false;
            DateTimeOffset? timeOfHdm = default;
            foreach (var s in _interestingSentences)
            {
                if (s is HeadingMagnetic hdm)
                {
                    if (!hdm.Valid)
                    {
                        continue;
                    }

                    if (Math.Abs(Math.Floor(hdm.Angle.Degrees) - direction) > 1E-8)
                    {
                        continue;
                    }

                    // Now we have an entry hdt that is a true heading more or less pointing to direction
                    headings.Add(hdm.Angle);
                    useNextTrack = true;
                    timeOfHdm = hdm.DateTime;
                }

                // Select the first track after the last matching heading received
                if (s is RecommendedMinimumNavigationInformation rmc && useNextTrack)
                {
                    useNextTrack = false;
                    if (rmc.DateTime - timeOfHdm < TimeSpan.FromSeconds(5))
                    {
                        // Only if this is near the corresponding heading message
                        tracks.Add(rmc.TrackMadeGoodInDegreesTrue);
                    }
                }
            }
        }

        /// <summary>
        /// Convert a magnetic heading to a compass reading (to tell the helmsman what he should steer on the compass for the desired magnetic course)
        /// </summary>
        /// <param name="magneticHeading">Magnetic heading input</param>
        /// <returns>The compass reading for the given magnetic heading</returns>
        public Angle FromMagneticHeading(Angle magneticHeading)
        {
            if (_deviationPointsToCompassReading == null)
            {
                throw new InvalidOperationException("Deviation table not initialized");
            }

            int ptIndex = (int)(magneticHeading.Normalize(true).Degrees);
            var ptToUse = _deviationPointsToCompassReading[ptIndex];
            return (magneticHeading - Angle.FromDegrees(ptToUse.DeviationSmooth)).Normalize(true);
        }

        /// <summary>
        /// Convert a compass reading to a magnetic heading.
        /// </summary>
        /// <param name="compassReading">Reading of the compass</param>
        /// <returns>The corrected magnetic heading</returns>
        public Angle ToMagneticHeading(Angle compassReading)
        {
            if (_deviationPointsFromCompassReading == null)
            {
                throw new InvalidOperationException("Deviation table not initialized");
            }

            int ptIndex = (int)(compassReading.Normalize(true).Degrees);
            var ptToUse = _deviationPointsFromCompassReading[ptIndex];
            return (compassReading + Angle.FromDegrees(ptToUse.DeviationSmooth)).Normalize(true);
        }

        /// <summary>
        /// Compares two deviation data sets for equality. Minor differences are ignored.
        /// </summary>
        /// <param name="other">The other object</param>
        /// <returns>True on equality, false otherwise</returns>
        public bool Equals(MagneticDeviationCorrection? other)
        {
           return Equals(other, out _);
        }

        /// <summary>
        /// Compares two deviation data sets for equality. Minor differences are ignored.
        /// </summary>
        /// <param name="other">The other object</param>
        /// <param name="firstDifference">A short error message where the error is</param>
        /// <returns>True on equality, false otherwise</returns>
        public bool Equals(MagneticDeviationCorrection? other, out string firstDifference)
        {
            if (other == null)
            {
                firstDifference = "Comparing with null";
                return false;
            }

            if (!Equals(Identification, other.Identification))
            {
                firstDifference = "Identification is not same";
                return false;
            }

            if (_deviationPointsFromCompassReading.Length != other._deviationPointsFromCompassReading.Length)
            {
                firstDifference = "Calibration has a different number of compass->magnetic points.";
                return false;
            }

            if (_deviationPointsToCompassReading.Length != other._deviationPointsToCompassReading.Length)
            {
                firstDifference = "Calibration has a different number of magnetic->compass points.";
                return false;
            }

            for (int i = 0; i < _deviationPointsFromCompassReading.Length; i++)
            {
                DeviationPoint left = _deviationPointsFromCompassReading[i];
                DeviationPoint right = other._deviationPointsFromCompassReading[i];
                if (!left.Equals(right))
                {
                    firstDifference = $"Points differ at index {i}, Compass reading {left.CompassReading}";
                    return false;
                }
            }

            for (int i = 0; i < _deviationPointsToCompassReading.Length; i++)
            {
                DeviationPoint left = _deviationPointsToCompassReading[i];
                DeviationPoint right = other._deviationPointsToCompassReading[i];
                if (!left.Equals(right))
                {
                    firstDifference = $"Points differ at index {i}, Compass reading {left.CompassReading}";
                    return false;
                }
            }

            firstDifference = string.Empty;
            return true;
        }
    }
}
