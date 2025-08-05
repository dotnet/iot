# GitHub Issue Triage Tool

A .NET tool for analyzing and triaging GitHub issues in the dotnet/iot repository.

## Features

- Analyzes GitHub issues and suggests appropriate labels
- Detects bug reports, questions, and device binding requests
- Automatically removes "untriaged" label and adds appropriate labels
- Closes device binding requests with appropriate explanation
- Supports dry-run mode (default) and apply mode
- Provides triage comments explaining the next steps

## Usage

```bash
dotnet run -- --token <github-token> --issue <issue-number> [options]
```

### Options

- `-t, --token` (required): GitHub personal access token
- `-i, --issue` (required): Issue number to triage
- `-o, --owner` (optional): Repository owner (default: dotnet)
- `-r, --repo` (optional): Repository name (default: iot)
- `-a, --apply` (optional): Apply changes instead of dry-run
- `-v, --verbose` (optional): Enable verbose output

### Examples

```bash
# Dry-run analysis of issue #123
dotnet run -- --token ghp_xxx --issue 123

# Apply triage suggestions to issue #456
dotnet run -- --token ghp_xxx --issue 456 --apply

# Verbose analysis of issue #789
dotnet run -- --token ghp_xxx --issue 789 --verbose
```

## Triage Logic

The tool analyzes issues based on:

1. **Existing labels** from issue templates
2. **Content analysis** of title and body text
3. **Keyword detection** for categorization

### Label Suggestions

- **Bug reports**: Adds "bug" label, requests reproduction steps
- **Questions**: Adds "question" label, requests more context
- **Device requests**: Adds "api-suggestion", closes with PR guidance
- **Unclear issues**: Adds "Needs: Author Feedback", requests clarification

### Device Binding Requests

Device binding requests are automatically closed with a comment explaining that:
- We don't track device requests as issues
- We welcome PRs for device bindings
- Contributors should submit PRs instead

## Requirements

- .NET 8.0 SDK
- GitHub personal access token with repository access
- Write permissions to the target repository (for apply mode)