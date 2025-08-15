# .NET IoT — Repository Custom Instructions for GitHub Copilot

> Purpose: steer Copilot to propose changes that match this repo’s structure, coding conventions, and contribution norms for **System.Device.Gpio** and **Iot.Device.Bindings**.

---

## Scope & context (what this repo is)
- This repo contains the low-level **System.Device.Gpio** APIs and a large collection of device drivers under **Iot.Device.Bindings**.
- Code is used across different OSes and boards. Prefer **hardware-agnostic** code, with platform-specific behavior isolated behind drivers/abstractions.
- Treat public APIs as **experimental but stable-in-spirit**: prefer additive changes, avoid breaking patterns unless maintainers direct otherwise.

---

## Build, test, and layout (how to work here)
- **Solution layout:** library code lives under `src/` (e.g., `src/System.Device.Gpio`, `src/devices/<DeviceName>`); runnable examples live under `samples/` and often under each device’s folder.
- **Build:** default to `dotnet build` at the root or use the repo scripts (`build.sh`/`Build.cmd`) when appropriate. Respect TFMs and settings defined by `Directory.Build.*` and `.editorconfig`.
- **Tests:** prefer **unit tests** for logic that can be isolated from hardware. For hardware-only functionality, ensure samples (and documentation) make manual verification straightforward.
- **Docs:** device-specific docs/README typically live next to the device code; repo-wide docs live under `Documentation/`.

> Copilot: when proposing commands or CI steps, use `dotnet build` and `dotnet test` as separate commands, or `dotnet build && dotnet test` for shell chaining. Do not bump target frameworks or change solution structure unless explicitly asked.

---

## Coding style & conventions (how to write code)
- Follow the repo’s **.editorconfig** and standard .NET naming rules (PascalCase for public members/types, camelCase for locals/fields; interfaces start with `I`).
- Prefer **clarity over cleverness**: braces for all control flow, avoid long chained expressions; add small helper methods if they improve readability.
- Enable and fix compiler warnings/analyzers; keep `using` directives tidy.
- Nullability: write **null-safe** code and validate inputs (`ArgumentNullException`, `ArgumentOutOfRangeException`, etc.).
- Avoid unnecessary allocations in hot paths; prefer spans/stackalloc/pooled buffers when reading/writing registers or doing tight loops.

---

## Resource management (very important for hardware)
- Any type that owns hardware resources (GPIO pins, I2C/SPI devices, file handles, PWM, etc.) must implement **`IDisposable`** and release resources promptly.
- Dispose/close transports (`I2cDevice`, `SpiDevice`, `GpioController`) you create. If a transport is passed in by the caller, **do not dispose it** unless documented as owned.
- Design for **failure**: ensure exceptions (or `Try*` patterns) leave the device in a consistent state; clean up on error.

---

## Device binding design (patterns Copilot should follow)
When adding or updating a **device binding** under `src/devices/<DeviceName>`:

1. **Constructor & dependencies**
   - Accept dependencies that the caller can configure, e.g. `I2cDevice`, `SpiDevice`, `GpioController`, pin numbers, bus addresses, etc.
   - Provide sensible defaults (e.g., common I²C addresses), but never hardcode board-specific details.
   - Avoid singletons and implicit global state.

2. **API shape**
   - Prefer **`TryRead*`** methods that return `bool` and set `out` parameters for sensor readings (e.g., `TryReadTemperature(out Temperature value)`).
   - For settings and commands, use clear methods (e.g., `Reset()`, `EnableHeater(bool)`) or strongly-typed properties. Use enums/flags for registers, modes, scales.
   - Keep synchronous APIs **predictable**; only introduce `async` if there’s genuine I/O concurrency or long waits.

3. **Timing & reliability**
   - Respect datasheet timing: enforce minimal delays between transactions; document any polling limits.
   - Validate checksums/status bits where appropriate and surface failures clearly.

4. **Error handling**
   - Guard against misuse (invalid pins, uninitialized device) with `ArgumentException`/`InvalidOperationException` and descriptive messages.
   - For transient hardware issues, either throw or return `false` in `Try*` patterns—**never** return bogus data silently.

5. **Cross-platform**
   - Use **System.Device** abstractions (controllers, drivers) instead of P/Invoke or OS-specific syscalls. If a feature is platform-only, guard it and fail gracefully elsewhere.

6. **Samples & docs**
   - Add a **sample** demonstrating typical usage (`samples/` and/or `src/devices/<DeviceName>/samples`). Keep samples small, readable, and disposable-friendly.
   - Add or update a **README** for the device with wiring notes, required pull-ups, expected ranges/units, and example code.

---

## Don’ts (things Copilot should avoid suggesting)
- Don’t propose **board-specific hacks** or hardcoded paths to `/sys`/proc unless the existing pattern does so via an abstraction.
- Don’t capture `Console` I/O in library code (leave printing/logging to samples/apps).
- Don’t add hidden thread‑static caches or background threads without clear ownership and disposal.
- Don’t change package versions, TFMs, or CI configuration unless requested.
- Don’t introduce breaking API changes to public types without a strong justification and maintainer direction.

---

## Review checklist for new/updated bindings (use this before finishing a PR)
- [ ] Public API follows repo naming/style; members are XML-documented with units (°C, Pa, %RH, lux, etc.).
- [ ] Constructor accepts caller‑provided transports and documents ownership.
- [ ] Implements `IDisposable`; all hardware resources released.
- [ ] Uses enums/consts for registers/addresses; no unexplained magic numbers.
- [ ] Validates timing and checksums; `Try*` methods return accurate success/failure.
- [ ] Adds/updates **samples** and **device README**.
- [ ] Adds tests for logic that can be isolated from hardware (e.g., framing/parsing functions).

---

## Good prompt patterns (how to ask Copilot)
Use explicit, repo‑aware prompts. Examples:

- *“Create a new I2C temperature sensor binding under `src/devices/AcmeTemp` with a `TryReadTemperature(out Temperature t)` method and a sample app. Constructor should accept an `I2cDevice` and an optional address (default 0x48). Implement `IDisposable` and document units.”*
- *“Refactor existing SPI display code to avoid per-frame allocations. Suggest a pooled buffer approach using `Span<byte>` and show before/after microbenchmarks.”*
- *“Add XML docs and a README section for BMP280 explaining oversampling settings and expected units. Update the sample to dispose controllers correctly.”*

---

## Where to look in this repo (for Copilot context)
- **Patterns:** browse `src/devices` for existing sensor/display bindings; mirror their constructors, enums, and `Try*` APIs.
- **Abstractions:** use `System.Device.Gpio`, `System.Device.I2c`, `System.Device.Spi`, `System.Device.Pwm` where possible.
- **Docs:** check `Documentation/` for contribution guidelines; keep wording and tone consistent.

---

## Security, licensing, and compliance
- Code is under **MIT**; include appropriate headers where needed and avoid copying vendor code without license compatibility.
- If a binding requires vendor blobs/firmware, keep them **out** of the repo; link to vendor downloads and document setup.

---

## One‑liner summary (what Copilot should optimize for)
> **Write clear, disposable-correct, hardware-agnostic device code that matches existing patterns, surfaces failures honestly, and ships with samples and docs.**

