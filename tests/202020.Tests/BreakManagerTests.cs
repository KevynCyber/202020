using TwentyTwentyTwenty;

namespace TwentyTwentyTwenty.Tests;

public class BreakManagerTests : IDisposable
{
    private readonly string _tempFile;
    private readonly SettingsManager _settingsManager;

    public BreakManagerTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"202020-test-{Guid.NewGuid():N}.json");
        _settingsManager = new SettingsManager(_tempFile);
        _settingsManager.Load();
    }

    [StaFact]
    public void TakeBreakNow_SetsIsBreakActive()
    {
        using var mgr = new BreakManager(_settingsManager);
        mgr.Start();

        mgr.TakeBreakNow();

        Assert.True(mgr.IsBreakActive);
    }

    [StaFact]
    public void TakeBreakNow_DoubleCall_GuardsReentry()
    {
        using var mgr = new BreakManager(_settingsManager);
        mgr.Start();

        int startCount = 0;
        mgr.BreakStarted += () => startCount++;

        mgr.TakeBreakNow();
        mgr.TakeBreakNow(); // should be no-op

        Assert.Equal(1, startCount);
    }

    [StaFact]
    public void Pause_StopsTimerAndEndsBreak()
    {
        using var mgr = new BreakManager(_settingsManager);
        mgr.Start();

        mgr.TakeBreakNow();
        Assert.True(mgr.IsBreakActive);

        mgr.Pause();
        Assert.False(mgr.IsBreakActive);
        Assert.True(mgr.IsPaused);
    }

    [StaFact]
    public void Resume_AfterPause_IsNotPaused()
    {
        using var mgr = new BreakManager(_settingsManager);
        mgr.Start();

        mgr.Pause();
        Assert.True(mgr.IsPaused);

        mgr.Resume();
        Assert.False(mgr.IsPaused);
    }

    [StaFact]
    public void OnSessionLocked_DuringBreak_EndsBreak()
    {
        using var mgr = new BreakManager(_settingsManager);
        mgr.Start();

        mgr.TakeBreakNow();
        Assert.True(mgr.IsBreakActive);

        mgr.OnSessionLocked();
        Assert.False(mgr.IsBreakActive);
    }

    [StaFact]
    public void BreakEvents_FireCorrectly()
    {
        using var mgr = new BreakManager(_settingsManager);
        mgr.Start();

        bool started = false, ended = false;
        mgr.BreakStarted += () => started = true;
        mgr.BreakEnded += () => ended = true;

        mgr.TakeBreakNow();
        Assert.True(started);

        mgr.Pause(); // ends break
        Assert.True(ended);
    }

    public void Dispose()
    {
        try { File.Delete(_tempFile); } catch { }
    }
}
