# 20-20-20 Eye Break

A lightweight Windows desktop app that enforces the 20-20-20 rule to prevent eye strain: every 20 minutes, look at something 20 feet away for 20 seconds.

## How It Works

The app runs silently in the system tray. Every 20 minutes, it covers all monitors with a black overlay showing a countdown timer and the message "Look at something 20 feet away." After 20 seconds, the overlay automatically dismisses. Press **Escape** to skip a break early.

## Features

- **Multi-monitor support** -- overlays all connected screens
- **System tray** -- runs unobtrusively with right-click menu
- **Take Break Now** -- trigger a break manually from the tray menu
- **Sound notifications** -- chime on break start/end (toggleable)
- **Start with Windows** -- optional auto-start via registry
- **Pause/Resume** -- pause the timer from the tray menu
- **Session-aware** -- pauses on lock/sleep/screen-off, resets timer on unlock/wake/screen-on
- **Single instance** -- prevents duplicate processes via named mutex
- **Escape to skip** -- dismiss any break early with Escape or Alt+F4

## Tray Menu

```
Take Break Now
--------------
Sound Enabled     (toggleable)
Start with Windows (toggleable)
--------------
Pause Timer
--------------
Exit
```

## Requirements

- Windows 10/11 with .NET 8 runtime (ships with Windows 11)

## Build & Run

```bash
dotnet build
dotnet run --project src/202020
```

## Test

```bash
dotnet test
```

## Settings

Settings are persisted as JSON in `%LOCALAPPDATA%\202020\settings.json`.

| Setting | Default |
|---------|---------|
| IntervalMinutes | 20 |
| BreakDurationSeconds | 20 |
| SoundEnabled | true |
| AutoStart | false |
