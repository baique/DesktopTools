using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTools.model
{
    public class WindowInfo
    {
        public string Title { get; set; }
        public IntPtr Ptr { get; set; }
        public Process P { get; set; }
        public string StartFile { get; set; }

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
