using DesktopTools.views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace DesktopTools.component
{
    public class GoodbyeModeComponent
    {
        private static List<Window> activeWindows = new List<Window>();
        public static bool IsInGoodbyeTime()
        {
            if (!GlobalEnable)
            {
                return false;
            }
            if (!"1".Equals(Setting.GetSetting(Setting.EnableGoodbyeModeKey)))
            {
                return false;
            }
            var h = int.Parse(Setting.GetSetting(Setting.EnableGoodbyeHKey));
            var m = int.Parse(Setting.GetSetting(Setting.EnableGoodbyeMKey));
            var type = Setting.GetSetting(Setting.GoodbyeModeTypeKey, "正常");
            if (type == "正常")
            {
#if DEBUG 
                if (DateTime.Now.Hour > h || (DateTime.Now.Hour == h && DateTime.Now.Minute >= m))
#else
                if (DateTime.Now.Hour == h && DateTime.Now.Minute == m)
#endif
                {
                    return true;
                }
            }
            else
            {
                if (DateTime.Now.Hour > h || (DateTime.Now.Hour == h && DateTime.Now.Minute >= m))
                {
                    return true;
                }
            }

            return false;
        }
        private static bool Stop = false;
        private static bool _GlobalEnable = true;

        public static bool GlobalEnable
        {
            get { return _GlobalEnable; }
            set
            {
                _GlobalEnable = value;
                if (!value)
                {
                    foreach (var item in new List<Window>(activeWindows))
                    {
                        try { item.Close(); } catch { }
                    }
                    activeWindows.Clear();
                }
            }
        }

        public static void Show()
        {
            var type = Setting.GetSetting(Setting.GoodbyeModeTypeKey, "正常");
            if (type == "遛弯")
            {
                if (Stop)
                {
                    return;
                }
                Stop = true; ;
                GoodbyeMode gm = new GoodbyeMode();
                gm.Show();
                gm.Closed += (a, e) =>
                {
                    activeWindows.Remove(gm);
                    Stop = false;
                };
                activeWindows.Add(gm);
            }
            else if (type == "炸街")
            {
                for (int i = 0; i < 3; i++)
                {
                    var d = new GoodbyeMode(i);
                    d.Closed += (a, e) =>
                    {
                        activeWindows.Remove(d);
                        ;
                    };
                    d.Show();
                    activeWindows.Add(d);
                }
            }
            else
            {
                if (Stop)
                {
                    return;
                }
                Stop = true; ;
                GoodbyeMode2 gm = new GoodbyeMode2();
                gm.Show();
                gm.Closed += (a, e) =>
                {
                    activeWindows.Remove(gm);
                    Stop = false;
                };
                activeWindows.Add(gm);

            }
        }
    }
}
