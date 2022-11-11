using DesktopTools.views;
using System;
using System.Windows.Forms;

namespace DesktopTools.component
{
    public class GoodbyeModeComponent
    {
        private static GoodbyeMode? gm;
        public static bool IsInGoodbyeTime()
        {
            if (!"1".Equals(Setting.GetSetting(Setting.EnableGoodbyeModeKey)))
            {
                gm = new GoodbyeMode();
                return false;
            }
            var h = int.Parse(Setting.GetSetting(Setting.EnableGoodbyeHKey));
            var m = int.Parse(Setting.GetSetting(Setting.EnableGoodbyeMKey));

            if (DateTime.Now.Hour > h + 1 || DateTime.Now.Hour >= h && DateTime.Now.Minute >= m)
            {
                return true;
            }
            gm = new GoodbyeMode();
            return false;
        }

        public static void Show()
        {
            if (gm == null)
            {
                gm = new GoodbyeMode();
                gm.Show();
            }
        }

        public void Handler(KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
