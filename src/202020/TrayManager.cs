namespace TwentyTwentyTwenty;

public sealed class TrayManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;
    private readonly ToolStripMenuItem _soundItem;
    private readonly ToolStripMenuItem _autoStartItem;
    private readonly ToolStripMenuItem _pauseItem;
    private readonly SettingsManager _settingsManager;

    public event Action? TakeBreakNowClicked;
    public event Action? ExitClicked;
    public event Action<bool>? PauseToggled;

    public TrayManager(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        _menu = new ContextMenuStrip();

        var takeBreakItem = new ToolStripMenuItem("Take Break Now");
        takeBreakItem.Click += (_, _) => TakeBreakNowClicked?.Invoke();

        _soundItem = new ToolStripMenuItem("Sound Enabled")
        {
            Checked = settingsManager.Current.SoundEnabled,
            CheckOnClick = true
        };
        _soundItem.CheckedChanged += (_, _) =>
        {
            settingsManager.Current.SoundEnabled = _soundItem.Checked;
            settingsManager.Save();
        };

        _autoStartItem = new ToolStripMenuItem("Start with Windows")
        {
            Checked = AutoStartManager.IsEnabled(),
            CheckOnClick = true
        };
        _autoStartItem.CheckedChanged += (_, _) =>
        {
            AutoStartManager.SetEnabled(_autoStartItem.Checked);
            settingsManager.Current.AutoStart = _autoStartItem.Checked;
            settingsManager.Save();
        };

        _pauseItem = new ToolStripMenuItem("Pause Timer");
        _pauseItem.Click += (_, _) =>
        {
            _pauseItem.Checked = !_pauseItem.Checked;
            PauseToggled?.Invoke(_pauseItem.Checked);
            UpdateTooltip();
        };

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => ExitClicked?.Invoke();

        _menu.Items.Add(takeBreakItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(_soundItem);
        _menu.Items.Add(_autoStartItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(_pauseItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadIcon(),
            ContextMenuStrip = _menu,
            Visible = true,
        };
        UpdateTooltip();
    }

    private void UpdateTooltip()
    {
        var status = _pauseItem.Checked ? "Paused" : "Active";
        _notifyIcon.Text = $"20-20-20 ({status})";
    }

    private static Icon LoadIcon()
    {
        var asm = typeof(TrayManager).Assembly;
        var stream = asm.GetManifestResourceStream("TwentyTwentyTwenty.Resources.icon.ico");
        if (stream != null)
            return new Icon(stream);

        return SystemIcons.Application;
    }

    public void SetPaused(bool paused)
    {
        _pauseItem.Checked = paused;
        UpdateTooltip();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
    }
}
