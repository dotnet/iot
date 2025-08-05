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
    exit 1
}

$ApplyFlag = ""
if ($Apply) {
    $ApplyFlag = "--apply"
    Write-Host "⚠️  APPLY MODE: Changes will be made to the GitHub issue!" -ForegroundColor Yellow
} else {
    Write-Host "ℹ️  DRY-RUN MODE: No changes will be made" -ForegroundColor Cyan
}

Write-Host "🔍 Analyzing issue #$IssueNumber in dotnet/iot..." -ForegroundColor Green
Write-Host

Set-Location $PSScriptRoot
dotnet run -- --token $env:GITHUB_TOKEN --issue $IssueNumber $ApplyFlag --verbose