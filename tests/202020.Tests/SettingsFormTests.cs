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

// Requirement: the settings window, shown modelessly from the tray, closes when the user clicks
// Close or presses Escape, and its Apply/Close buttons are fully visible inside the window with
// a margin below them (never clipped by the bottom edge).
// Covers: ESC-001
public class SettingsFormWindowTests : IDisposable
{
    private const int MinBottomMargin = 6;

    private readonly string _tempDir;
    private readonly SettingsManager _manager;

    public SettingsFormWindowTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "202020-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _manager = new SettingsManager(Path.Combine(_tempDir, "settings.json"));
        _manager.Load();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [StaFact]
    public void CloseButton_OnModelessForm_ClosesForm()
    {
        using var form = new SettingsForm(_manager);
        int closed = 0;
        form.FormClosed += (_, _) => closed++;
        form.Show();
        Application.DoEvents();

        form.CloseButton.PerformClick();
        Application.DoEvents();

        Assert.Equal(1, closed);
    }

    [StaFact]
    public void Escape_ViaCancelButton_OnModelessForm_ClosesForm()
    {
        using var form = new SettingsForm(_manager);
        int closed = 0;
        form.FormClosed += (_, _) => closed++;
        form.Show();
        Application.DoEvents();

        Assert.Same(form.CloseButton, form.CancelButton);
        ((Button)form.CancelButton!).PerformClick();
        Application.DoEvents();

        Assert.Equal(1, closed);
    }

    [StaFact]
    public void ApplyAndCloseButtons_FullyInsideClientArea_WithBottomMargin()
    {
        using var form = new SettingsForm(_manager);
        form.Show();
        Application.DoEvents();

        var client = form.RectangleToScreen(form.ClientRectangle);
        foreach (var button in new[] { form.ApplyButton, form.CloseButton })
        {
            var rect = button.RectangleToScreen(button.ClientRectangle);
            Assert.True(client.Contains(rect),
                $"{button.Text} button {rect} not inside client area {client}");
            Assert.True(client.Bottom - rect.Bottom >= MinBottomMargin,
                $"{button.Text} button bottom margin {client.Bottom - rect.Bottom}px < {MinBottomMargin}px (button {rect}, client {client})");
        }

        form.Close();
    }
}
