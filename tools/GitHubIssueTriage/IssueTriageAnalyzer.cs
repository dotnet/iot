// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

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
    /// Represents the structured response from LLM analysis.
    /// </summary>
    public class LLMTriageResponse
    {
        public string Category { get; set; } = string.Empty;
        public List<string> LabelsToAdd { get; set; } = new List<string>();
        public List<string> LabelsToRemove { get; set; } = new List<string>();
        public bool ShouldClose { get; set; }
        public string TriageComment { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public int Confidence { get; set; }
    }

    /// <summary>
    /// Analyzes GitHub issues and provides triage suggestions using LLM or keyword-based analysis.
    /// </summary>
    public class IssueTriageAnalyzer
    {
        private readonly bool _verbose;
        private readonly IChatClient? _chatClient;

        public IssueTriageAnalyzer(bool verbose = false, IChatClient? chatClient = null)
        {
            _verbose = verbose;
            _chatClient = chatClient;
        }

        /// <summary>
        /// Analyzes an issue and returns triage suggestions.
        /// </summary>
        public async Task<TriageSuggestion> AnalyzeIssueAsync(Octokit.Issue issue)
        {
            if (_verbose)
            {
                Console.WriteLine($"Analyzing issue #{issue.Number}: {issue.Title}");
                Console.WriteLine($"Using {(_chatClient != null ? "LLM-based" : "keyword-based")} analysis");
            }

            TriageSuggestion suggestion;

            if (_chatClient != null)
            {
                suggestion = await AnalyzeWithLLMAsync(issue);
            }
            else
            {
                suggestion = AnalyzeWithKeywords(issue);
            }

            // Always remove untriaged label if present
            if (HasLabel(issue, "untriaged") && !suggestion.LabelsToRemove.Contains("untriaged"))
            {
                suggestion.LabelsToRemove.Add("untriaged");
            }

            return suggestion;
        }

        private Task<TriageSuggestion> AnalyzeWithLLMAsync(Octokit.Issue issue)
        {
            var prompt = CreateAnalysisPrompt(issue);
            
            try
            {
                if (_verbose)
                {
                    Console.WriteLine("Sending issue to LLM for analysis...");
                }

                // TODO: Implement actual LLM call once Microsoft.Extensions.AI APIs are finalized
                // For now, fall back to keyword analysis
                if (_verbose)
                {
                    Console.WriteLine("LLM integration placeholder - falling back to keyword analysis");
                }
                
                return Task.FromResult(AnalyzeWithKeywords(issue));
            }
            catch (Exception ex)
            {
                if (_verbose)
                {
                    Console.WriteLine($"LLM analysis failed: {ex.Message}");
                    Console.WriteLine("Falling back to keyword-based analysis...");
                }
                
                // Fallback to keyword-based analysis
                return Task.FromResult(AnalyzeWithKeywords(issue));
            }
        }

        private string CreateAnalysisPrompt(Octokit.Issue issue)
        {
            var existingLabels = string.Join(", ", issue.Labels.Select(l => l.Name));
            
            return $@"You are a GitHub issue triage assistant for the dotnet/iot repository. Analyze the following issue and provide triage suggestions.

ISSUE DETAILS:
Title: {issue.Title}
Body: {issue.Body}
Current Labels: {existingLabels}
Author: {issue.User.Login}

CONTEXT:
- This is a .NET IoT repository that contains device bindings and GPIO/hardware APIs
- Common issue types: bugs, questions, device binding requests, feature requests
- Device binding requests should typically be closed with guidance to submit PRs instead
- Important labels: 'bug', 'question', 'api-suggestion', 'area-System.Device.Gpio', 'untriaged'

ANALYSIS REQUIREMENTS:
1. Categorize the issue as: bug, question, device-request, feature-request, or unclear
2. Determine if it's GPIO/hardware related (add 'area-System.Device.Gpio' label)
3. For device binding requests: suggest closing with explanation
4. Always remove 'untriaged' label when categorizing
5. Provide a helpful triage comment

Respond with valid JSON in this exact format:
{{
  ""category"": ""bug|question|device-request|feature-request|unclear"",
  ""labelsToAdd"": [""label1"", ""label2""],
  ""labelsToRemove"": [""untriaged""],
  ""shouldClose"": false,
  ""triageComment"": ""[Triage] Your helpful comment here..."",
  ""reasoning"": ""Brief explanation of your decision"",
  ""confidence"": 85
}}

Be concise but helpful in your triage comment. Focus on actionable guidance for the issue author.";
        }

        private LLMTriageResponse ParseLLMResponse(string content)
        {
            try
            {
                // Try to extract JSON from the response
                var jsonStart = content.IndexOf('{');
                var jsonEnd = content.LastIndexOf('}');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    return JsonSerializer.Deserialize<LLMTriageResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new LLMTriageResponse();
                }
            }
            catch (JsonException ex)
            {
                if (_verbose)
                {
                    Console.WriteLine($"Failed to parse LLM JSON response: {ex.Message}");
                }
            }

            // If JSON parsing fails, create a basic response
            return new LLMTriageResponse
            {
                Category = "unclear",
                LabelsToAdd = new List<string> { "Needs: Author Feedback" },
                LabelsToRemove = new List<string> { "untriaged" },
                TriageComment = "[Triage] Thank you for filing this issue. Could you please provide more details to help us better understand your concern?",
                Reasoning = "Could not parse LLM response",
                Confidence = 50
            };
        }

        private TriageSuggestion ConvertToTriageSuggestion(LLMTriageResponse llmResponse)
        {
            return new TriageSuggestion
            {
                LabelsToAdd = llmResponse.LabelsToAdd,
                LabelsToRemove = llmResponse.LabelsToRemove,
                ShouldClose = llmResponse.ShouldClose,
                TriageComment = llmResponse.TriageComment,
                Reasoning = $"{llmResponse.Reasoning} (Confidence: {llmResponse.Confidence}%)"
            };
        }

        private TriageSuggestion AnalyzeWithKeywords(Octokit.Issue issue)
        {
            var suggestion = new TriageSuggestion();
            
            if (_verbose)
            {
                Console.WriteLine("Using keyword-based analysis...");
            }

            // Analyze based on existing labels and content
            AnalyzeByExistingLabels(issue, suggestion);
            if (string.IsNullOrEmpty(suggestion.Reasoning))
            {
                AnalyzeByContent(issue, suggestion);
            }

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

            // Check for GPIO-specific issues
            if (IsGpioRelated(content))
            {
                suggestion.LabelsToAdd.Add("area-System.Device.Gpio");
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

            // Check for feature request indicators (not device-related)
            if (IsFeatureRequest(content))
            {
                suggestion.LabelsToAdd.Add("api-suggestion");
                suggestion.TriageComment = "[Triage] This appears to be a feature request. " +
                    "Please provide a detailed description of the proposed changes, use cases, " +
                    "and any relevant API design considerations.";
                suggestion.Reasoning = "Feature request detected";
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

        private bool IsGpioRelated(string content)
        {
            var gpioKeywords = new[]
            {
                "gpio", "pin", "digital", "analog", "pwm", "spi", "i2c", "uart", "serial",
                "system.device.gpio", "raspberry pi", "arduino"
            };

            return gpioKeywords.Any(keyword => content.Contains(keyword));
        }

        private bool IsFeatureRequest(string content)
        {
            var featureKeywords = new[]
            {
                "feature request", "enhancement", "add support", "new feature",
                "would be nice", "suggestion", "improve", "add method", "add property",
                "api request", "could you add"
            };

            return featureKeywords.Any(keyword => content.Contains(keyword));
        }
        
        private bool HasLabel(Octokit.Issue issue, string labelName)
        {
            return issue.Labels.Any(label => 
                string.Equals(label.Name, labelName, StringComparison.OrdinalIgnoreCase));
        }
    }
}