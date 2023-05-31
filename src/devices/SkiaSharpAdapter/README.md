# SkiaSharp graphics library adapter

## Summary

This folder contains the SkiaSharp adapter for Iot.Device.Bindings. It provides an implementation for BitmapImage that uses the
[SkiaSharp](https://github.com/mono/SkiaSharp) multi-platform graphics library. This folder is compiled into its own .nuget file, so that no external dependencies
are included when they're not needed.

See [Ili934x](../Ili934x/Readme.md) for an usage example.

To use SkiaSharp as image library with Iot.Device.Bindings, reference (in addition to `Iot.Device.Bindings.nuget`) the `Iot.Device.Bindings.SkiaSharpAdapter.nuget`
and put a line with `SkiaSharpAdapter.Register()` somewhere at the beginning of your application.
