# OnTopReplica

**A real-time always-on-top "replica" of a window of your choice, plus a pixel-accurate image overlay tool, for Windows Vista, 7, 8, 10, or 11.**

This utility shows a blank always-on-top window by default. It supports two source modes:

- **Thumbnail mode** — clone any window of the system as an always up-to-date live thumbnail. Useful for monitoring background processes, wrangling complex multi-window games or tools, watching videos while working, and so on.
- **Image mode** — place a static image as a pixel-accurate overlay anywhere on screen. Useful for design comparison, reference overlays (PureRef-like), pixel alignment, and screen layout planning.

## 📢 Features

### Thumbnail mode (original)

- Clone any window and keep it *always-on-top* while working with others
- Select a subregion of the cloned window
  - Stored for future use
  - Relative coordinates from window borders supported
- Auto-resizing: fit original, half, quarter, fullscreen
- "Click forwarding" to interact with the cloned window
- "Group switch" mode auto-switches through a group of windows
- "Click-through" makes the replica ignore mouse input (combined with partial opacity = unobtrusive overlay)

### Image mode (new)

- **Load any image** (.png / .jpg / .bmp / .gif / .tif / .webp) via menu or **drag-and-drop**
- Loads at native **1:1 pixel size** for accurate overlay
- **Image layer manager** — save multiple images as "layers", each with (position, size, scale, opacity); swap instantly between them
  - Toggleable layer panel via hotkey **Ctrl+Shift+I**
  - Add / update / delete / apply / reorder (▲▼)
  - **Per-layer global hotkey assignment** — click a row, press a key combo (e.g. `Ctrl+Alt+1`) in the hotkey box, press *Set* → that combo applies the layer from anywhere; conflicts auto-clear the previous owner
- **Placement mode** — form expands to cover the current monitor with a click-through background; drag the image inside to position pixel-precise on the desktop, then exit to lock in
- **Fit to image size (1:1)** — snap form back to native pixel size at any time
- **Fit to monitor** — cover the entire monitor including the taskbar (one-click full coverage)

### Shared between modes

- Always on top, adjustable opacity, hide-chrome, position lock to screen corners
- Pixel-precise positioning dialogs: **Set position…**, **Set size…**, **Set scale…** (with live preview, Cancel restores original)
- **Resize lock** to pin the current window size

## Hotkeys

| Hotkey | Action |
|--------|--------|
| `Ctrl+Shift+C` | Clone current foreground window |
| `Ctrl+Shift+O` | Show / hide OnTopReplica |
| `Ctrl+Shift+I` | Toggle the image layer panel |
| `Ctrl+Alt+<key>` (configurable per layer) | Apply that layer instantly |
| `F11` / image double-click | Toggle fullscreen |
| `ESC` | Exit placement mode → click-through → fullscreen → click-forwarding (in order) |
| `Alt+1/2/3/4` | Quarter / Half / Original / Double scale |

## Image overlay workflow

1. Launch OnTopReplica
2. Drag an image onto the window, or right-click → **Load image…**
3. Drag the window with the mouse to roughly position
4. (Optional) Right-click → **Resize** → **Placement mode** for pixel-precise placement: image becomes draggable on a transparent full-monitor canvas; ESC to commit
5. Right-click → **Image presets…** (or `Ctrl+Shift+I`) → **Add current** to save the layout
6. Repeat for additional images; double-click any preset in the list to swap instantly
7. (Optional) Select a preset, type a key combo in the **Hotkey** box, press *Set* — that combo now applies the preset from anywhere in Windows

## Requirements

- Microsoft Windows Vista or greater (uses DWM Thumbnails)
- Microsoft .NET Framework 4.7
- Desktop Composition (Windows Aero) enabled

## Installation

Get the [latest version](https://github.com/LorenzCK/OnTopReplica/releases) from the releases section as an MSI installer.

## Building from Source

See [BUILD.md](BUILD.md) for detailed build instructions, environment setup, and test runner usage.

## Contributions

…are very welcome. Fork away! 🍽️

Submitting [issues](https://github.com/LorenzCK/OnTopReplica/issues) and other feedback is also appreciated.

### Roadmap

1. ✅ Update to the newest [WindowsFormsAero](https://github.com/LorenzCK/WindowsFormsAero) version.
1. ✅ Migrate to .NET 4.7.
1. ✅ Korean localization, position/size/scale input dialogs, resize lock.
1. ✅ Static-image overlay mode with presets and placement mode.
1. ✅ Per-preset hotkeys (assignable from the layer panel) for instant slot recall.
1. Improve / add **High DPI** support!
1. "Stored scenarios" that auto-clone a window by title/class and apply region/options. Ideally as Taskbar shortlinks.
1. Move to the Windows Store via Centennial. 🤞

## License

**OnTopReplica** is licensed under the [MS-RL (Microsoft Reciprocal License)](https://github.com/LorenzCK/OnTopReplica/blob/master/LICENSE).
