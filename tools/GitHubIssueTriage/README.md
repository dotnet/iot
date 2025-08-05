# GitHub Issue Triage Tool

A .NET tool for analyzing and triaging GitHub issues in the dotnet/iot repository. This tool helps maintainers quickly categorize incoming issues, suggest appropriate labels, and provide consistent triage guidance.

## Features

- 🤖 **LLM-Powered Analysis**: Uses Microsoft.Extensions.AI for intelligent issue categorization (when configured)
- 🔍 **Smart Fallback**: Falls back to keyword-based analysis when LLM is unavailable  
- 🏷️ **Label Management**: Automatically removes "untriaged" and suggests appropriate categorization labels
- 📝 **Triage Comments**: Generates helpful comments explaining next steps to issue authors
- 🔄 **Dry-Run Mode**: Default safe mode shows suggestions without making changes
- ⚡ **Apply Mode**: Option to automatically apply suggested changes
- 🎯 **Device Handling**: Special handling for device binding requests (closes with PR guidance)
- 📊 **Area Detection**: Automatically adds area labels (e.g., area-System.Device.Gpio)

## Quick Start

### Using the Script (Recommended)

```bash
# Set your GitHub token
export GITHUB_TOKEN="ghp_your_token_here"

# Dry-run analysis of issue #123
./triage.sh 123

# Apply triage suggestions to issue #456  
./triage.sh 456 apply
```

### Using with LLM Analysis (Future)

When the LLM integration is complete, you'll be able to use OpenAI for more intelligent analysis:

```bash
# With OpenAI API key for LLM analysis
export OPENAI_API_KEY="your_openai_key"
./triage.sh 123 --openai-key $OPENAI_API_KEY

# Using different model
./triage.sh 123 --openai-key $OPENAI_API_KEY --openai-model gpt-4
```

### Using dotnet run directly

```bash
# Dry-run analysis with keyword-based approach
dotnet run -- --token ghp_xxx --issue 123 --verbose

# Apply changes
dotnet run -- --token ghp_xxx --issue 456 --apply

# Future: LLM-based analysis (when implemented)
dotnet run -- --token ghp_xxx --issue 123 --openai-key sk-xxx --verbose
```

## Triage Logic

### Issue Categories

| Category | Detection | Labels Added | Action |
|----------|-----------|--------------|---------|
| **Bug Report** | Keywords: "error", "exception", "not working", "fails" | `bug` | Request reproduction steps |
| **Question** | Keywords: "how to", "help", "?", "guidance" | `question` | Request more context |
| **Device Request** | Keywords: "device binding", "new device", "sensor support" | `api-suggestion` | Close with PR guidance |
| **Feature Request** | Keywords: "feature request", "enhancement", "add support" | `api-suggestion` | Request detailed description |
| **GPIO Related** | Keywords: "gpio", "pin", "pwm", "spi", "i2c" | `area-System.Device.Gpio` | Area categorization |
| **Unclear** | No clear category found | `Needs: Author Feedback` | Request clarification |

### Device Binding Request Handling

Device binding requests are automatically closed with this guidance:
> [Triage] Thank you for your device binding suggestion! We don't track device binding requests as issues, but we absolutely welcome and help drive Pull Requests to completion. Please feel free to submit a PR with your device binding implementation.

### Label Management

- ✅ Always removes `untriaged` label when categorizing
- ➕ Adds appropriate category labels based on content analysis  
- 🏷️ Respects existing labels from issue templates
- 📍 Adds area labels when specific technologies are detected

## Configuration

### Environment Variables

- `GITHUB_TOKEN`: Required GitHub personal access token with repository access

### Command Line Options

| Option | Description | Default |
|--------|-------------|---------|
| `-t, --token` | GitHub token for API access (required) | - |
| `-i, --issue` | Issue number to triage (required) | - |
| `-o, --owner` | Repository owner | `dotnet` |
| `-r, --repo` | Repository name | `iot` |
| `-a, --apply` | Apply changes instead of dry-run | `false` |
| `-v, --verbose` | Enable verbose output | `false` |

## Examples

### Example 1: Bug Report Analysis
```bash
$ ./triage.sh 1234

=== TRIAGE SUGGESTIONS ===
Reasoning: Bug indicators detected in content
Labels to remove: untriaged
Labels to add: bug, area-System.Device.Gpio
Comment: [Triage] This appears to be a bug report. Please ensure you have provided steps to reproduce, expected vs actual behavior, and version information.
```

### Example 2: Device Request (Auto-Close)
```bash
$ ./triage.sh 5678

=== TRIAGE SUGGESTIONS ===
Reasoning: Device binding request detected
Labels to remove: untriaged
Labels to add: api-suggestion
Action: Close issue
Comment: [Triage] Thank you for your device binding suggestion! We don't track device binding requests as issues...
```

## LLM Integration Status

### Current Implementation
The tool now includes framework support for LLM-based analysis using Microsoft.Extensions.AI:

- ✅ **Architecture**: Core LLM integration structure in place
- ✅ **Command-line options**: `--openai-key` and `--openai-model` parameters added
- ✅ **Fallback mechanism**: Gracefully falls back to keyword analysis
- ⏳ **API Integration**: LLM calls currently use placeholder (Microsoft.Extensions.AI API finalization pending)

### Future Enhancements
When the LLM integration is complete, it will provide:

- 🎯 **Context-aware analysis**: Understanding issue intent beyond keyword matching
- 📝 **Better categorization**: More nuanced classification of complex issues  
- 💬 **Improved comments**: More helpful and personalized triage responses
- 🔍 **Advanced detection**: Better identification of device requests vs. genuine feature requests

## Requirements

- .NET 8.0 SDK
- GitHub personal access token with repository access
- Write permissions to target repository (for apply mode)
- OpenAI API key (optional, for future LLM features)

## Building

```bash
dotnet build
```

## Safety Features

- **Dry-run by default**: Never applies changes unless explicitly requested
- **Verbose logging**: Shows exactly what changes will be made
- **Error handling**: Graceful handling of API errors and edge cases
- **Validation**: Confirms issue exists before attempting modifications

## Integration

This tool is designed to complement the existing GitHub automation in `.github/policies/` and can be:
- Run manually by maintainers
- Integrated into CI/CD workflows
- Used for bulk triage operations
- Extended with additional triage rules