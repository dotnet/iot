// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Base class for active NMEA components
    /// This is for parts that handle messages in some way, such as conversion from one format to another,
    /// controlling external hardware or check states. Components usually don't access NMEA interfaces
    /// directly.
    /// </summary>
    public abstract class NmeaComponent : NmeaSinkAndSource
    {
        /// <summary>
        /// Protected standard constructor
        /// </summary>
        protected NmeaComponent(string interfaceName)
            : base(interfaceName)
        {
        }

        /// <summary>
        /// Send a message to this component.
        /// To avoid confusion about the message direction, this method is sealed and clients
        /// should override <see cref="ProcessInputSentence"/> instead.
        /// </summary>
        /// <param name="source">The message's source</param>
        /// <param name="sentence">The sentence</param>
        public sealed override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            ProcessInputSentence(source, sentence);
        }

        /// <summary>
        /// This method is called by the infrastructure if this component gets a message
        /// </summary>
        /// <param name="source">The message's source</param>
        /// <param name="sentence">The sentence</param>
        protected abstract void ProcessInputSentence(NmeaSinkAndSource source, NmeaSentence sentence);

        /// <summary>
        /// Returns the list of recommended input rules for this component
        /// </summary>
        /// <returns>A list of filter rules</returns>
        public List<FilterRule> RecommendedInputFilters()
        {
            return new List<FilterRule>();
        }
    }
}
