// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// This abstract class describes an NMEA data source or data sink.
    /// It can be a connection, a data processor or an output device.
    /// </summary>
    public abstract class NmeaSinkAndSource : IDisposable
    {
        /// <summary>
        /// This is fired when a new position is available
        /// </summary>
        public virtual event PositionUpdate? OnNewPosition;

        /// <summary>
        /// This is fired when the time is updated
        /// </summary>
        public virtual event Action<DateTimeOffset>? OnNewTime;

        /// <summary>
        /// This is fired on every new sentence
        /// </summary>
        public virtual event Action<NmeaSinkAndSource, NmeaSentence>? OnNewSequence;

        /// <summary>
        /// This is fired when a message couldn't be parsed
        /// </summary>
        public virtual event Action<NmeaSinkAndSource, string, NmeaError>? OnParserError;

        /// <summary>
        /// Constructs a message sink
        /// </summary>
        /// <param name="interfaceName">Name of the interface (mostly used for logging purposes)</param>
        protected NmeaSinkAndSource(string interfaceName)
        {
            InterfaceName = interfaceName;
        }

        /// <summary>
        /// Name of the interface
        /// </summary>
        public string InterfaceName
        {
            get;
        }

        /// <summary>
        /// Start receiving messages from this interface.
        /// An implementation should open streams, connect to sockets or create receiver threads, as appropriate.
        /// </summary>
        public abstract void StartDecode();

        /// <summary>
        /// Send the message to the device.
        /// From the implementation side, this is where the input data comes in (e.g. from the message dispatcher)
        /// </summary>
        /// <param name="source">Source of message</param>
        /// <param name="sentence">Sentence to send</param>
        public abstract void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence);

        /// <summary>
        /// Send the given sentence to the interface.
        /// </summary>
        /// <param name="sentence">Sentence to send</param>
        public void SendSentence(NmeaSentence sentence)
        {
            SendSentence(this, sentence);
        }

        /// <summary>
        /// Stops sending or receiving messages from and to this interface.
        /// Any pending messages are discarded.
        /// </summary>
        public abstract void StopDecode();

        /// <summary>
        /// Fire an event informing about parser errors
        /// </summary>
        /// <param name="message">The message to write</param>
        /// <param name="error">The kind of error seen</param>
        protected void FireOnParserError(string message, NmeaError error)
        {
            OnParserError?.Invoke(this, message, error);
        }

        /// <summary>
        /// Dispose this instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopDecode();
            }
        }

        /// <summary>
        /// Standard dispose method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Forwards the given sentence to listeners, as needed. Use the current instance as source.
        /// </summary>
        /// <param name="typedSequence">The sentences to process</param>
        protected virtual void DispatchSentenceEvents(NmeaSentence? typedSequence)
        {
            DispatchSentenceEvents(this, typedSequence);
        }

        /// <summary>
        /// Forwards the given sentence to listeners, as needed.
        /// </summary>
        /// <param name="source">The source from which this event comes (if forwarded)</param>
        /// <param name="typedSequence">The sentences to process</param>
        protected virtual void DispatchSentenceEvents(NmeaSinkAndSource source, NmeaSentence? typedSequence)
        {
            if (typedSequence != null)
            {
                OnNewSequence?.Invoke(source, typedSequence);
            }

            if (typedSequence is RecommendedMinimumNavigationInformation rmc && rmc.Valid)
            {
                OnNewPosition?.Invoke(rmc.Position, rmc.TrackMadeGoodInDegreesTrue, rmc.SpeedOverGround);
            }
            else if (typedSequence is TimeDate td)
            {
                if (td.Valid)
                {
                    OnNewTime?.Invoke(td.DateTime);
                }
            }
        }

        /// <summary>
        /// Sends a list of messages at once
        /// </summary>
        /// <param name="sentencesToSend">The list of sentences to send</param>
        public virtual void SendSentences(IEnumerable<NmeaSentence> sentencesToSend)
        {
            foreach (var sentence in sentencesToSend)
            {
                SendSentence(sentence);
            }
        }
    }
}
