# Creating new .NET IoT package

This is a list of steps which need to happen in order to create new package version of .NET IoT.

## Steps

- regenerate device listing (`dotnet run` on https://github.com/dotnet/iot/tree/master/tools/device-listing and fix all warnings)
  - this always requires manual review
- ensure all PRs meant for this release are merged
- ideally until done do not merge any new PRs (technically not required but will avoid confusion and accidental errors)
- [Internal] create new package:
  - Go to https://dev.azure.com/dnceng/internal/_build?definitionId=239
  - Select "Run Pipeline", select row which says variables and add new one: DotNetFinalVersionKind=release (no quotes anywhere)
  - Run and wait for it to finish
  - Once it is done and passes go to the artifacts section of the build, and find an artifact called 'Built packages' and from there download the two stable packages
  - Validate the package (double check version, check there are no unintentional changes)
- push package to nuget
- add git tag (`git tag -a <version> <commit_hash>`) and push it (`git push origin <version>`); `<version>` is i.e. `1.3`
- edit release notes on github (i.e. https://github.com/dotnet/iot/releases/tag/1.2), for list of commits between release use i.e.: https://github.com/dotnet/iot/compare/1.1...1.2
- after shipping create PR similar to https://github.com/dotnet/iot/pull/1310 to prepare for next release
- wait for dependabot to create dependencies update PR (similar to: [System.Device.Gpio](https://github.com/dotnet/iot/pulls?q=is%3Apr+Bump+System.Device.Gpio+is%3Aclosed+author%3Aapp%2Fdependabot); [Iot.Device.Bindings](https://github.com/dotnet/iot/pulls?q=is%3Apr+Bump+Iot.Device.Bindings+is%3Aclosed+author%3Aapp%2Fdependabot))
  - alternatively update it manually to avoid swarm of PRs
- update the documentation
  - Link: https://github.com/dotnet/iot-api-docs
  - [Internal] See e-mail thread

## TODO

- Automate as much of this process as possible
