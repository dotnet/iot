#!/bin/bash

# GitHub Issue Triage Script for dotnet/iot
# Usage: ./triage.sh <issue-number> [apply]

set -e

if [ $# -lt 1 ]; then
    echo "Usage: $0 <issue-number> [apply]"
    echo "       $0 123        # Dry-run triage for issue #123"  
    echo "       $0 123 apply  # Apply triage suggestions to issue #123"
    exit 1
fi

ISSUE_NUMBER=$1
APPLY_FLAG=""

if [ "$2" = "apply" ]; then
    APPLY_FLAG="--apply"
    echo "⚠️  APPLY MODE: Changes will be made to the GitHub issue!"
else
    echo "ℹ️  DRY-RUN MODE: No changes will be made"
fi

if [ -z "$GITHUB_TOKEN" ]; then
    echo "❌ Error: GITHUB_TOKEN environment variable is required"
    echo "   Please set your GitHub token: export GITHUB_TOKEN='ghp_your_token_here'"
    exit 1
fi

echo "🔍 Analyzing issue #$ISSUE_NUMBER in dotnet/iot..."
echo

cd "$(dirname "$0")"
dotnet run -- --token "$GITHUB_TOKEN" --issue "$ISSUE_NUMBER" $APPLY_FLAG --verbose