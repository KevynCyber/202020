namespace TwentyTwentyTwenty;

public class OverlayForm : Form
{
    private readonly Label _countdownLabel;
    private readonly Label _messageLabel;

    public OverlayForm(Screen screen)
    {
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.Black;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        DoubleBuffered = true;
        Cursor = Cursors.WaitCursor;

        _messageLabel = new Label
        {
            Text = "Look at something 20 feet away",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 28f, FontStyle.Regular),
            AutoSize = true,
        };

        _countdownLabel = new Label
        {
            Text = "20",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 120f, FontStyle.Bold),
            AutoSize = true,
        };

        Controls.Add(_messageLabel);
        Controls.Add(_countdownLabel);

        Bounds = screen.Bounds;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        CenterLabels();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        CenterLabels();
    }

    private void CenterLabels()
    {
        _countdownLabel.Left = (ClientSize.Width - _countdownLabel.Width) / 2;
        _countdownLabel.Top = (ClientSize.Height - _countdownLabel.Height) / 2 - 40;

        _messageLabel.Left = (ClientSize.Width - _messageLabel.Width) / 2;
        _messageLabel.Top = _countdownLabel.Bottom + 20;
    }

    public void UpdateCountdown(int seconds)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateCountdown(seconds));
            return;
        }
        _countdownLabel.Text = seconds.ToString();
        CenterLabels();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape ||
            (e.Control && e.KeyCode == Keys.C))
        {
            e.Handled = true;
            OnDismissRequested();
            return;
        }
        base.OnKeyDown(e);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            OnDismissRequested();
            return;
        }
        base.OnFormClosing(e);
    }

    public event Action? DismissRequested;

    private void OnDismissRequested()
    {
        DismissRequested?.Invoke();
    }
}
