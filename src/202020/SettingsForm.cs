namespace TwentyTwentyTwenty;

public class SettingsForm : Form
{
    private readonly SettingsManager _settingsManager;
    private readonly CheckBox _autoStartCheck;
    private readonly System.Windows.Forms.Timer _appliedTimer;

    // Last loaded/applied values; Apply is enabled only while a control differs from these.
    private decimal _baseInterval;
    private decimal _baseBreak;
    private bool _baseSound;
    private bool _baseAutoStart;
    private bool _baseAllowDismiss;

    internal NumericUpDown IntervalInput { get; }
    internal NumericUpDown BreakInput { get; }
    internal CheckBox SoundCheck { get; }
    internal CheckBox AllowDismissCheck { get; }
    internal Button ApplyButton { get; }
    internal Button CloseButton { get; }

    public event Action? Applied;

    public bool CanApply =>
        IntervalInput.Value != _baseInterval
        || BreakInput.Value != _baseBreak
        || SoundCheck.Checked != _baseSound
        || _autoStartCheck.Checked != _baseAutoStart
        || AllowDismissCheck.Checked != _baseAllowDismiss;

    public SettingsForm(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        var current = settingsManager.Current;

        Text = "20-20-20 Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        IntervalInput = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 240,
            Value = Math.Clamp(current.IntervalMinutes, 1, 240),
        };
        BreakInput = new NumericUpDown
        {
            Minimum = 5,
            Maximum = 600,
            Value = Math.Clamp(current.BreakDurationSeconds, 5, 600),
        };
        SoundCheck = new CheckBox { Text = "Sound enabled", AutoSize = true, Checked = current.SoundEnabled };
        _autoStartCheck = new CheckBox { Text = "Start with Windows", AutoSize = true, Checked = AutoStartManager.IsEnabled() };
        AllowDismissCheck = new CheckBox
        {
            Text = "Allow Esc / Alt+F4 to skip a break",
            AutoSize = true,
            Checked = current.AllowEscapeDismiss,
        };

        var buttonMinSize = new Size(75, 23);
        ApplyButton = new Button { Text = "Apply", Enabled = false, AutoSize = true, MinimumSize = buttonMinSize };
        CloseButton = new Button { Text = "Close", DialogResult = DialogResult.Cancel, AutoSize = true, MinimumSize = buttonMinSize };
        ApplyButton.Click += (_, _) => Apply();
        // The form is shown modeless (Show()), where DialogResult alone does not close it.
        CloseButton.Click += (_, _) => Close();
        AcceptButton = ApplyButton;
        CancelButton = CloseButton;

        _appliedTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        _appliedTimer.Tick += (_, _) =>
        {
            _appliedTimer.Stop();
            ApplyButton.Text = "Apply";
        };

        SetBaseline();
        IntervalInput.ValueChanged += (_, _) => OnControlChanged();
        BreakInput.ValueChanged += (_, _) => OnControlChanged();
        SoundCheck.CheckedChanged += (_, _) => OnControlChanged();
        _autoStartCheck.CheckedChanged += (_, _) => OnControlChanged();
        AllowDismissCheck.CheckedChanged += (_, _) => OnControlChanged();

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0, 8, 0, 0),
        };
        buttons.Controls.Add(CloseButton);
        buttons.Controls.Add(ApplyButton);

        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Location = new Point(Padding.Left, Padding.Top),
        };
        layout.Controls.Add(new Label { Text = "Break interval (minutes):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        layout.Controls.Add(IntervalInput, 1, 0);
        layout.Controls.Add(new Label { Text = "Break duration (seconds):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        layout.Controls.Add(BreakInput, 1, 1);
        layout.Controls.Add(SoundCheck, 0, 2);
        layout.SetColumnSpan(SoundCheck, 2);
        layout.Controls.Add(_autoStartCheck, 0, 3);
        layout.SetColumnSpan(_autoStartCheck, 2);
        layout.Controls.Add(AllowDismissCheck, 0, 4);
        layout.SetColumnSpan(AllowDismissCheck, 2);
        layout.Controls.Add(buttons, 0, 5);
        layout.SetColumnSpan(buttons, 2);

        Controls.Add(layout);
    }

    public void Apply()
    {
        if (!CanApply)
            return;

        var current = _settingsManager.Current;
        current.IntervalMinutes = (int)IntervalInput.Value;
        current.BreakDurationSeconds = (int)BreakInput.Value;
        current.SoundEnabled = SoundCheck.Checked;
        current.AutoStart = _autoStartCheck.Checked;
        current.AllowEscapeDismiss = AllowDismissCheck.Checked;
        // Only touch the registry when the user actually changed the autostart option.
        if (_autoStartCheck.Checked != _baseAutoStart)
            AutoStartManager.SetEnabled(_autoStartCheck.Checked);
        _settingsManager.Save();

        SetBaseline();
        ApplyButton.Enabled = false;
        ApplyButton.Text = "Applied";
        _appliedTimer.Stop();
        _appliedTimer.Start();
        Applied?.Invoke();
    }

    private void SetBaseline()
    {
        _baseInterval = IntervalInput.Value;
        _baseBreak = BreakInput.Value;
        _baseSound = SoundCheck.Checked;
        _baseAutoStart = _autoStartCheck.Checked;
        _baseAllowDismiss = AllowDismissCheck.Checked;
    }

    private void OnControlChanged()
    {
        _appliedTimer.Stop();
        ApplyButton.Text = "Apply";
        ApplyButton.Enabled = CanApply;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _appliedTimer.Dispose();
        base.Dispose(disposing);
    }
}
