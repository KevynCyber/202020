using TwentyTwentyTwenty;

namespace TwentyTwentyTwenty.Tests;

// Requirement: when the "allow Escape dismiss" setting is off, the break overlay cannot be
// dismissed by Escape, Ctrl+C, or a user close; when on, those inputs dismiss the break.
// Covers: ESC-001
public class OverlayFormTests
{
    private sealed class TestOverlayForm : OverlayForm
    {
        public TestOverlayForm(bool allowDismiss) : base(Screen.PrimaryScreen!, allowDismiss) { }

        public void PressKey(Keys keyData) => OnKeyDown(new KeyEventArgs(keyData));

        public FormClosingEventArgs SimulateClosing(CloseReason reason)
        {
            var args = new FormClosingEventArgs(reason, false);
            OnFormClosing(args);
            return args;
        }
    }

    private static int CountDismissals(TestOverlayForm form, Action<TestOverlayForm> act)
    {
        int count = 0;
        form.DismissRequested += () => count++;
        act(form);
        return count;
    }

    [StaTheory]
    [InlineData(Keys.Escape)]
    [InlineData(Keys.Control | Keys.C)]
    public void KeyDown_AllowDismissFalse_DoesNotRaiseDismissRequested(Keys keys)
    {
        using var form = new TestOverlayForm(allowDismiss: false);

        Assert.Equal(0, CountDismissals(form, f => f.PressKey(keys)));
    }

    [StaTheory]
    [InlineData(Keys.Escape)]
    [InlineData(Keys.Control | Keys.C)]
    public void KeyDown_AllowDismissTrue_RaisesDismissRequested(Keys keys)
    {
        using var form = new TestOverlayForm(allowDismiss: true);

        Assert.Equal(1, CountDismissals(form, f => f.PressKey(keys)));
    }

    [StaFact]
    public void UserClosing_AllowDismissFalse_CancelsWithoutDismiss()
    {
        using var form = new TestOverlayForm(allowDismiss: false);
        FormClosingEventArgs? args = null;

        int count = CountDismissals(form, f => args = f.SimulateClosing(CloseReason.UserClosing));

        Assert.Equal(0, count);
        Assert.True(args!.Cancel);
    }

    [StaFact]
    public void UserClosing_AllowDismissTrue_CancelsAndRaisesDismiss()
    {
        using var form = new TestOverlayForm(allowDismiss: true);
        FormClosingEventArgs? args = null;

        int count = CountDismissals(form, f => args = f.SimulateClosing(CloseReason.UserClosing));

        Assert.Equal(1, count);
        Assert.True(args!.Cancel);
    }
}
