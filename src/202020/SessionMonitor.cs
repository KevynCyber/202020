using Microsoft.Win32;

namespace TwentyTwentyTwenty;

public sealed class SessionMonitor : IDisposable
{
    public event Action? SessionLocked;
    public event Action? SessionUnlocked;

    public SessionMonitor()
    {
        SystemEvents.SessionSwitch += OnSessionSwitch;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        switch (e.Reason)
        {
            case SessionSwitchReason.SessionLock:
            case SessionSwitchReason.SessionLogoff:
                SessionLocked?.Invoke();
                break;
            case SessionSwitchReason.SessionUnlock:
            case SessionSwitchReason.SessionLogon:
                SessionUnlocked?.Invoke();
                break;
        }
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        switch (e.Mode)
        {
            case PowerModes.Suspend:
                SessionLocked?.Invoke();
                break;
            case PowerModes.Resume:
                SessionUnlocked?.Invoke();
                break;
        }
    }

    public void Dispose()
    {
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
    }
}
