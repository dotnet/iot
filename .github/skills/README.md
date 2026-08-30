# GitHub Copilot AI Skills for .NET IoT Repository

This directory contains AI skills that help GitHub Copilot agents perform specialized tasks in the .NET IoT repository. Skills are automatically loaded by Copilot based on context and task requirements.

## Available Skills

### 1. [add-device-binding](./add-device-binding/SKILL.md)
**Purpose:** Guide for adding new IoT device bindings following repository conventions.

**When to use:**
- Adding a new sensor, display, motor, or other hardware device binding
- Creating driver code under `src/devices/<DeviceName>`
- Ensuring device follows .NET IoT patterns

**Key features:**
- Device structure setup
- API design patterns (TryRead* methods, UnitsNet types)
- Resource management (IDisposable)
- Documentation requirements
- Sample application guidelines

### 2. [debug-build-issues](./debug-build-issues/SKILL.md)
**Purpose:** Diagnose and resolve build, compilation, and SDK issues.

**When to use:**
- Build fails with SDK resolution errors
- Package restore failures
- Long-running build timeouts
- CI/CD pipeline failures
- Memory issues during build

**Key features:**
- Azure DevOps feeds troubleshooting
- Common build issues and solutions
- Alternative build approaches
- Expected timing reference
- Log location guidance

### 3. [fix-api-conventions](./fix-api-conventions/SKILL.md)
**Purpose:** Ensure device bindings follow repository API conventions.

**When to use:**
- Reviewing device binding code
- Fixing API design issues
- Code review identifies convention violations
- Modernizing older device bindings

**Key features:**
- API naming conventions
- UnitsNet usage patterns
- Constructor design guidelines
- Resource management patterns
- Error handling best practices

### 4. [update-device-documentation](./update-device-documentation/SKILL.md)
**Purpose:** Create and maintain comprehensive device documentation.

**When to use:**
- Adding documentation for new device
- Updating existing device docs
- Adding wiring diagrams
- Improving code examples
- Adding XML documentation

**Key features:**
- README structure and content
- Wiring information templates
- XML documentation patterns
- Code example guidelines
- Markdown linting compliance

### 5. [hardware-abstraction-check](./hardware-abstraction-check/SKILL.md)
**Purpose:** Verify proper hardware abstraction and cross-platform compatibility.

**When to use:**
- Reviewing new device binding code
- Code contains platform-specific APIs
- Ensuring cross-platform compatibility
- Validating no OS dependencies introduced

**Key features:**
- Anti-pattern detection (P/Invoke, hardcoded paths)
- System.Device.* abstraction usage
- Automated compliance checks
- Cross-platform testing guidance
- Platform-specific code guidelines

## How Skills Work

GitHub Copilot agents automatically discover and use these skills when:
1. Task matches skill description
2. Agent needs domain-specific guidance
3. Context indicates skill would be helpful

Skills are defined using the standard GitHub Copilot Agent Skills format:
- Each skill has its own directory
- `SKILL.md` file contains metadata (YAML frontmatter) and instructions
- Skills can include supporting files (scripts, examples, templates)

## Skill Format

Each `SKILL.md` follows this structure:

```markdown
---
name: skill-name
description: Brief description of what the skill does
license: MIT
---

## Purpose
What the skill helps with

## When to Use This Skill
Specific scenarios for using the skill

## Instructions
Detailed step-by-step guidance

## Examples
Code examples and demonstrations

## References
Links to related documentation
```

## Creating New Skills

To add a new skill:

1. Create directory: `.github/skills/<skill-name>/`
2. Create `SKILL.md` with proper frontmatter
3. Include clear instructions and examples
4. Add references to relevant documentation
5. Test skill with Copilot agent

## Skill Development Guidelines

When creating or updating skills:

- **Be specific:** Provide concrete, actionable instructions
- **Include examples:** Show code examples and commands
- **Reference documentation:** Link to relevant docs and conventions
- **Keep focused:** Each skill should address one specific domain
- **Test thoroughly:** Ensure instructions work as documented

## Relationship to Copilot Instructions

These skills complement the main [copilot-instructions.md](../copilot-instructions.md):

- **Copilot Instructions:** General guidance loaded for all tasks
- **Skills:** Specialized, task-specific guidance loaded on-demand

Skills should reference and build upon copilot instructions, not duplicate them.

## Contributing

When adding new skills, consider:
- Common issues encountered in the repository
- Repetitive tasks that need guidance
- Complex procedures that benefit from step-by-step instructions
- Platform-specific knowledge (build system, conventions, etc.)

## References

- [GitHub Copilot Agent Skills Documentation](https://docs.github.com/en/copilot/concepts/agents/about-agent-skills)
- [.NET IoT Copilot Instructions](../copilot-instructions.md)
- [.NET IoT Contributing Guide](../../Documentation/CONTRIBUTING.md)
- [Device Conventions](../../Documentation/Devices-conventions.md)
