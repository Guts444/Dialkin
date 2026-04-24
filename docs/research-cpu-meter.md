# Research: Windows Vista / Windows 7 CPU Meter

## Sources Checked

- Microsoft Learn, [Windows Sidebar](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/sidebar/-sidebar-entry)
- Microsoft Learn, [Introduction to the Gadget Platform](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/gadgetplatform/introduction-to-the-gadget-platform)
- Microsoft Learn, [Desktop gadgets removed](https://learn.microsoft.com/en-us/windows/compatibility/desktop-gadgets-removed)
- O'Reilly / Pogue, [Windows Vista: The Missing Manual, section 6.9.3.4 CPU Meter](https://flylib.com/books/en/4.301.1.80/1/)

## Confirmed Behavior

- CPU Meter was part of the Windows Sidebar / Desktop Gadget era. Microsoft describes Vista Sidebar as the desktop host for small "gadgets", and Windows 7 as moving gadget hosting directly onto the desktop while preserving the gadget model.
- It was a lightweight desktop/sidebar glanceable utility, not a full performance monitor.
- The CPU Meter displayed two analog-style gauges.
- The left, larger gauge represented total CPU utilization as a percentage.
- The smaller gauge represented physical memory/RAM usage as a percentage.
- The original description focuses on current CPU and RAM load only.
- It did not break CPU usage down by socket, core, or per-core thread activity.
- It did not show GPU, network, disk, temperatures, processes, stocks, feeds, weather, or news. Those belong in separate widgets.
- Windows gadgets were based on HTML/script and the Sidebar gadget platform; this project intentionally does not use `.gadget` packaging, MSHTML, Internet Explorer hosting, ActiveX, or old Sidebar APIs.

## Platform Notes

Microsoft's archived Sidebar documentation describes gadgets as HTML/script mini-applications hosted by Sidebar or the Windows 7 desktop gadget platform. Microsoft later removed desktop gadgets from Windows 8 because the platform carried security risk, and advised desktop app developers not to package gadgets in installers.

For this project, the research implies a recreation should preserve the CPU Meter's small, visual, at-a-glance behavior while replacing the old platform with a normal native Windows application. The implementation should therefore use freshly drawn vector visuals and Windows-native telemetry APIs rather than extracting or embedding Microsoft gadget files or images.

## Visual Notes

The target is the familiar Vista-era small glass gadget: two overlapping circular analog gauges, chrome rings, off-white faces, red needles, dark percent readouts, soft shared shadow, and warm warning color bands near the high end of each dial. The current recreation avoids a separate backplate and instead makes the CPU and RAM gauges connect visually through overlap, matching the compact look of the original reference more closely.

The recreation should feel nostalgic and simple rather than becoming a modern dashboard.
