namespace TwentyTwentyTwenty;

static class Program
{
    private static Mutex? _mutex;

    [STAThread]
    static void Main()
    {
        _mutex = new Mutex(true, @"Global\TwentyTwentyTwentyApp", out bool createdNew);
        if (!createdNew)
        {
            // Another instance is already running
            return;
        }

        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new AppContext());
        }
        finally
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
    }
}
