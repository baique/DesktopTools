using System;
using System.Timers;

namespace DesktopTools.util
{
    /// <summary>
    /// 对刷新频率进行一定的控制
    /// </summary>
    public class Debounce
    {
        public Action? work { get; set; }
        public int timeout = 200;
        private Timer? timer;
        public Debounce(int timeout)
        {
            this.timeout = timeout;
        }
        public Debounce(int timeout, Action work)
        {
            this.timeout = timeout;
            this.work = work;
        }
        public void invoke()
        {
            lock (this)
            {
                if (timer == null)
                {
                    timer = new Timer(this.timeout);
                    timer.AutoReset = false;
                    timer.Elapsed += (a, e) => work();
                }
                timer.Stop();
                timer.Start();
            }
        }
    }
}
