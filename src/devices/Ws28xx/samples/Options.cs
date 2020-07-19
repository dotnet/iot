using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace Ws28xx.Samples
{
    /// <summary>
    /// Options for CLI
    /// </summary>
    public class Options
    {
        /// <summary>
        /// ColorWipe Animation
        /// </summary>
        [Option(Default = false, HelpText = "Enables ColorWipe Animation")]
        public bool ColorWipe { get; set; }

        /// <summary>
        /// TheaterChase Animation
        /// </summary>
        [Option(Default = false, HelpText = "Enables TheaterChase animation")]
        public bool TheaterChase { get; set; }

        /// <summary>
        /// Rainbow Animations - Ignores other options
        /// </summary>
        [Option(Default = false, HelpText = "Enables ColorWipe and TheaterChase using Rainbows, ignores --colors setting")]
        public bool Rainbows { get; set; }

        /// <summary>
        /// Colors to use for animations
        /// </summary>
        [Option('c', "colors", Required = false, HelpText = "Colors to use for all animations")]
        public IEnumerable<string> Colors { get; set; }

        /// <summary>
        /// Number of LEDs
        /// </summary>
        [Value(8, MetaName = "ledcount", HelpText = "Number of LEDs")]
        public long? LEDCount { get; set; }
    }
}
