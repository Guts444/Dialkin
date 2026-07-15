# Dialkin Privacy Policy

**Effective date:** July 14, 2026

Dialkin is a local desktop utility. It does not collect, transmit, sell, or share personal information.

## Data processed

Dialkin reads aggregate CPU utilization and physical memory usage from Windows so it can draw the desktop meters. These values are processed locally in memory and are not retained or transmitted.

Dialkin does not include telemetry, analytics, advertising, crash reporting, user accounts, or network communication.

## Data stored on the device

Dialkin stores only the preferences needed to restore the widgets, including position, size, opacity, visibility, update interval, lock state, always-on-top state, click-through state, and the user's start-with-Windows choice.

For an unpackaged or portable copy, preferences are stored in:

```text
%APPDATA%\Dialkin\settings.json
```

The Microsoft Store/MSIX build stores the same preferences inside the app's Windows-managed package data directory.

If the user enables **Start with Windows**, the portable build creates a current-user Windows startup entry. The MSIX build uses the Windows StartupTask mechanism. The user can disable either option from Dialkin or Windows Settings.

## Data retention and deletion

Dialkin keeps preferences until the user deletes them or uninstalls the packaged application. Uninstalling the MSIX package removes its Windows-managed package data. Users of a portable build can remove all retained preferences by deleting `%APPDATA%\Dialkin` after exiting Dialkin.

Settings migrated from the former Vista Widgets application may remain in `%APPDATA%\VistaWidgets` until the user deletes that folder.

## Third parties

Dialkin does not send data to the developer or any third party. It has no third-party online services.

## Changes

Material changes to this policy will be documented in this repository and reflected by an updated effective date.

## Contact

Privacy questions can be filed at:

https://github.com/Guts444/Dialkin/issues
