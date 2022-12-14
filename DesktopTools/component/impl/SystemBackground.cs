using DesktopTools.util;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DesktopTools.component.impl
{
    public class SystemBackground
    {
        public static string getImgPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "desktop_tools_generate_bg_img.bmp");
        }

        public static DateTime getLastChangeBackgroundTime()
        {
            try
            {
                return DateTime.ParseExact(SettingUtil.GetSetting("last-change-background-time"), "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            }
            catch
            {
                return new DateTime();
            }
        }

        public static async void ChangeBackground()
        {
            if (!IsEnableBiYing())
            {
                return;
            }
            SettingUtil.SetSetting("last-change-background-time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await Task.Run(() =>
            {
                try
                {
                    var imgData = AppUtil.DownloadFileToByteArray("http://bingw.jasonzeng.dev/?index=random");
                    using (var image = AppUtil.ByteArrayToImage(imgData))
                    {
                        using (Bitmap img = new Bitmap(image))
                        {
                            FileInfo f = new FileInfo(getImgPath());
                            var fn = f.FullName;
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                try
                                {
                                    img.Save(f.FullName);
                                }
                                catch
                                {
                                    try
                                    {
                                        var fn = f.FullName + ".tmp.bmp";
                                        img.Save(fn);
                                        Win32.SystemParametersInfo(0x0014, 0, fn, 0x2 | 0x1);
                                        File.Move(fn, f.FullName);
                                    }
                                    catch { }
                                }
                                using (RegistryKey myRegKey = Registry.CurrentUser.CreateSubKey("Control Panel//Desktop"))
                                {
                                    myRegKey.SetValue("TileWallpaper", "0");
                                    myRegKey.SetValue("WallpaperStyle", "2");
                                    myRegKey.SetValue("Wallpaper", f.FullName);
                                }
                                Win32.SystemParametersInfo(0x0014, 0, f.FullName, 0x2 | 0x1);
                            });

                        }
                    }

                }
                catch { }
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
            if (Equals(sb.ToString(), getImgPath()) || Equals(sb.ToString(), getImgPath() + ".tmp.bmp"))
            {
                return;
            }
            ChangeBackground();
        }

        public bool Handler(KeyEventArgs e)
        {
            ChangeBackground();
            return true;
        }

        private static bool IsEnableBiYing()
        {
            return "1".Equals(SettingUtil.GetSetting(SettingUtil.EnableBiYingKey));
        }

    }
}
