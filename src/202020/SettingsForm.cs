namespace TwentyTwentyTwenty;

public class SettingsForm : Form
{
    private readonly SettingsManager _settingsManager;
    private readonly NumericUpDown _intervalInput;
    private readonly NumericUpDown _breakInput;
    private readonly CheckBox _soundCheck;
    private readonly CheckBox _autoStartCheck;
    private readonly CheckBox _allowDismissCheck;

    public SettingsForm(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        var current = settingsManager.Current;

        Text = "20-20-20 Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        _intervalInput = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 240,
            Value = Math.Clamp(current.IntervalMinutes, 1, 240),
        };
        _breakInput = new NumericUpDown
        {
            Minimum = 5,
            Maximum = 600,
            Value = Math.Clamp(current.BreakDurationSeconds, 5, 600),
        };
        _soundCheck = new CheckBox { Text = "Sound enabled", AutoSize = true, Checked = current.SoundEnabled };
        _autoStartCheck = new CheckBox { Text = "Start with Windows", AutoSize = true, Checked = AutoStartManager.IsEnabled() };
        _allowDismissCheck = new CheckBox
        {
            Text = "Allow Esc / Alt+F4 to skip a break",
            AutoSize = true,
            Checked = current.AllowEscapeDismiss,
        };

        var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK };
        var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
        okButton.Click += (_, _) => ApplyAndSave();
        AcceptButton = okButton;
        CancelButton = cancelButton;

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Dock = DockStyle.Fill,
        };
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(okButton);

        var layout = new TableLayoutPanel { ColumnCount = 2, AutoSize = true };
        layout.Controls.Add(new Label { Text = "Break interval (minutes):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        layout.Controls.Add(_intervalInput, 1, 0);
        layout.Controls.Add(new Label { Text = "Break duration (seconds):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        layout.Controls.Add(_breakInput, 1, 1);
        layout.Controls.Add(_soundCheck, 0, 2);
        layout.SetColumnSpan(_soundCheck, 2);
        layout.Controls.Add(_autoStartCheck, 0, 3);
        layout.SetColumnSpan(_autoStartCheck, 2);
        layout.Controls.Add(_allowDismissCheck, 0, 4);
        layout.SetColumnSpan(_allowDismissCheck, 2);
        layout.Controls.Add(buttons, 0, 5);
        layout.SetColumnSpan(buttons, 2);

        Controls.Add(layout);
    }

    private void ApplyAndSave()
    {
        var current = _settingsManager.Current;
        current.IntervalMinutes = (int)_intervalInput.Value;
        current.BreakDurationSeconds = (int)_breakInput.Value;
        current.SoundEnabled = _soundCheck.Checked;
        current.AutoStart = _autoStartCheck.Checked;
        current.AllowEscapeDismiss = _allowDismissCheck.Checked;
        AutoStartManager.SetEnabled(_autoStartCheck.Checked);
        _settingsManager.Save();
    }
}
