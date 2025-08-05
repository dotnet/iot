// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.AI;
using Octokit;

namespace GitHubIssueTriage
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            Console.WriteLine("GitHub Issue Triage Tool for dotnet/iot");
            
            try
            {
                return await Parser.Default.ParseArguments<CommandLineOptions>(args)
                    .MapResult(
                        async options => await RunTriageAsync(options),
                        errors => Task.FromResult(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: An unexpected error occurred: {ex.Message}");
                if (args.Contains("--verbose") || args.Contains("-v"))
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                return 1;
            }
        }

        private static async Task<int> RunTriageAsync(CommandLineOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.GitHubToken))
            {
                Console.WriteLine("ERROR: GitHub token is required.");
                return 1;
            }

            try
            {
                var client = new GitHubClient(new ProductHeaderValue("dotnet-iot-triage-tool"))
                {
                    Credentials = new Credentials(options.GitHubToken)
                };

                // Initialize AI client if OpenAI key is provided
                IChatClient? chatClient = null;
                if (!string.IsNullOrWhiteSpace(options.OpenAIKey))
                {
                    try
                    {
                        chatClient = CreateOpenAIChatClient(options.OpenAIKey, options.OpenAIModel);
                        if (options.Verbose)
                        {
                            Console.WriteLine($"LLM analysis enabled using model: {options.OpenAIModel}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"WARNING: Failed to initialize OpenAI client: {ex.Message}");
                        Console.WriteLine("Falling back to keyword-based analysis...");
                    }
                }
                else if (options.Verbose)
                {
                    Console.WriteLine("No OpenAI key provided, using keyword-based analysis");
                }

                if (options.Verbose)
                {
                    Console.WriteLine($"Connecting to GitHub API...");
                    Console.WriteLine($"Repository: {options.Owner}/{options.Repository}");
                    Console.WriteLine($"Issue: #{options.IssueNumber}");
                    Console.WriteLine($"Mode: {(options.Apply ? "APPLY" : "DRY-RUN")}");
                }

                // Fetch the issue
                var issue = await client.Issue.Get(options.Owner, options.Repository, options.IssueNumber);
                
                if (options.Verbose)
                {
                    Console.WriteLine($"Fetched issue: '{issue.Title}'");
                    Console.WriteLine($"State: {issue.State}");
                    Console.WriteLine($"Current labels: {string.Join(", ", issue.Labels.Select(l => l.Name))}");
                }

                // Analyze the issue
                var analyzer = new IssueTriageAnalyzer(options.Verbose, chatClient);
                var suggestion = await analyzer.AnalyzeIssueAsync(issue);

                // Display suggestions
                Console.WriteLine("\n=== TRIAGE SUGGESTIONS ===");
                Console.WriteLine($"Reasoning: {suggestion.Reasoning}");
                
                if (suggestion.LabelsToRemove.Any())
                {
                    Console.WriteLine($"Labels to remove: {string.Join(", ", suggestion.LabelsToRemove)}");
                }
                
                if (suggestion.LabelsToAdd.Any())
                {
                    Console.WriteLine($"Labels to add: {string.Join(", ", suggestion.LabelsToAdd)}");
                }
                
                if (suggestion.ShouldClose)
                {
                    Console.WriteLine("Action: Close issue");
                }
                
                Console.WriteLine($"Comment: {suggestion.TriageComment}");

                // Apply changes if requested
                if (options.Apply)
                {
                    Console.WriteLine("\n=== APPLYING CHANGES ===");
                    await ApplyTriageSuggestionsAsync(client, options, issue, suggestion);
                    Console.WriteLine("Changes applied successfully!");
                }
                else
                {
                    Console.WriteLine("\n=== DRY-RUN MODE ===");
                    Console.WriteLine("Use --apply flag to actually make these changes.");
                }

                return 0;
            }
            catch (NotFoundException)
            {
                Console.WriteLine($"ERROR: Issue #{options.IssueNumber} not found in {options.Owner}/{options.Repository}");
                return 1;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("ERROR: Unauthorized access. Please check your GitHub token permissions.");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                if (options.Verbose)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                return 1;
            }
        }

        private static IChatClient? CreateOpenAIChatClient(string apiKey, string model)
        {
            // TODO: Implement OpenAI client creation once Microsoft.Extensions.AI APIs are stabilized
            // For now, return null to fall back to keyword-based analysis
            Console.WriteLine("LLM integration is not fully implemented yet. Using keyword-based analysis.");
            return null;
        }

        private static async Task ApplyTriageSuggestionsAsync(
            GitHubClient client, 
            CommandLineOptions options, 
            Issue issue, 
            TriageSuggestion suggestion)
        {
            // Add comment
            if (!string.IsNullOrEmpty(suggestion.TriageComment))
            {
                Console.WriteLine("Adding triage comment...");
                await client.Issue.Comment.Create(options.Owner, options.Repository, issue.Number, suggestion.TriageComment);
            }

            // Update labels
            var currentLabels = issue.Labels.Select(l => l.Name).ToList();
            var newLabels = new List<string>(currentLabels);

            // Remove labels
            foreach (var labelToRemove in suggestion.LabelsToRemove)
            {
                newLabels.RemoveAll(l => string.Equals(l, labelToRemove, StringComparison.OrdinalIgnoreCase));
                Console.WriteLine($"Removing label: {labelToRemove}");
            }

            // Add labels
            foreach (var labelToAdd in suggestion.LabelsToAdd)
            {
                if (!newLabels.Any(l => string.Equals(l, labelToAdd, StringComparison.OrdinalIgnoreCase)))
                {
                    newLabels.Add(labelToAdd);
                    Console.WriteLine($"Adding label: {labelToAdd}");
                }
            }

            // Update labels if there are changes
            if (!currentLabels.SequenceEqual(newLabels))
            {
                var issueUpdate = issue.ToUpdate();
                issueUpdate.ClearLabels();
                foreach (var label in newLabels)
                {
                    issueUpdate.AddLabel(label);
                }
                await client.Issue.Update(options.Owner, options.Repository, issue.Number, issueUpdate);
            }

            // Close issue if suggested
            if (suggestion.ShouldClose)
            {
                Console.WriteLine("Closing issue...");
                var issueUpdate = issue.ToUpdate();
                issueUpdate.State = ItemState.Closed;
                await client.Issue.Update(options.Owner, options.Repository, issue.Number, issueUpdate);
            }
        }
    }
}