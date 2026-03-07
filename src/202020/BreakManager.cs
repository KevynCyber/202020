namespace TwentyTwentyTwenty;

public sealed class BreakManager : IDisposable
{
    private readonly SettingsManager _settingsManager;
    private readonly System.Windows.Forms.Timer _intervalTimer;
    private readonly System.Windows.Forms.Timer _countdownTimer;
    private readonly List<OverlayForm> _overlays = new();
    private int _remainingSeconds;
    private bool _isBreakActive;
    private bool _isPaused;

    public bool IsBreakActive => _isBreakActive;
    public bool IsPaused => _isPaused;

    public event Action? BreakStarted;
    public event Action? BreakEnded;
    public event Action<int>? CountdownTick;

    public BreakManager(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;

        _intervalTimer = new System.Windows.Forms.Timer();
        _intervalTimer.Tick += OnIntervalElapsed;
        UpdateIntervalFromSettings();

        _countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _countdownTimer.Tick += OnCountdownTick;
    }

    public void Start()
    {
        _isPaused = false;
        UpdateIntervalFromSettings();
        _intervalTimer.Start();
    }

    public void Stop()
    {
        _intervalTimer.Stop();
        EndBreak();
    }

    public void Pause()
    {
        _isPaused = true;
        _intervalTimer.Stop();
        if (_isBreakActive)
            EndBreak();
    }

    public void Resume()
    {
        _isPaused = false;
        ResetInterval();
    }

    public void TakeBreakNow()
    {
        if (_isBreakActive) return;
        _intervalTimer.Stop();
        StartBreak();
    }

    public void OnSessionLocked()
    {
        _intervalTimer.Stop();
        if (_isBreakActive)
            EndBreak();
    }

    public void OnSessionUnlocked()
    {
        if (!_isPaused)
            ResetInterval();
    }

    private void OnIntervalElapsed(object? sender, EventArgs e)
    {
        if (_isBreakActive) return;
        _intervalTimer.Stop();
        StartBreak();
    }

    private void StartBreak()
    {
        _isBreakActive = true;
        _remainingSeconds = _settingsManager.Current.BreakDurationSeconds;

        foreach (var screen in Screen.AllScreens)
        {
            var overlay = new OverlayForm(screen);
            overlay.DismissRequested += DismissBreak;
            overlay.UpdateCountdown(_remainingSeconds);
            _overlays.Add(overlay);
            overlay.Show();
            overlay.Activate();
        }

        if (_overlays.Count > 0)
            _overlays[0].Focus();

        BreakStarted?.Invoke();
        _countdownTimer.Start();
    }

    private void OnCountdownTick(object? sender, EventArgs e)
    {
        _remainingSeconds--;
        CountdownTick?.Invoke(_remainingSeconds);

        foreach (var overlay in _overlays)
        {
            if (!overlay.IsDisposed)
                overlay.UpdateCountdown(_remainingSeconds);
        }

        if (_remainingSeconds <= 0)
            EndBreak();
    }

    private void DismissBreak()
    {
        EndBreak();
    }

    private void EndBreak()
    {
        if (!_isBreakActive) return;

        _isBreakActive = false;
        _countdownTimer.Stop();

        foreach (var overlay in _overlays)
        {
            overlay.DismissRequested -= DismissBreak;
            if (!overlay.IsDisposed)
            {
                overlay.Close();
                overlay.Dispose();
            }
        }
        _overlays.Clear();

        BreakEnded?.Invoke();

        if (!_isPaused)
            ResetInterval();
    }

    private void ResetInterval()
    {
        UpdateIntervalFromSettings();
        _intervalTimer.Start();
    }

    private void UpdateIntervalFromSettings()
    {
        _intervalTimer.Interval = _settingsManager.Current.IntervalMinutes * 60 * 1000;
    }

    public void Dispose()
    {
        _intervalTimer.Stop();
        _countdownTimer.Stop();
        EndBreak();
        _intervalTimer.Dispose();
        _countdownTimer.Dispose();
    }
}
