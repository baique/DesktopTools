using DesktopTools.component;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using VirtualDesktopSwitch;

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
        public static void AlwaysToTop(Window view)
        {
            var ptr = new WindowInteropHelper(view).Handle;
            double viewRawTop = -20000;
            Task.Run(async () =>
            {
                for (; ; )
                {
                    await Task.Delay(10);
                    if (ToggleWindow.HasFullScreen && viewRawTop == -20000)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            viewRawTop = view.Top;
                            view.Top = -20000;
                        });
                        continue;
                    }
                    else if (!ToggleWindow.HasFullScreen && viewRawTop != -20000)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            view.Top = viewRawTop;
                            viewRawTop = -20000;
                        });
                    }
                    if (!VDM.IsWindowOnCurrentVirtualDesktop(ptr))
                    {
                        VDM.MoveWindowToDesktop(ptr, VDM.GetWindowDesktopId(ptr));
                    }
                    Win32.SetWindowPos(ptr, -1, 0, 0, 0, 0, 3);
                }
            });
        }
    }
}
