//using DesktopTools.component.impl;
//using DesktopTools.component.support;
//using System.Timers;

//namespace DesktopTools.component
//{
//    public class GoodbyeTimer : EventTrigger<MainWindow, bool>
//    {
//        private Timer? timer;
//        public bool Trigger(MainWindow? window)
//        {
//            RegisterGoodbyeMode();
//            return true;
//        }

//        #region 挥手模式
//        private void RegisterGoodbyeMode()
//        {
//            timer = new Timer();
//            timer.Interval = 5000;
//            timer.AutoReset = true;
//            timer.Elapsed += (a, e) => { 
//                if (GoodbyeModeComponent.IsInGoodbyeTime()) 
//                    GoodbyeModeComponent.Show(); 
//            };
//            timer.Start();
//        }
//        #endregion
//    }
//}
