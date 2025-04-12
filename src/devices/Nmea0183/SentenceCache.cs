// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Caches the last sentence(s) of each type for later retrieval.
    /// This is a helper class for <see cref="AutopilotController"/> and <see cref="PositionProvider"/>. Use <see cref="PositionProvider"/> to query the position from
    /// the most appropriate messages.
    /// </summary>
    public sealed class SentenceCache : IDisposable
    {
        private readonly NmeaSinkAndSource _source;
        private readonly object _lock;

        private readonly Dictionary<int, NmeaSentence> _dinData;
        private readonly Dictionary<SentenceId, NmeaSentence> _sentences;
        private readonly Dictionary<String, Dictionary<SentenceId, NmeaSentence>> _sentencesBySource;
        private readonly ILogger _logger;

        private long _ticksLastCleanup;
        private Queue<RoutePart> _lastRouteSentences;
        private Dictionary<string, Waypoint> _wayPoints;
        private Queue<SatellitesInView> _lastSatelliteInfos;
        private Dictionary<string, TransducerDataSet> _xdrData;

        private SentenceId[] _groupSentences = new SentenceId[]
        {
            // These sentences come in groups or carry multiple different data sets
            new SentenceId("GSV"),
            new SentenceId("RTE"),
            new SentenceId("WPL"),
            new SentenceId("DIN"),
            new SentenceId("XDR"),
        };

        /// <summary>
        /// Creates an new cache using the given source
        /// </summary>
        /// <param name="source">The source to monitor</param>
        public SentenceCache(NmeaSinkAndSource source)
        {
            _source = source;
            _lock = new object();
            _sentences = new Dictionary<SentenceId, NmeaSentence>();
            _sentencesBySource = new Dictionary<String, Dictionary<SentenceId, NmeaSentence>>();
            _lastRouteSentences = new Queue<RoutePart>();
            _lastSatelliteInfos = new Queue<SatellitesInView>();
            _wayPoints = new Dictionary<string, Waypoint>();
            _xdrData = new Dictionary<string, TransducerDataSet>();
            _dinData = new Dictionary<int, NmeaSentence>();
            StoreRawSentences = false;
            _logger = this.GetCurrentClassLogger();
            _source.OnNewSequence += OnNewSequence;
            MaxDataAge = TimeSpan.FromSeconds(30);
            _ticksLastCleanup = 0;
        }

        /// <summary>
        /// True to (also) store raw sentences. Otherwise only recognized decoded sentences are stored.
        /// Defaults to false.
        /// </summary>
        public bool StoreRawSentences
        {
            get;
            set;
        }

        /// <summary>
        /// Maximum age after which any message is discarded. Default 30 Seconds
        /// </summary>
        public TimeSpan MaxDataAge
        {
            get;
            set;
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _sentences.Clear();
                _lastRouteSentences.Clear();
                _wayPoints.Clear();
                _lastSatelliteInfos.Clear();
                _sentencesBySource.Clear();
            }
        }

        /// <summary>
        /// Gets the last sentence of the given type.
        /// Does not return sentences that are part of a group (i.e. GSV, RTE)
        /// </summary>
        /// <param name="id">Sentence Id to query</param>
        /// <returns>The last sentence of that type, or null.</returns>
        public NmeaSentence? GetLastSentence(SentenceId id)
        {
            CleanOutdatedEntries();
            lock (_lock)
            {
                if (_sentences.TryGetValue(id, out var sentence))
                {
                    return sentence;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the last sentence with the given id from the given talker.
        /// </summary>
        /// <param name="source">Source to query</param>
        /// <param name="id">Id to query</param>
        /// <returns>The last sentence of that type and source, null if not found</returns>
        public NmeaSentence? GetLastSentence(string? source, SentenceId id)
        {
            CleanOutdatedEntries();
            if (source == null)
            {
                return GetLastSentence(id);
            }

            lock (_lock)
            {
                if (_sentencesBySource.TryGetValue(source, out var list))
                {
                    if (list.TryGetValue(id, out var sentence))
                    {
                        return sentence;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Tries to get a sentence of the given type
        /// </summary>
        /// <typeparam name="T">The type of the sentence to query</typeparam>
        /// <param name="id">The sentence id for T</param>
        /// <param name="sentence">Receives the sentence, if any was found</param>
        /// <returns>True on success, false if no such message was received</returns>
        public bool TryGetLastSentence<T>(SentenceId id,
            [NotNullWhen(true)]
            out T? sentence)
            where T : NmeaSentence
        {
            var s = GetLastSentence(id);
            if (s is T)
            {
                sentence = (T)s;
                return true;
            }

            sentence = null;
            return false;
        }

        /// <summary>
        /// Gets the last sentence of the given type.
        /// Does not return sentences that are part of a group (i.e. GSV, RTE)
        /// </summary>
        /// <param name="id">Sentence Id to query</param>
        /// <param name="maxAge">Maximum age of the sentence</param>
        /// <returns>The last sentence of that type, or null if none was received within the given timespan.</returns>
        public NmeaSentence? GetLastSentence(SentenceId id, TimeSpan maxAge)
        {
            CleanOutdatedEntries();
            lock (_lock)
            {
                if (_sentences.TryGetValue(id, out var sentence))
                {
                    if (sentence.Age < maxAge)
                    {
                        return sentence;
                    }
                }

                return null;
            }
        }

        private void OnNewSequence(NmeaSinkAndSource? source, NmeaSentence sentence)
        {
            // Cache only valid sentences
            if (!sentence.Valid)
            {
                return;
            }

            if (!StoreRawSentences && sentence is RawSentence)
            {
                return;
            }

            string sourceName;
            if (source == null)
            {
                sourceName = MessageRouter.LocalMessageSource;
                _logger.LogWarning($"Cache got message without source: {sentence.ToNmeaMessage()}");
            }
            else
            {
                sourceName = source.InterfaceName;
            }

            lock (_lock)
            {
                if (!_groupSentences.Contains(sentence.SentenceId))
                {
                    // Standalone sequences. Only the last message needs to be kept
                    _sentences[sentence.SentenceId] = sentence;

                    // We already own the lock to do that a bit more complex update.
                    if (_sentencesBySource.TryGetValue(sourceName, out var dict))
                    {
                        dict[sentence.SentenceId] = sentence;
                    }
                    else
                    {
                        var d = new Dictionary<SentenceId, NmeaSentence>();
                        d[sentence.SentenceId] = sentence;
                        _sentencesBySource[sourceName] = d;
                    }
                }
                else if (sentence.SentenceId == RoutePart.Id && (sentence is RoutePart rte))
                {
                    _lastRouteSentences.Enqueue(rte);
                    while (_lastRouteSentences.Count > 100)
                    {
                        // Throw away old entry
                        _lastRouteSentences.Dequeue();
                    }
                }
                else if (sentence.SentenceId == Waypoint.Id && (sentence is Waypoint wpt))
                {
                    // No reason to clean this up, this will never grow larger than a few hundred entries
                    _wayPoints[wpt.Name] = wpt;
                }
                else if (sentence.SentenceId == SatellitesInView.Id && (sentence is SatellitesInView gsv))
                {
                    _lastSatelliteInfos.Enqueue(gsv);
                    while (_lastSatelliteInfos.Count > 20)
                    {
                        // Throw away old entry
                        _lastSatelliteInfos.Dequeue();
                    }
                }
                else if (sentence.SentenceId == TransducerMeasurement.Id && (sentence is TransducerMeasurement xdr))
                {
                    foreach (var measurement in xdr.DataSets)
                    {
                        _xdrData[measurement.DataName] = measurement;
                    }
                }
                else if (sentence.SentenceId == ProprietaryMessage.Id && (sentence is ProprietaryMessage din))
                {
                    _dinData[din.Identifier] = din;
                }
            }
        }

        /// <summary>
        /// Clean up everything
        /// </summary>
        public void Dispose()
        {
            _source.OnNewSequence -= OnNewSequence;
            _sentences.Clear();
        }

        /// <summary>
        /// Adds the given sentence to the cache - if manual filling is preferred
        /// </summary>
        /// <param name="sentence">Sentence to add</param>
        public void Add(NmeaSentence sentence)
        {
            OnNewSequence(null, sentence);
        }

        /// <summary>
        /// Tries to get a DIN sentence type
        /// </summary>
        /// <typeparam name="T">The type of the sentence to query</typeparam>
        /// <param name="hexId">The hexadecimal identifier for this sub-message</param>
        /// <param name="sentence">Receives the sentence, if any was found</param>
        /// <returns>True on success, false if no such message was received</returns>
        public bool TryGetLastDinSentence<T>(int hexId,
            [NotNullWhen(true)]
            out T? sentence)
            where T : NmeaSentence
        {
            CleanOutdatedEntries();
            // The second condition should always be true, because this list only contains din messages
            if (!_dinData.TryGetValue(hexId, out var s) || s.SentenceId != ProprietaryMessage.Id)
            {
                sentence = null;
                return false;
            }

            if (s is T)
            {
                sentence = (T)s;
                return true;
            }

            sentence = null;
            return false;
        }

        /// <summary>
        /// Gets the last transducer data set (from an XDR sentence, see <see cref="TransducerMeasurement"/>) if one with the given name exists.
        /// </summary>
        /// <param name="name">The name of the data set. Case sensitive</param>
        /// <param name="data">Returns the value if it exists</param>
        /// <returns>True if a value with the given name was found, false otherwise</returns>
        public bool TryGetTransducerData(string name,
            [NotNullWhen(true)]
            out TransducerDataSet? data)
        {
            CleanOutdatedEntries();
            lock (_lock)
            {
                if (_xdrData.TryGetValue(name, out var data1))
                {
                    data = data1;
                    return true;
                }
            }

            data = null;
            return false;
        }

        /// <summary>
        /// Queries the waypoint with the given name
        /// </summary>
        /// <param name="name">The name of the waypoint</param>
        /// <param name="wp">The return data</param>
        /// <returns>True if found, false otherwise</returns>
        public bool TryGetWayPoint(string name,
            [NotNullWhen(true)]
            out Waypoint? wp)
        {
            CleanOutdatedEntries();
            lock (_lock)
            {
                if (_wayPoints.TryGetValue(name, out var innerResult))
                {
                    wp = innerResult!;
                    return true;
                }

                wp = null;
                return false;
            }
        }

        /// <summary>
        /// Returns the last RTE sentences received, to construct the active route.
        /// A set of at most 100 elements is returned, with the newest entry first.
        /// </summary>
        /// <param name="routeParts">The list of RTE sentences</param>
        /// <returns>True if a list was found, false if no RTE messages where received</returns>
        public bool QueryActiveRouteSentences(out List<RoutePart> routeParts)
        {
            CleanOutdatedEntries();
            List<RoutePart> routeSentences;
            lock (_lock)
            {
                // Newest shall be first in list
                routeSentences = _lastRouteSentences.ToList();
                routeSentences.Reverse();
            }

            routeParts = routeSentences;
            return routeSentences.Any();
        }

        /// <summary>
        /// Returns a list of recently received <see cref="SatellitesInView"/> (GSV) messages.
        /// </summary>
        /// <param name="sats">The result</param>
        /// <returns>True if the list was non-empty</returns>
        public bool QuerySatellitesInView(out List<SatellitesInView> sats)
        {
            CleanOutdatedEntries();
            lock (_lock)
            {
                sats = _lastSatelliteInfos.ToList();
                sats.Reverse();
                return sats.Count > 0;
            }
        }

        private void CleanOutdatedEntries()
        {
            var now = Environment.TickCount64;
            if (_ticksLastCleanup < Environment.TickCount64 - 5000)
            {
                _ticksLastCleanup = now;
            }

            lock (_lock)
            {
                // Convert to list, so this can be done in one round
                foreach (var entry in _sentences.ToList())
                {
                    if (entry.Value.Age > MaxDataAge)
                    {
                        _sentences.Remove(entry.Key);
                    }
                }

                if (_lastRouteSentences.All(x => x.Age > MaxDataAge))
                {
                    _lastRouteSentences.Clear();
                }

                if (_lastSatelliteInfos.All(x => x.Age > MaxDataAge))
                {
                    _lastSatelliteInfos.Clear();
                }

                foreach (var entry in _dinData.ToList())
                {
                    if (entry.Value.Age > MaxDataAge)
                    {
                        _dinData.Remove(entry.Key);
                    }
                }

                foreach (var entry in _sentencesBySource.ToList())
                {
                    // If one source is completely dead, remove it
                    if (entry.Value.All(x => x.Value.Age > MaxDataAge))
                    {
                        _sentencesBySource.Remove(entry.Key);
                    }
                }
            }
        }
    }
}
