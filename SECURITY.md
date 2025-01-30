# Security Policy

## Supported Versions

We only provide support for the latest version of `System.Device.Gpio`. Issues and contributions related to `Iot.Device.Bindings` are community-driven and not officially supported by the maintainers.

## Considerations

- Default Trust: By default, many IoT devices assume inputs are from trusted sources. This is a risky assumption because attackers can inject malicious data or manipulate signals.
- Security Vulnerabilities: Attackers may craft inputs to exploit buffer overflows, force unexpected states, or gain unauthorized access to device functions.
- Mitigation:
  - Input Validation: Perform rigorous checks (e.g., sanitization, length checks, type validation) on every input—whether it’s sensor data, network traffic, or user-supplied information.
  - Least Privilege: Run IoT components with minimal permissions. Ensure that any compromised part of the system cannot escalate privileges or affect other parts.

## Reporting a Vulnerability

Security issues and bugs should be reported privately to the Microsoft Security Response Center (MSRC), either by emailing [secure@microsoft.com](mailto:secure@microsoft.com) or via the portal at <https://msrc.microsoft.com>.
You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your
original message. Further information, including the MSRC PGP key, can be found in the [MSRC Report an Issue FAQ](https://www.microsoft.com/en-us/msrc/faqs-report-an-issue).

Reports via MSRC may qualify for the .NET Core Bug Bounty. Details of the .NET Core Bug Bounty including terms and conditions are at [https://aka.ms/corebounty](https://aka.ms/corebounty).

Please do not open issues for anything you think might have a security implication.
