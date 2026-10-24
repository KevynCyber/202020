using TwentyTwentyTwenty;

namespace TwentyTwentyTwenty.Tests;

// Requirement: the settings window applies changes with an Apply button that is enabled only
// while some control differs from its last loaded/applied value; Apply persists the values,
// notifies listeners once, keeps the window open, and shows "Applied" until the next change.
// Covers: ESC-001
public class SettingsFormTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _tempFile;
    private readonly SettingsManager _manager;

    public SettingsFormTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "202020-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _tempFile = Path.Combine(_tempDir, "settings.json");
        _manager = new SettingsManager(_tempFile);
        _manager.Load();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [StaFact]
    public void NewForm_CannotApply_ButtonDisabledWithApplyText()
    {
        using var form = new SettingsForm(_manager);

        Assert.False(form.CanApply);
        Assert.False(form.ApplyButton.Enabled);
        Assert.Equal("Apply", form.ApplyButton.Text);
    }

    [StaFact]
    public void ChangingInterval_EnablesApply()
    {
        using var form = new SettingsForm(_manager);

        form.IntervalInput.Value = form.IntervalInput.Value + 1;

        Assert.True(form.CanApply);
        Assert.True(form.ApplyButton.Enabled);
    }

    [StaFact]
    public void RevertingInterval_DisablesApply()
    {
        using var form = new SettingsForm(_manager);
        var original = form.IntervalInput.Value;

        form.IntervalInput.Value = original + 1;
        form.IntervalInput.Value = original;

        Assert.False(form.CanApply);
        Assert.False(form.ApplyButton.Enabled);
    }

    [StaFact]
    public void ChangingBreakDuration_EnablesApply()
    {
        using var form = new SettingsForm(_manager);

        form.BreakInput.Value = form.BreakInput.Value + 1;

        Assert.True(form.CanApply);
    }

    [StaFact]
    public void TogglingSound_EnablesApply_AndTogglingBack_Disables()
    {
        using var form = new SettingsForm(_manager);

        form.SoundCheck.Checked = !form.SoundCheck.Checked;
        Assert.True(form.CanApply);

        form.SoundCheck.Checked = !form.SoundCheck.Checked;
        Assert.False(form.CanApply);
    }

    [StaFact]
    public void TogglingAllowDismiss_EnablesApply_AndTogglingBack_Disables()
    {
        using var form = new SettingsForm(_manager);

        form.AllowDismissCheck.Checked = !form.AllowDismissCheck.Checked;
        Assert.True(form.CanApply);

        form.AllowDismissCheck.Checked = !form.AllowDismissCheck.Checked;
        Assert.False(form.CanApply);
    }

    [StaFact]
    public void Apply_PersistsValues_RaisesAppliedOnce_ResetsCanApply()
    {
        using var form = new SettingsForm(_manager);
        int applied = 0;
        form.Applied += () => applied++;

        form.IntervalInput.Value = 37;
        form.BreakInput.Value = 45;
        form.SoundCheck.Checked = false;
        form.AllowDismissCheck.Checked = false;
        form.Apply();

        Assert.Equal(1, applied);
        Assert.False(form.CanApply);
        Assert.False(form.ApplyButton.Enabled);
        Assert.Equal("Applied", form.ApplyButton.Text);
        Assert.False(form.IsDisposed);

        var reloaded = new SettingsManager(_tempFile);
        reloaded.Load();
        Assert.Equal(37, reloaded.Current.IntervalMinutes);
        Assert.Equal(45, reloaded.Current.BreakDurationSeconds);
        Assert.False(reloaded.Current.SoundEnabled);
        Assert.False(reloaded.Current.AllowEscapeDismiss);
    }

    [StaFact]
    public void Apply_ResetsBaseline_SoRevertingToOldValueEnablesApply()
    {
        using var form = new SettingsForm(_manager);
        var original = form.IntervalInput.Value;

        form.IntervalInput.Value = original + 1;
        form.Apply();
        form.IntervalInput.Value = original;

        Assert.True(form.CanApply);
    }

    [StaFact]
    public void ChangeAfterApply_RestoresApplyText()
    {
        using var form = new SettingsForm(_manager);

        form.IntervalInput.Value = form.IntervalInput.Value + 1;
        form.Apply();
        form.BreakInput.Value = form.BreakInput.Value + 1;

        Assert.True(form.CanApply);
        Assert.True(form.ApplyButton.Enabled);
        Assert.Equal("Apply", form.ApplyButton.Text);
    }

    [StaFact]
    public void Apply_WhenNothingChanged_IsNoOp()
    {
        using var form = new SettingsForm(_manager);
        int applied = 0;
        form.Applied += () => applied++;

        form.Apply();

        Assert.Equal(0, applied);
        Assert.False(File.Exists(_tempFile));
        Assert.Equal("Apply", form.ApplyButton.Text);
    }
}
