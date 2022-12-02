using BeanFramework.core.bean;
using DesktopTools.component.impl;
using System;
using Timer = System.Timers.Timer;

namespace DesktopTools.component
{
    [Bean(Name = "自动切换壁纸")]
    internal class AutoChangeBackground : Component
    {
        private Timer? autoChangeBackgroundTimer;
        private Timer? checkTimer;

        public void Destroy()
        {
        }

        public void Init()
        {
            RegisterAutoChangeBackground();
        }

        #region 自动切换壁纸
        private void RegisterAutoChangeBackground()
        {
            autoChangeBackgroundTimer = new Timer(120000);
            autoChangeBackgroundTimer.AutoReset = true;
            autoChangeBackgroundTimer.Elapsed += (a, e) =>
            {
                if (DateTime.Now.Subtract(SystemBackground.getLastChangeBackgroundTime()).TotalMinutes >= 60)
                    SystemBackground.ChangeBackground();
            };
            autoChangeBackgroundTimer.Start();

            checkTimer = new Timer(10000);
            checkTimer.AutoReset = true;
            checkTimer.Elapsed += (a, e) => SystemBackground.ChangeBackgroundIfModify();
            checkTimer.Start();
        }
        #endregion
    }
}
