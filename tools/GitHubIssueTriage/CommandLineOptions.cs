// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CommandLine;

namespace GitHubIssueTriage
{
    /// <summary>
    /// Command line options for the GitHub Issue Triage tool.
    /// </summary>
    public class CommandLineOptions
    {
        [Option('t', "token", Required = true, HelpText = "GitHub token for API access.")]
        public string GitHubToken { get; set; } = string.Empty;

        [Option('o', "owner", Required = false, Default = "dotnet", HelpText = "Repository owner (default: dotnet).")]
        public string Owner { get; set; } = "dotnet";

        [Option('r', "repo", Required = false, Default = "iot", HelpText = "Repository name (default: iot).")]
        public string Repository { get; set; } = "iot";

        [Option('i', "issue", Required = true, HelpText = "Issue number to triage.")]
        public int IssueNumber { get; set; }

        [Option('a', "apply", Required = false, Default = false, HelpText = "Apply suggested changes instead of dry-run.")]
        public bool Apply { get; set; }

        [Option('v', "verbose", Required = false, Default = false, HelpText = "Enable verbose output.")]
        public bool Verbose { get; set; }

        [Option("openai-key", Required = false, HelpText = "OpenAI API key for LLM-based analysis. If not provided, uses keyword-based analysis.")]
        public string? OpenAIKey { get; set; }

        [Option("openai-model", Required = false, Default = "gpt-4o-mini", HelpText = "OpenAI model to use for analysis (default: gpt-4o-mini).")]
        public string OpenAIModel { get; set; } = "gpt-4o-mini";
    }
}