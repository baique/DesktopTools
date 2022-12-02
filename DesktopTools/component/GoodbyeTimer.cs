using BeanFramework.core.bean;
using DesktopTools.component.impl;
using System.Timers;

namespace DesktopTools.component
{
    [Bean(Name = "挥手模式")]
    public class GoodbyeTimer : Component
    {
        private Timer? timer;

        public void Destroy()
        {
        }

        public void Init()
        {
            RegisterGoodbyeMode();
        }

        #region 挥手模式
        private void RegisterGoodbyeMode()
        {
            timer = new Timer();
            timer.Interval = 5000;
            timer.AutoReset = true;
            timer.Elapsed += (a, e) =>
            {
                if (GoodbyeModeComponent.IsInGoodbyeTime())
                    GoodbyeModeComponent.Show();
            };
            timer.Start();
        }
        #endregion
    }
}
