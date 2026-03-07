# Changelog

## [1.0.0] - 2026-03-04

### Added

- Core 20-20-20 break timer with fullscreen black overlay and countdown
- Multi-monitor support (one overlay per screen)
- System tray icon with context menu (Take Break Now, Sound, Auto-start, Pause, Exit)
- Escape / Alt+F4 to dismiss break early
- Sound notifications on break start/end (SystemSounds)
- Start with Windows toggle (HKCU registry)
- Session lock/unlock and sleep/wake detection (pauses timer, resets on unlock)
- Single-instance enforcement via named mutex
- JSON settings persistence to %LOCALAPPDATA%\202020\
- Unit tests for SettingsManager, AutoStartManager, and BreakManager
