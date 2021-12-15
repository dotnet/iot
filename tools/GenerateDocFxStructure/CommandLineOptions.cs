// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CommandLine;

namespace GenerateDocFxStructure
{
    /// <summary>
    /// Class for command line options.
    /// </summary>
    public class CommandlineOptions
    {
        /// <summary>
        /// Gets or sets the destination folder.
        /// </summary>
        [Option('d', "destination", Required = true, HelpText = "Folder containing the destination folder..")]
        public string DestinationFolder { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source folder.
        /// </summary>
        [Option('s', "source", Required = true, HelpText = "Folder containing the source folder.")]
        public string SourceFolder { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination media folder.
        /// </summary>
        [Option('m', "media", Required = true, HelpText = "Folder containing the destination media folder.")]
        public string MediaFolder { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the main repository.
        /// </summary>
        [Option('r', "repo", Required = false, HelpText = "The main repository with the branch to target poiting on the folder used as a source. Default value is 'https://github.com/dotnet/iot/tree/main/src/devices'.")]
        public string Repo { get; set; } = "https://github.com/dotnet/iot/tree/main/src/devices";

        /// <summary>
        /// Gets or sets a value indicating whether verbose information is shown in the output.
        /// </summary>
        [Option('v', "verbose", Required = false, HelpText = "Show verbose messages.")]
        public bool Verbose { get; set; }
    }
}
