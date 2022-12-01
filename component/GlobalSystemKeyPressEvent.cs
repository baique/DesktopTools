using DesktopTools.component.support;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace DesktopTools.component
{
    public class GlobalSystemKeyPressEvent : GlobalEventTrigger<Key, bool>, EventTrigger<MainWindow, bool>, ResourceHook
    {
        /// <summary>
        /// 全局键盘钩子
        /// </summary>
        private static KeyboardHook k_hook = new KeyboardHook();
        /// <summary>
        /// 全局键盘事件是否响应
        /// </summary>
        public static bool GlobalKeybordEventStatus { get; set; } = true;

        private void hook_KeyPress(object done, KeyEventArgs e)
        {
            if (!GlobalKeybordEventStatus)
            {
                return;
            }
            if (Handler(e.Key))
            {
                ((Action)done)();
            }
        }


        public new void UnRegister()
        {
            base.UnRegister();
            k_hook.Stop();
        }


        public new void Register()
        {
            base.Register();
            k_hook.KeyDownEvent += new KeyEventHandler(hook_KeyPress);//钩住键按下
            k_hook.Start();//安装键盘钩子
        }
    }
}
