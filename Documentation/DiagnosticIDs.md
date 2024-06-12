# Diagnostic IDs

Some of the APIs in this repository are still in development and may be changed in the future, so they were marked with the Experimental attribute. If you want to consume these APIs and understand the risks, you can disable the diagnostic IDs directly in your project either by adding a diagnostic id to the `NoWarn` property in your project file or by using the `#pragma warning disable` directive in your code.

## Diagnostic list

| Diagnostic ID     | Description |
| :---------------- | :---------- |
| `SDGPIO0001` | Experimental APIs related to LibGpiod v2 driver used by newer versions of the library |
