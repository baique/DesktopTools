using DesktopTools.views;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopTools.component
{
    public class GoodbyeModeComponent
    {
        public static bool IsInGoodbyeTime()
        {
            if (!"1".Equals(Setting.GetSetting(Setting.EnableGoodbyeModeKey)))
            {
                return false;
            }
            var h = int.Parse(Setting.GetSetting(Setting.EnableGoodbyeHKey));
            var m = int.Parse(Setting.GetSetting(Setting.EnableGoodbyeMKey));

            if (DateTime.Now.Hour > h || (DateTime.Now.Hour == h && DateTime.Now.Minute >= m))
            {
                return true;
            }
            return false;
        }
        private static bool Stop = false;

        public static void Show()
        {
            if ("1".Equals(Setting.GetSetting(Setting.EnableMouseGoodbyeModeKey)))
            {
                for (int i = 0; i < 3; i++)
                {
                    new GoodbyeMode(i).Show();
                }
            }
            else
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
                    Stop = false;
                };
            }

        }
    }
}
