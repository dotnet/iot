# Creating new .NET IoT release

This is a list of steps which need to happen in order to release new version of .NET IoT.

> **Note**: This document is meant only for maintainers as special permissions might be required

## Steps

- Step 1: Ensure all PRs meant for this release are merged.
  - While the process is ongoing, make sure no new PRs get merged in order to ensure that the official build doesn't get reset.
- Step 2: Regenerate device listing:
  - `dotnet run` on <https://github.com/dotnet/iot/tree/main/tools/device-listing>
  - Fix all warnings, re-run if needed to ensure no warnings.
  - Always manually review the changes paying attention the generated document looks clear.
  - Adjust categories/code of the generator if necessary.
- Step 3: Create new package:
  - Go to <https://dev.azure.com/dotnet/IoT/_build?definitionId=179>
  - Select "Run Pipeline", select row which says `variables` and add new one: `DotNetFinalVersionKind=release` (no quotes anywhere).
  - Run and wait for it to finish.
  - Once it is done and passes go to the artifacts section of the build, and find an artifact called 'Built packages' and from there download the two stable packages.
  - Validate the package: double check version, check there are no unintentional changes.
- Step 4: Manually push package to Nuget - people who had access to push in the past: @joperezr @rbhanda
- Step 5: Add git tag:
  - Use `git tag -a <version> <commit_hash>` to locally create a tag.
  - Use `git push origin <version>` to push it (`<version>` is i.e. `1.3`).
- Step 6: Edit release notes on github:
  - Follow example: <https://github.com/dotnet/iot/releases/tag/1.2>
  - Copy the list of commits, clean those related to dependencies update.
    - Use i.e.: <https://github.com/dotnet/iot/compare/1.1...1.2> to see list of commits.
  - Categorize them by: `System.Gpio`, `Iot.Device.Bindings`, `Other` changes.
  - Rephrase/group them for consistency.
  - Add the list of contributors ordered by the number of commits or alphabetically. Command: `git shortlog -s 1.4..1.5` is very helpful but doesn't give github user names
- Step 7: After package is pushed to Nuget create a PR similar to <https://github.com/dotnet/iot/pull/1310> to prepare for next release.
- Step 8: Update dependencies on old version of the package:
  - Option 1: Wait for `dependabot` to automatically create dependencies update PR (similar to: [System.Device.Gpio](https://github.com/dotnet/iot/pulls?q=is%3Apr+Bump+System.Device.Gpio+is%3Aclosed+author%3Aapp%2Fdependabot); [Iot.Device.Bindings](https://github.com/dotnet/iot/pulls?q=is%3Apr+Bump+Iot.Device.Bindings+is%3Aclosed+author%3Aapp%2Fdependabot))
  - Options 2: Alternatively update versions manually to avoid swarm of PRs.
    - See list of PRs created by bot in the last release, search for old version across the repo and create a single PR doing all these changes combined.
- Step 9: Update the documentation:
  - Link to repository: <https://github.com/dotnet/iot-api-docs>
  - Currently this is not done by us, you will need to respond to the e-mail thread with request to update the documentation.
    - E-mail thread title for reference: `System.Device.Gpio and Iot.Device.Bindings in the .NET API browser`.
    - Contributors on the thread: @krwq @joperezr @richlander

## TODO

- Automate as much of this process as possible
