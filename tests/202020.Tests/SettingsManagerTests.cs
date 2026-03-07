using TwentyTwentyTwenty;

namespace TwentyTwentyTwenty.Tests;

public class SettingsManagerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _tempFile;

    public SettingsManagerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "202020-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _tempFile = Path.Combine(_tempDir, "settings.json");
    }

    [Fact]
    public void Load_NoFile_ReturnsDefaults()
    {
        var mgr = new SettingsManager(_tempFile);
        mgr.Load();

        Assert.Equal(20, mgr.Current.IntervalMinutes);
        Assert.Equal(20, mgr.Current.BreakDurationSeconds);
        Assert.True(mgr.Current.SoundEnabled);
        Assert.False(mgr.Current.AutoStart);
    }

    [Fact]
    public void SaveAndLoad_RoundTrips()
    {
        var mgr = new SettingsManager(_tempFile);
        mgr.Load();
        mgr.Current.IntervalMinutes = 15;
        mgr.Current.BreakDurationSeconds = 30;
        mgr.Current.SoundEnabled = false;
        mgr.Current.AutoStart = true;
        mgr.Save();

        var mgr2 = new SettingsManager(_tempFile);
        mgr2.Load();

        Assert.Equal(15, mgr2.Current.IntervalMinutes);
        Assert.Equal(30, mgr2.Current.BreakDurationSeconds);
        Assert.False(mgr2.Current.SoundEnabled);
        Assert.True(mgr2.Current.AutoStart);
    }

    [Fact]
    public void Load_CorruptJson_ReturnsDefaults()
    {
        File.WriteAllText(_tempFile, "not valid json!!!");
        var mgr = new SettingsManager(_tempFile);
        mgr.Load();

        Assert.Equal(20, mgr.Current.IntervalMinutes);
    }

    [Fact]
    public void Save_CreatesDirectory()
    {
        var nested = Path.Combine(_tempDir, "sub", "dir", "settings.json");
        var mgr = new SettingsManager(nested);
        mgr.Load();
        mgr.Save();

        Assert.True(File.Exists(nested));
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }
}
