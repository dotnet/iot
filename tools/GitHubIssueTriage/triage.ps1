# GitHub Issue Triage Script for dotnet/iot
# Usage: .\triage.ps1 <issue-number> [apply]

param(
    [Parameter(Mandatory=$true)]
    [int]$IssueNumber,
    
    [Parameter(Mandatory=$false)]
    [switch]$Apply
)

if (-not $env:GITHUB_TOKEN) {
    Write-Error "❌ Error: GITHUB_TOKEN environment variable is required"
    Write-Host "   Please set your GitHub token: `$env:GITHUB_TOKEN='ghp_your_token_here'"
    Write-Host ""
    Write-Host "Environment variables:"
    Write-Host "  GITHUB_TOKEN    - Required: GitHub API token"
    Write-Host "  OPENAI_API_KEY  - Optional: OpenAI API key for LLM analysis (future feature)"
    exit 1
}

$ApplyFlag = ""
if ($Apply) {
    $ApplyFlag = "--apply"
    Write-Host "⚠️  APPLY MODE: Changes will be made to the GitHub issue!" -ForegroundColor Yellow
} else {
    Write-Host "ℹ️  DRY-RUN MODE: No changes will be made" -ForegroundColor Cyan
}

# Check if OpenAI key is provided (for future LLM features)
$OpenAIFlags = ""
if ($env:OPENAI_API_KEY) {
    $OpenAIFlags = "--openai-key $($env:OPENAI_API_KEY)"
    Write-Host "🤖 LLM analysis enabled (when implemented)" -ForegroundColor Magenta
}

Write-Host "🔍 Analyzing issue #$IssueNumber in dotnet/iot..." -ForegroundColor Green
Write-Host

Set-Location $PSScriptRoot
$Command = "dotnet run -- --token $($env:GITHUB_TOKEN) --issue $IssueNumber $ApplyFlag --verbose $OpenAIFlags"
Invoke-Expression $Command