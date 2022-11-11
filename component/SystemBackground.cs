﻿using DesktopTools.util;
using DesktopTools.views;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopTools.component
{
    public class SystemBackground : GlobalKeyboardEvent.Event
    {
        public static string getImgPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "desktop_tools_generate_bg_img.bmp");
        }
        public static async void ChangeBackground()
        {
            if (!IsEnableBiYing())
            {
                return;
            }
            await Task.Run(() =>
            {
                var imgData = AppUtil.DownloadFileToByteArray("https://bingw.jasonzeng.dev/?index=random");
                var image = AppUtil.ByteArrayToImage(imgData);
                //using (Bitmap img = IsInGoodbyeTime()? createText(image) : new Bitmap(image))
                using (Bitmap img = new Bitmap(image))
                {
                    FileInfo f = new FileInfo(getImgPath());
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        img.Save(f.FullName);
                        Win32.SystemParametersInfo(0x0014, 0, f.FullName, 2);
                    });
                }
            });
        }

        internal static void ChangeBackgroundIfModify()
        {
            if (!IsEnableBiYing())
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            Win32.SystemParametersInfo(0x0073, 65535, sb, 0);
            if (Path.Equals(sb.ToString(), getImgPath()))
            {
                return;
            }
            ChangeBackground();
        }

        public void Handler(KeyEventArgs e)
        {
            ChangeBackground();
        }

        private static bool IsEnableBiYing()
        {
            return "1".Equals(Setting.GetSetting(Setting.EnableBiYingKey));
        }

        public string Key()
        {
            return Setting.GetSetting(Setting.ChangeBiYingBackgroundKey, "LeftCtrl + LeftAlt + B + N");
        }

    }
}