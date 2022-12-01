using System;
using System.Collections.Generic;
using System.Linq;

namespace DesktopTools.util
{
    public class SettingUtil
    {
        private static Dictionary<string, string?> CacheValue = new Dictionary<string, string?>();
        public static bool SetSetting(string key, string value)
        {
            CacheValue[key] = value;
            Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\desktop_tools");
            try
            {
                rk2.SetValue(key, value);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                rk2.Close();
            }
        }

        public static string GetSetting(string key, string def = "")
        {
            if (CacheValue.ContainsKey(key))
            {
                var v = CacheValue[key];
                if (null == v)
                {
                    return def;
                }
                return v;
            }
            Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\desktop_tools");
            try
            {
                string? val = null;
                var v = rk2.GetValue(key);
                if (v != null)
                {
                    val = v.ToString();
                }
                if (string.IsNullOrEmpty(val))
                {
                    CacheValue[key] = def;
                    return def;
                }
                CacheValue[key] = val;
                return val;
            }
            catch
            {
                CacheValue[key] = def;
                return def;
            }
            finally
            {
                rk2.Close();
            }
        }

        public static string? GetSettingOrDefValueIfNotExists(string key, string def = "")
        {
            if (CacheValue.ContainsKey(key))
            {
                return CacheValue[key];
            }
            Microsoft.Win32.RegistryKey rk2 = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\desktop_tools");
            try
            {
                var v = rk2.GetValueNames().Contains(key) ? rk2.GetValue(key) : null;
                if (v == null)
                {
                    CacheValue[key] = def;
                    return def;
                }
                else
                {
                    string? val = v.ToString();
                    CacheValue[key] = val;
                    return val;
                }
            }
            catch
            {
                CacheValue[key] = def;
                return def;
            }
            finally
            {
                rk2.Close();
            }
        }

        public static string EnableBiYingKey = "enable-biying";
        public static string ChangeBiYingBackgroundKey = "change-biying-background";
        public static string WindowBindOrChangeKey = "window-bind-change";
        public static string ForceWindowBindOrChangeKey = "force-window-bind-change";
        public static string UnWindowBindOrChangeKey = "un-window-bind-change";
        public static string ErrorModeKey = "error-mode";
        public static string EnableGoodbyeModeKey = "enable-goodbye";
        public static string EnableGoodbyeMKey = "enable-goodbye-m";
        public static string EnableGoodbyeHKey = "enable-goodbye-h";
        public static string EnableMouseGoodbyeModeKey = "enable-mouse-goodbye";
        public static string EnableDisableLockScreenKey = "enable-disable-lock-screen";
        public static string ChangeEnableDisableLockScreenKey = "global-change-enable-disable-lock-screen";
        public static string EnableViewHeartbeatKey = "enable-view-heartbeat";
        public static string OpacityValueKey = "opacity-value";
        public static string GoodbyeModeTypeKey = "goodbye-mode-type";
        public static string RandomGoodbyeModeThemeKey = "random-goodbye-mode-theme";
        public static string GlobalThemeKey = "global-theme";
        public static string EnableGameTimeKey = "enable-game-time";
        public static string FlowModeKey = "flow-mode";
        public static string HiddenTimeWindowKey = "hidden-time-window";
        public static string DisablePrintScreenKey = "enable-disable-print-screen";
        public static string ChangeDisablePrintScreenStateKey = "change-disable-print-screen";

        public static bool HasFullScreen { get; internal set; }

        public static List<IntPtr> SelfPtr = new List<IntPtr>();
    }
}
