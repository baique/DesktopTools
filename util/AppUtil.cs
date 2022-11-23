using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VirtualDesktopSwitch;
using static DesktopTools.util.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Color = System.Drawing.Color;
using Window = System.Windows.Window;

namespace DesktopTools.util
{
    public class AppUtil
    {
        public static void DisableAltF4(Window win)
        {
            win.KeyDown += (a, e) =>
            {
                Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
                if (key == Key.F4 && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                {
                    e.Handled = true;
                }
            };
            win.KeyUp += (a, e) =>
            {
                Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
                if (key == Key.F4 && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                {
                    e.Handled = true;
                }
            };
        }

        public static ImageSource GetAppIcon(int pid, Process process)
        {
            string? fn = "";
            try
            {
                if (process.MainModule != null)
                {
                    fn = process.MainModule.FileName;
                }
                if (fn == null)
                {
                    throw new Exception();
                }
            }
            catch
            {
                fn = getModuleFilePath(pid);
            }

            if (fn == null)
            {
                throw new Exception("进程模块加载异常");
            }
            var icon = Icon.ExtractAssociatedIcon(fn);
            if (icon == null)
            {
                throw new Exception("进程图标加载异常");
            }
            return ToImageSource(icon);
        }
        private static string getModuleFilePath(int processId)
        {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (var results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }
        public static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }
        /// <summary>
        /// 下载文件到字节数组
        /// </summary>
        /// <param name="url">地址</param>
        /// <returns>数据</returns>
        public static byte[] DownloadFileToByteArray(string url)
        {
            using (var client = new HttpClient())
            {
                var tk = client.GetByteArrayAsync(url);
                return tk.Result;
            }
        }
        /// <summary>
        /// byte[]转换成Image
        /// </summary>
        /// <param name="byteArrayIn">二进制图片流</param>
        /// <returns>Image</returns>
        public static System.Drawing.Image ByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null)
                return null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(byteArrayIn))
            {
                System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
                ms.Flush();
                return returnImage;
            }
        }

        private void createTextOnTop()
        {
            using (Graphics g = Graphics.FromHdc(Win32.GetDC(IntPtr.Zero)))
            {
                var s = Application.GetResourceStream(new Uri("pack://application:,,,/font/Aa破竹体.TTF"));
                try
                {

                    byte[] buf = new byte[1024 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int rawReadLength = 0;
                        while ((rawReadLength = s.Stream.Read(buf, 0, buf.Length)) > 0)
                        {
                            ms.Write(buf, 0, rawReadLength);
                        }
                        g.DrawString("下班！", GetResoruceFont(ms.ToArray(), 538), new SolidBrush(Color.Black), new PointF(400, 300));
                    }
                }
                finally
                {
                    s.Stream.Close();
                }
            }
        }

        private Bitmap createText(Image img)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                var s = Application.GetResourceStream(new Uri("pack://application:,,,/font/Aa破竹体.TTF"));
                try
                {
                    byte[] buf = new byte[1024 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int rawReadLength = 0;
                        while ((rawReadLength = s.Stream.Read(buf, 0, buf.Length)) > 0)
                        {
                            ms.Write(buf, 0, rawReadLength);
                        }
                        g.DrawImage(img, 0, 0, img.Width, img.Height);
                        g.DrawString("下班！", GetResoruceFont(ms.ToArray(), 328), new SolidBrush(Color.Black), new PointF(150, 150));
                    }
                }
                finally
                {
                    s.Stream.Close();
                }
            }

            return bmp;

        }

        public Font GetResoruceFont(byte[] bytes, int size)
        {
            System.Drawing.Text.PrivateFontCollection pfc = new System.Drawing.Text.PrivateFontCollection();
            IntPtr MeAdd = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, MeAdd, bytes.Length);
            pfc.AddMemoryFont(MeAdd, bytes.Length);
            return new Font(pfc.Families[0], size);
        }

        public static VirtualDesktopManager VDM = new VirtualDesktopManager();

        public static void AlwaysToTop(Window view, bool forceTop = false)
        {
            var ptr = GetHwnd(view);
            double viewRawTop = -20000;
            Win32.SetWindowPos(ptr, -1, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0004 | 0x0020 | 0x0040);
            //Hide on other window full screen
            Task.Run(async () =>
            {
                for (; ; )
                {
                    await Task.Delay(10);
                    if (forceTop)
                    {
                        Win32.SetWindowPos(ptr, -1, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0004 | 0x0020 | 0x0040);
                        continue;
                    }
                    if (SettingUtil.HasFullScreen && viewRawTop == -20000)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            viewRawTop = view.Top;
                            view.Top = -20000;
                        });
                        continue;
                    }
                    else if (!SettingUtil.HasFullScreen && viewRawTop != -20000)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            view.Top = viewRawTop;
                            viewRawTop = -20000;
                        });
                    }
                    if (!VDM.IsWindowOnCurrentVirtualDesktop(ptr))
                    {
                        VDM.MoveWindowToDesktop(ptr, VDM.GetWindowDesktopId(ptr));
                    }
                }
            });
        }

        public static void HideAltTab(Window window)
        {
            Win32.HideAltTab(GetHwnd(window));
        }

        public static void MonitorFromCapture(Window window)
        {
            SetWindowDisplayAffinity(GetHwnd(window), DisplayAffinity.Monitor);
        }

        public static void ExcludeFromCapture(Window window)
        {
            SetWindowDisplayAffinity(GetHwnd(window), DisplayAffinity.ExcludeFromCapture);
        }

        internal static IntPtr GetHwnd(Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }
    }
}
