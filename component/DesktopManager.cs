using DesktopTools.views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DesktopTools.component.GlobalKeyboardEvent;

namespace DesktopTools.component
{
    public class DesktopManager : Event
    {
        [DllImport("user32.dll")]
        private static extern bool SwitchDesktop(IntPtr hDesktop);
        [DllImport("user32.dll")]
        private static extern IntPtr CreateDesktop(string lpszDesktop,
            IntPtr lpszDevice,
            IntPtr pDevmode,
            int dwFlags,
            uint dwDesiredAccess,
            IntPtr lpsa);
        enum DESKTOP_ACCESS : uint
        {
            DESKTOP_NONE = 0,
            DESKTOP_READOBJECTS = 0x0001,
            DESKTOP_CREATEWINDOW = 0x0002,
            DESKTOP_CREATEMENU = 0x0004,
            DESKTOP_HOOKCONTROL = 0x0008,
            DESKTOP_JOURNALRECORD = 0x0010,
            DESKTOP_JOURNALPLAYBACK = 0x0020,
            DESKTOP_ENUMERATE = 0x0040,
            DESKTOP_WRITEOBJECTS = 0x0080,
            DESKTOP_SWITCHDESKTOP = 0x0100,

            GENERIC_ALL = (0x0001 | 0x0002 | 0x0004 |
                            0x0008 | 0x0010 | 0x0020 |
                            0x0040 | 0x0080 | 0x0100),
        }
        public bool Handler(KeyEventArgs e)
        {
            SwitchDesktop(CreateDesktop("newdesktop", IntPtr.Zero, IntPtr.Zero, 0,
                0x0001 | 0x0002 | 0x0004 |
                            0x0008 | 0x0010 | 0x0020 |
                            0x0040 | 0x0080 | 0x0100
                , IntPtr.Zero));
            return true;
        }

        public string Key()
        {
            return Setting.GetSetting("create-new-desktop", "LeftAlt + W + N");
        }
    }
}
