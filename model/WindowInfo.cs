using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DesktopTools.model
{
    public class WindowInfo
    {
        public string Title { get; set; }
        public IntPtr Ptr { get; set; }
        public Process P { get; set; }
        public int Pid { get; set; }
        public IntPtr Hook { get; set; }

        public WindowInfo(IntPtr ptr) : this("", ptr, 0, null, IntPtr.Zero)
        {

        }

        public WindowInfo(string title, IntPtr ptr, int pid, Process p, IntPtr hook)
        {
            Title = title;
            Pid = pid;
            Ptr = ptr;
            P = p;
            Hook = hook;
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
