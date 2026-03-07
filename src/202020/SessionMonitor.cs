using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace TwentyTwentyTwenty;

public sealed class SessionMonitor : IDisposable
{
    public event Action? SessionLocked;
    public event Action? SessionUnlocked;

    private readonly DisplayPowerWindow _displayPowerWindow;

    public SessionMonitor()
    {
        SystemEvents.SessionSwitch += OnSessionSwitch;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        _displayPowerWindow = new DisplayPowerWindow(this);
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        switch (e.Reason)
        {
            case SessionSwitchReason.SessionLock:
            case SessionSwitchReason.SessionLogoff:
                SessionLocked?.Invoke();
                break;
            case SessionSwitchReason.SessionUnlock:
            case SessionSwitchReason.SessionLogon:
                SessionUnlocked?.Invoke();
                break;
        }
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        switch (e.Mode)
        {
            case PowerModes.Suspend:
                SessionLocked?.Invoke();
                break;
            case PowerModes.Resume:
                SessionUnlocked?.Invoke();
                break;
        }
    }

    public void Dispose()
    {
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        _displayPowerWindow.Dispose();
    }

    private sealed class DisplayPowerWindow : NativeWindow, IDisposable
    {
        private const int WM_POWERBROADCAST = 0x0218;
        private const int PBT_POWERSETTINGCHANGE = 0x8013;

        private static readonly Guid GUID_CONSOLE_DISPLAY_STATE =
            new("6fe69556-704a-47a0-8f24-c28d936fda47");

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr RegisterPowerSettingNotification(
            IntPtr hRecipient, ref Guid powerSettingGuid, int flags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterPowerSettingNotification(IntPtr handle);

        [StructLayout(LayoutKind.Sequential)]
        private struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        private readonly SessionMonitor _owner;
        private IntPtr _notificationHandle;

        public DisplayPowerWindow(SessionMonitor owner)
        {
            _owner = owner;
            CreateHandle(new CreateParams());
            var guid = GUID_CONSOLE_DISPLAY_STATE;
            _notificationHandle = RegisterPowerSettingNotification(Handle, ref guid, 0);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_POWERBROADCAST && (int)m.WParam == PBT_POWERSETTINGCHANGE)
            {
                var setting = Marshal.PtrToStructure<POWERBROADCAST_SETTING>(m.LParam);
                if (setting.PowerSetting == GUID_CONSOLE_DISPLAY_STATE)
                {
                    switch (setting.Data)
                    {
                        case 0: // Display off
                            _owner.SessionLocked?.Invoke();
                            break;
                        case 1: // Display on
                            _owner.SessionUnlocked?.Invoke();
                            break;
                        // 2 = dimmed, no action
                    }
                }
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            if (_notificationHandle != IntPtr.Zero)
            {
                UnregisterPowerSettingNotification(_notificationHandle);
                _notificationHandle = IntPtr.Zero;
            }
            DestroyHandle();
        }
    }
}
