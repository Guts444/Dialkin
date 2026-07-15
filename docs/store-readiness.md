# Microsoft Store readiness

Dialkin is prepared as an x64 self-contained packaged classic Windows application. This document records the values and decisions needed when the Partner Center submission begins. It is not authorization to publish.

## Suggested listing

**Product name:** Dialkin

**Tagline:** Little instruments for your desktop.

**Category:** Utilities & tools

**Short description:** A lightweight native CPU and memory meter inspired by classic Windows desktop gadgets.

**Description:**

> Dialkin brings friendly little instruments back to the Windows desktop. Its first widget is an original reimagining of the classic dual-dial CPU Meter, with live CPU and physical memory readings, smooth animated needles, adjustable size and opacity, saved placement, always-on-top, lock, and click-through controls.
>
> Dialkin is a native Windows application with no ads, accounts, telemetry, or network communication. System readings and preferences stay on the device.

## Package profile

- Architecture: x64
- Runtime: .NET 10, self-contained
- Minimum OS: Windows 10 version 2004 (`10.0.19041.0`)
- Package type: MSIX packaged classic app
- Trust level: medium integrity
- Capability: `runFullTrust`
- Startup task: `Dialkin.Startup`, disabled by default
- Development identity version: `1.0.2.0`
- Public product version: `0.2.0`

## `runFullTrust` justification

Suggested Partner Center text:

> Dialkin is a native WPF desktop widget that displays local CPU and physical memory utilization. It requires `runFullTrust` because it is a packaged classic desktop application, uses Win32 system APIs to read aggregate performance metrics, creates transparent tool windows and a notification-area icon, and optionally applies native extended window styles for click-through behavior. Dialkin does not elevate, install services or drivers, access user documents, collect personal data, communicate over a network, or run code downloaded from the internet.

## Privacy declaration

- No personal data is collected or transmitted.
- No telemetry, analytics, advertising, accounts, or networking are present.
- Aggregate CPU and memory readings are processed locally in memory.
- Widget preferences are stored locally.
- Privacy policy source: [`PRIVACY.md`](../PRIVACY.md)
- Intended public URL: `https://github.com/Guts444/Dialkin/blob/main/PRIVACY.md`

## Assets available

- Executable/tray icon: `assets/app.ico`
- 1024 px source artwork: `assets/app-icon-1024.png`
- 300 px Store logo: `assets/store-logo-300.png`
- Manifest assets: `packaging/Assets/`
- Reproducible asset generator: `scripts/generate-assets.py`

Partner Center screenshots still need to be captured from the final build. Do not use screenshots containing unrelated desktop content or private information.

## Values that must come from Partner Center

After reserving the product, replace the development placeholders with the exact values shown under **Product identity**:

- Package Identity Name
- Package Identity Publisher
- Publisher display name

The final Store package must not use `Guts444.Dialkin.Development` or `CN=Guts444-Dialkin-Development`.

## Submission boundary checklist

- [ ] Reserve **Dialkin** in Partner Center.
- [ ] Copy exact Store identity values into the package build.
- [ ] Rebuild the Store-targeted MSIX.
- [ ] Run Windows App Certification Kit against that exact artifact.
- [ ] Install, update, startup-toggle, and uninstall test in a clean VM/profile.
- [ ] Publish the privacy policy at a stable public URL.
- [ ] Capture final screenshots.
- [ ] Review age rating, markets, pricing, and availability.
- [ ] Review the `runFullTrust` declaration and justification.
- [ ] Submit only after explicit final approval.
