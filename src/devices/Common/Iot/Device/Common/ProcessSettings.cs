// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Common
{
    /// <summary>
    /// The settings used to run the external process driving
    /// the image or video acquisition.
    /// </summary>
    public class ProcessSettings
    {
        /// <summary>
        /// Gets or sets the relative or absolute executable file to run.
        /// </summary>
        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size of the buffer used to copy the stream of incoming data.
        /// </summary>
        public int BufferSize { get; set; } = 81920;

        /// <summary>
        /// Gets or sets the working directory used when the process is started.
        /// If null or empty string, Environment.CurrentDirectory will be used.
        /// </summary>
        public string? WorkingDirectory { get; set; } = null;

        /// <summary>
        /// Gets or sets whether stderr should be captured instead of stdout.
        /// When true, the stderr output is captured instead of the stdout.
        /// This is needed in apps that outputs text such as the app usage.
        /// </summary>
        public bool CaptureStderrInsteadOfStdout { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of milliseconds that are waited before
        /// forcibly closing (kill) the process when the operation is completed.
        /// </summary>
        public int MaxMillisecondsToWaitAfterProcessCompletes { get; set; } = 1000;
    }
}
