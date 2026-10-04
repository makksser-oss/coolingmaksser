# LOVINGCOOL Studio 2
Clean-room rewrite for LOVINGCOOL 6PRO 480x480.

## Product rules
- Logical editor is always upright 480x480.
- Physical rotation is an output transform only.
- No fake UI: unavailable features are explicitly experimental.
- Device I/O, sensors, rendering, media and UI are isolated.
- Projects are portable JSON + assets.
- Target: Windows 11 x64, self-contained desktop EXE.

## Modules
Studio2.App / Studio2.Core / Studio2.Device / Studio2.Sensors / Studio2.Media

## Planned working feature set
Scene editor; selection/move/resize; layers; undo/redo; snap/guides; text/clock/image/ring/bar/gauge/graph; hardware sensors; images/GIF/video; screen capture; profiles; tray/autostart; presets; device brightness/orientation; project import/export.

True Windows extended-desktop mode is separate because it requires an Indirect Display Driver and driver signing.