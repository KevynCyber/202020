using Microsoft.Win32;
using TwentyTwentyTwenty;

namespace TwentyTwentyTwenty.Tests;

public class AutoStartManagerTests
{
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "202020";

    [Fact]
    public void SetEnabled_True_CreatesRegistryValue()
    {
        try
        {
            AutoStartManager.SetEnabled(true);
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
            var value = key?.GetValue(AppName);
            Assert.NotNull(value);
        }
        finally
        {
            // Clean up
            AutoStartManager.SetEnabled(false);
        }
    }

    [Fact]
    public void SetEnabled_False_RemovesRegistryValue()
    {
        AutoStartManager.SetEnabled(true);
        AutoStartManager.SetEnabled(false);

        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
        var value = key?.GetValue(AppName);
        Assert.Null(value);
    }

    [Fact]
    public void IsEnabled_ReflectsState()
    {
        try
        {
            AutoStartManager.SetEnabled(true);
            Assert.True(AutoStartManager.IsEnabled());

            AutoStartManager.SetEnabled(false);
            Assert.False(AutoStartManager.IsEnabled());
        }
        finally
        {
            AutoStartManager.SetEnabled(false);
        }
    }
}
