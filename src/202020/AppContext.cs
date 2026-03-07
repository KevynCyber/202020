namespace TwentyTwentyTwenty;

public class AppContext : ApplicationContext
{
    private readonly SettingsManager _settingsManager;
    private readonly BreakManager _breakManager;
    private readonly TrayManager _trayManager;
    private readonly SessionMonitor _sessionMonitor;

    public AppContext()
    {
        _settingsManager = new SettingsManager();
        _settingsManager.Load();

        _breakManager = new BreakManager(_settingsManager);
        _trayManager = new TrayManager(_settingsManager);
        _sessionMonitor = new SessionMonitor();

        _trayManager.TakeBreakNowClicked += () => _breakManager.TakeBreakNow();
        _trayManager.ExitClicked += () => ExitThread();
        _trayManager.PauseToggled += paused =>
        {
            if (paused) _breakManager.Pause();
            else _breakManager.Resume();
        };

        _breakManager.BreakStarted += () =>
        {
            if (_settingsManager.Current.SoundEnabled)
                ChimePlayer.PlayBreakStart();
        };

        _breakManager.BreakEnded += () =>
        {
            if (_settingsManager.Current.SoundEnabled)
                ChimePlayer.PlayBreakEnd();
        };

        _sessionMonitor.SessionLocked += () => _breakManager.OnSessionLocked();
        _sessionMonitor.SessionUnlocked += () => _breakManager.OnSessionUnlocked();

        _breakManager.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _breakManager.Dispose();
            _trayManager.Dispose();
            _sessionMonitor.Dispose();
        }
        base.Dispose(disposing);
    }
}
