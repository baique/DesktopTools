using System;
using System.Timers;

namespace DesktopTools.util
{
    public class Debounce
    {
        public Action work { get; set; }
        public int timeout = 200;
        private Timer? timer;
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
                    timer.Elapsed += (a, e) =>
                    {
                        timer.Stop();
                        timer.Close();
                        timer = null;
                        work();
                    };
                }
            }
            timer.Stop();
            timer.Start();
        }
    }
}
