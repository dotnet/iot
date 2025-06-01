// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// A filter rule for the <see cref="MessageRouter"/>.
    /// </summary>
    public class FilterRule
    {
        private readonly bool _rawMessagesOnly;

        /// <summary>
        /// A standard filter rule. When the filter matches
        /// </summary>
        /// <param name="sourceName">Name of the source (Nmea stream name) for which the filter applies or * for all</param>
        /// <param name="talkerId">TalkerId for which the rule applies or <see cref="TalkerId.Any"/></param>
        /// <param name="sentenceId">SentenceId for which the rule applies or <see cref="SentenceId.Any"/></param>
        /// <param name="sinks">Where to send the message when the filter matches</param>
        /// <param name="rawMessagesOnly">The filter matches raw messages only. This is the default, because otherwise known message
        /// types would be implicitly duplicated on forwarding</param>
        /// <param name="continueAfterMatch">True to continue processing after a match, false to stop processing this message</param>
        public FilterRule(string sourceName, TalkerId talkerId, SentenceId sentenceId, IEnumerable<string> sinks, bool rawMessagesOnly = true,
            bool continueAfterMatch = false)
        {
            _rawMessagesOnly = rawMessagesOnly;
            SourceName = sourceName;
            TalkerId = talkerId;
            SentenceId = sentenceId;
            Sinks = sinks;
            ContinueAfterMatch = continueAfterMatch;
        }

        /// <summary>
        /// A standard filter rule. When the filter matches
        /// </summary>
        /// <param name="sourceName">Name of the source (Nmea stream name) for which the filter applies or * for all</param>
        /// <param name="talkerId">TalkerId for which the rule applies or <see cref="TalkerId.Any"/></param>
        /// <param name="sentenceId">SentenceId for which the rule applies or <see cref="SentenceId.Any"/></param>
        /// <param name="sinks">Where to send the message when the filter matches</param>
        /// <param name="forwardingAction">When the message is about to be sent, this method is called that can modify the
        /// message. First arg is the source of the message, second the designated sink.</param>
        /// <param name="rawMessagesOnly">The filter matches raw messages only. This is the default, because otherwise known message
        /// types would be implicitly duplicated on forwarding</param>
        /// <param name="continueAfterMatch">True to continue processing after a match, false to stop processing this message</param>
        public FilterRule(string sourceName, TalkerId talkerId, SentenceId sentenceId, IEnumerable<string> sinks,
            Func<NmeaSinkAndSource, NmeaSinkAndSource, NmeaSentence, NmeaSentence?> forwardingAction, bool rawMessagesOnly = true,
            bool continueAfterMatch = false)
        {
            _rawMessagesOnly = rawMessagesOnly;
            SourceName = sourceName;
            TalkerId = talkerId;
            SentenceId = sentenceId;
            Sinks = sinks;
            ForwardingAction = forwardingAction;
            ContinueAfterMatch = continueAfterMatch;
        }

        /// <summary>
        /// Name of the source for which this filter shall apply
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// TalkerId for which this filter applies
        /// </summary>
        public TalkerId TalkerId { get; }

        /// <summary>
        /// SentenceId for which this filter applies
        /// </summary>
        public SentenceId SentenceId { get; }

        /// <summary>
        /// Action this filter performs
        /// </summary>
        public IEnumerable<String> Sinks { get; }

        /// <summary>
        /// If non-null, this action can modify the message before it is being sent to the
        /// indicated sink.
        /// Note that the input message shall not be modified, clone it if necessary.
        /// The return value can be null to suppress the message. That way, advanced filter testing can be done using this callback.
        /// </summary>
        public Func<NmeaSinkAndSource, NmeaSinkAndSource, NmeaSentence, NmeaSentence?>? ForwardingAction { get; }

        /// <summary>
        /// If this is true, filter testing is continued even after a match.
        /// If it is false (the default), no further filters are tested after the first match (which typically means
        /// that a message is only matching one filter)
        /// </summary>
        public bool ContinueAfterMatch { get; }

        /// <summary>
        /// True if this filter matches the given sentence and source
        /// </summary>
        public bool SentenceMatch(string nmeaSource, NmeaSentence sentence)
        {
            if (sentence.Valid)
            {
                if (!(sentence is RawSentence) && _rawMessagesOnly)
                {
                    // Non-raw sentences are thrown away by default
                    return false;
                }

                if (TalkerId != TalkerId.Any)
                {
                    if (TalkerId != sentence.TalkerId)
                    {
                        return false;
                    }
                }

                if (SentenceId != Iot.Device.Nmea0183.SentenceId.Any)
                {
                    if (SentenceId != sentence.SentenceId)
                    {
                        return false;
                    }
                }

                if (SourceName != "*" && !string.IsNullOrWhiteSpace(SourceName))
                {
                    if (SourceName != nmeaSource)
                    {
                        return false;
                    }
                }

                return true;
            }

            // Invalid sentences can't be routed anywhere
            return false;
        }
    }
}
