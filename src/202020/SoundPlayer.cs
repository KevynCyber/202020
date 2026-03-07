using System.Media;

namespace TwentyTwentyTwenty;

public static class ChimePlayer
{
    public static void PlayBreakStart()
    {
        SystemSounds.Asterisk.Play();
    }

    public static void PlayBreakEnd()
    {
        SystemSounds.Exclamation.Play();
    }
}
