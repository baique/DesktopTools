using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;

namespace DesktopTools.component.model
{
    public class WindowInfo
    {
        public string Title { get; set; }
        public IntPtr Ptr { get; set; }
        public Process P { get; set; }
        public int Pid { get; set; }
        public ImageSource Icon { get; set; }
        public List<IntPtr> Hook { get; } = new List<IntPtr>();

        public WindowInfo(string title, IntPtr ptr, int pid, Process p, ImageSource icon)
        {
            Title = title;
            Pid = pid;
            Ptr = ptr;
            P = p;
            Icon = icon;
        }

        public override bool Equals(object? obj)
        {
            return obj is WindowInfo info &&
                   EqualityComparer<IntPtr>.Default.Equals(Ptr, info.Ptr);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Ptr);
        }
    }
}
