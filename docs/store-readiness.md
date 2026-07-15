# Microsoft Store readiness

Dialkin is prepared as an x64 self-contained packaged classic Windows application. This document records the values and decisions used for the Partner Center submission. Product publication still requires Microsoft certification.

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
- Store identity version: `1.0.2.0`
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

## Partner Center identity

Dialkin was reserved in Partner Center on July 15, 2026. The package manifest uses the exact assigned values:

- Package Identity Name: `Guts444.Dialkin`
- Package Identity Publisher: `CN=0CE853A2-4D9A-43A6-B4CC-9371C515BC1C`
- Publisher display name: `Guts444`
- Store ID: `9N9GGH4B57N3`
- Package family name: `Guts444.Dialkin_jtc3039kpntyp`

## Submission boundary checklist

- [x] Reserve **Dialkin** in Partner Center.
- [x] Copy exact Store identity values into the package build.
- [ ] Rebuild the Store-targeted MSIX.
- [ ] Run Windows App Certification Kit against that exact artifact.
- [ ] Install, update, startup-toggle, and uninstall test in a clean VM/profile.
- [x] Publish the privacy policy at a stable public URL.
- [ ] Capture final screenshots.
- [ ] Review age rating, markets, pricing, and availability.
- [ ] Review the `runFullTrust` declaration and justification.
- [x] Receive explicit approval to submit for certification.
