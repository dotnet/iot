// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHubIssueTriage
{
    /// <summary>
    /// Represents a triage suggestion for a GitHub issue.
    /// </summary>
    public class TriageSuggestion
    {
        public List<string> LabelsToAdd { get; set; } = new List<string>();
        public List<string> LabelsToRemove { get; set; } = new List<string>();
        public bool ShouldClose { get; set; }
        public string TriageComment { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
    }

    /// <summary>
    /// Analyzes GitHub issues and provides triage suggestions.
    /// </summary>
    public class IssueTriageAnalyzer
    {
        private readonly bool _verbose;

        public IssueTriageAnalyzer(bool verbose = false)
        {
            _verbose = verbose;
        }

        /// <summary>
        /// Analyzes an issue and returns triage suggestions.
        /// </summary>
        public TriageSuggestion AnalyzeIssue(Octokit.Issue issue)
        {
            var suggestion = new TriageSuggestion();
            
            if (_verbose)
            {
                Console.WriteLine($"Analyzing issue #{issue.Number}: {issue.Title}");
            }

            // Always remove untriaged label if present
            if (HasLabel(issue, "untriaged"))
            {
                suggestion.LabelsToRemove.Add("untriaged");
            }

            // Analyze based on existing labels and content
            AnalyzeByExistingLabels(issue, suggestion);
            AnalyzeByContent(issue, suggestion);

            return suggestion;
        }

        private void AnalyzeByExistingLabels(Octokit.Issue issue, TriageSuggestion suggestion)
        {
            // If it's already labeled as bug from template
            if (HasLabel(issue, "bug"))
            {
                suggestion.TriageComment = "[Triage] This issue has been identified as a bug report. " +
                    "Please ensure you have provided steps to reproduce, expected vs actual behavior, " +
                    "and version information as requested in the template.";
                suggestion.Reasoning = "Issue template identified this as a bug";
                return;
            }

            // If it's already labeled as api-suggestion from template
            if (HasLabel(issue, "api-suggestion"))
            {
                if (IsDeviceRequest(issue))
                {
                    suggestion.ShouldClose = true;
                    suggestion.TriageComment = "[Triage] Thank you for your device binding suggestion! " +
                        "We don't track device binding requests as issues, but we absolutely welcome " +
                        "and help drive Pull Requests to completion. Please feel free to submit a PR " +
                        "with your device binding implementation. You can find guidance on creating " +
                        "device bindings in our documentation.";
                    suggestion.Reasoning = "Device binding requests should be closed with explanation";
                }
                else
                {
                    suggestion.TriageComment = "[Triage] This issue has been identified as an API suggestion. " +
                        "Please provide a detailed description of the proposed API changes and use cases.";
                    suggestion.Reasoning = "API suggestion needs clarification";
                }
                return;
            }
        }

        private void AnalyzeByContent(Octokit.Issue issue, TriageSuggestion suggestion)
        {
            var title = issue.Title?.ToLowerInvariant() ?? "";
            var body = issue.Body?.ToLowerInvariant() ?? "";
            var content = $"{title} {body}";

            // Check for device-related keywords
            if (IsDeviceRequest(issue))
            {
                if (!HasLabel(issue, "api-suggestion"))
                {
                    suggestion.LabelsToAdd.Add("api-suggestion");
                }
                suggestion.ShouldClose = true;
                suggestion.TriageComment = "[Triage] Thank you for your device binding suggestion! " +
                    "We don't track device binding requests as issues, but we absolutely welcome " +
                    "and help drive Pull Requests to completion. Please feel free to submit a PR " +
                    "with your device binding implementation. You can find guidance on creating " +
                    "device bindings in our documentation.";
                suggestion.Reasoning = "Device binding request detected";
                return;
            }

            // Check for bug indicators
            if (IsBugReport(content))
            {
                if (!HasLabel(issue, "bug"))
                {
                    suggestion.LabelsToAdd.Add("bug");
                }
                suggestion.TriageComment = "[Triage] This appears to be a bug report. " +
                    "Please ensure you have provided steps to reproduce, expected vs actual behavior, " +
                    "and version information. If any of these are missing, please update your issue.";
                suggestion.Reasoning = "Bug indicators detected in content";
                return;
            }

            // Check for question indicators
            if (IsQuestion(content))
            {
                suggestion.LabelsToAdd.Add("question");
                suggestion.TriageComment = "[Triage] This appears to be a question. " +
                    "Please provide as much context as possible about what you're trying to achieve. " +
                    "You might also want to check our documentation and existing issues for similar questions.";
                suggestion.Reasoning = "Question indicators detected";
                return;
            }

            // Default case - needs more information
            suggestion.LabelsToAdd.Add("Needs: Author Feedback");
            suggestion.TriageComment = "[Triage] Thank you for filing this issue. " +
                "To help us better understand and address your concern, could you please provide more details? " +
                "If this is a bug, please include steps to reproduce, expected behavior, and actual behavior. " +
                "If this is a feature request, please describe the use case and expected functionality.";
            suggestion.Reasoning = "Insufficient information to categorize";
        }

        private bool IsDeviceRequest(Octokit.Issue issue)
        {
            var title = issue.Title?.ToLowerInvariant() ?? "";
            var body = issue.Body?.ToLowerInvariant() ?? "";
            var content = $"{title} {body}";

            // Device binding keywords
            var deviceKeywords = new[]
            {
                "device binding", "device support", "new device", "add device", "binding for",
                "support for device", "device driver", "sensor support", "add sensor",
                "new sensor", "binding", "i2c device", "spi device", "gpio device"
            };

            return deviceKeywords.Any(keyword => content.Contains(keyword));
        }

        private bool IsBugReport(string content)
        {
            var bugKeywords = new[]
            {
                "bug", "error", "exception", "not working", "doesn't work", "fails",
                "incorrect", "wrong", "unexpected", "should work", "broken",
                "crash", "throws", "stacktrace", "null reference"
            };

            return bugKeywords.Any(keyword => content.Contains(keyword));
        }

        private bool IsQuestion(string content)
        {
            var questionKeywords = new[]
            {
                "how do i", "how to", "how can", "help", "guidance", "question",
                "what is", "which", "where", "when", "why", "?", "please help",
                "need help", "assistance", "clarification"
            };

            return questionKeywords.Any(keyword => content.Contains(keyword));
        }

        private bool HasLabel(Octokit.Issue issue, string labelName)
        {
            return issue.Labels.Any(label => 
                string.Equals(label.Name, labelName, StringComparison.OrdinalIgnoreCase));
        }
    }
}