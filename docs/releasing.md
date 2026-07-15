# Release and packaging

Dialkin supports a self-contained portable build and a manually generated x64 MSIX package. The manual path is deliberate: this repository does not require the Visual Studio MSIX packaging workload or a `.wapproj`.

## Versions

- Product version: `0.2.0`
- Development MSIX identity version: `1.0.2.0`
- Packaging Windows SDK: `10.0.26100.0`

The MSIX major version is nonzero, and its fourth component remains zero because Microsoft Store reserves that component. Increment one of the first three components for every Store update.

Product metadata lives in `src/Dialkin.App/Dialkin.App.csproj`. Package defaults live in `packaging/AppxManifest.xml` and `scripts/build-msix.ps1`.

## Focused verification

```powershell
dotnet restore
dotnet test Dialkin.sln -c Release --no-restore
dotnet publish src/Dialkin.App -c Release -r win-x64 --self-contained true -o artifacts\portable
```

Manual checks before a public build:

- Launch and confirm both gauges update.
- Compare CPU and RAM values roughly with Task Manager.
- Drag, exit, relaunch, and confirm the saved position.
- Exercise lock, always-on-top, click-through, opacity, size, visibility, and update interval.
- Move between monitors with different scaling.
- Confirm sleep/resume sampling recovery.
- Confirm a second launch exits without creating another widget.
- Confirm Start with Windows can be enabled and disabled.

## Portable build

```powershell
dotnet publish src/Dialkin.App `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -o artifacts\Dialkin-0.2.0-win-x64-self-contained

Compress-Archive `
  -Path artifacts\Dialkin-0.2.0-win-x64-self-contained\* `
  -DestinationPath artifacts\Dialkin-0.2.0-win-x64-self-contained.zip `
  -Force
```

## Development MSIX

Build and sign with a locally generated development certificate:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-msix.ps1 -SignWithTestCertificate
```

Outputs:

```text
artifacts\msix\Dialkin-1.0.2.0-x64.msix
artifacts\msix\Dialkin-Development.cer
```

The script performs:

1. Self-contained x64 `dotnet publish`.
2. Staging of application files, manifest, and package artwork.
3. Packing with Windows SDK `10.0.26100.0` `MakeAppx.exe` and its limited package validation.
4. Optional test-certificate creation and SHA-256 signing with `SignTool.exe`.
5. A SHA-256 digest and JSON provenance record containing source revision, dirty state, and toolchain versions.

Generated publish, staging, certificate, and package files stay under ignored `artifacts/` directories.

### Local packaged-development check without elevation

On a machine with Developer Mode enabled, the staged package can be registered directly:

```powershell
Add-AppxPackage -Register .\artifacts\msix\stage\AppxManifest.xml
```

Launch `Dialkin` from Start, then remove the development registration:

```powershell
Get-AppxPackage Guts444.Dialkin | Remove-AppxPackage
```

This exercises package identity, the packaged-classic-app entry point, package-local settings, and startup-task registration. It does not replace testing the final Store-associated MSIX.

### Installing the signed development MSIX

Windows requires the self-signed development certificate to be trusted at the computer level. From an elevated PowerShell session on a disposable test machine or VM:

```powershell
Import-Certificate `
  -FilePath .\artifacts\msix\Dialkin-Development.cer `
  -CertStoreLocation Cert:\LocalMachine\TrustedPeople

Add-AppxPackage .\artifacts\msix\Dialkin-1.0.2.0-x64.msix
```

Remove the package and development certificate after testing. Never distribute the development certificate as a production trust anchor.

## Partner Center boundary

The checked-in manifest intentionally uses development identity values. After reserving **Dialkin** in Partner Center, start from a reviewed, clean, tagged source revision. Copy the exact case-sensitive values from **Product identity** and provide them to the script:

```powershell
.\scripts\build-msix.ps1 `
  -IdentityName '<Package/Identity/Name>' `
  -Publisher '<Package/Identity/Publisher>' `
  -PublisherDisplayName '<Package/Properties/PublisherDisplayName>' `
  -RequireCleanWorktree
```

Do not guess these values. The final Store artifact must use the assigned identity.

Before submission:

- Associate the final package identity.
- Rebuild with the assigned values and no development signing requirement.
- Retain the tagged source revision, generated provenance JSON, and MSIX SHA-256 digest.
- Run the Windows App Certification Kit against that package.
- Verify install, launch, startup toggle, update, and uninstall in a clean VM/user profile.
- Host `PRIVACY.md` at a stable public URL.
- Supply the `runFullTrust` justification from `docs/store-readiness.md`.
- Add screenshots and final listing copy.

Microsoft signs accepted Store packages. Do not submit or publish until the Partner Center identity and listing are deliberately approved.
