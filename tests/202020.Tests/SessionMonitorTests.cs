using TwentyTwentyTwenty;

namespace TwentyTwentyTwenty.Tests;

public class SessionMonitorTests : IDisposable
{
    private readonly SessionMonitor _monitor;

    public SessionMonitorTests()
    {
        _monitor = new SessionMonitor();
    }

    [StaFact]
    public void CreateAndDispose_DoesNotThrow()
    {
        // Verifies that the hidden window and power notification
        // registration/unregistration work without error.
        _monitor.Dispose();
    }

    [StaFact]
    public void Events_CanBeSubscribed()
    {
        bool lockedFired = false;
        bool unlockedFired = false;

        _monitor.SessionLocked += () => lockedFired = true;
        _monitor.SessionUnlocked += () => unlockedFired = true;

        // Events are wired but only fire from OS signals,
        // so we just verify subscription doesn't throw.
        Assert.False(lockedFired);
        Assert.False(unlockedFired);
    }

    public void Dispose()
    {
        _monitor.Dispose();
    }
}
