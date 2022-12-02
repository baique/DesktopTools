using BeanFramework.core.bean;
using DesktopTools.component.support;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DesktopTools.component
{
    [Bean(Name = "全局键盘钩子")]
    public class GlobalSystemKeyPressEvent : Component
    {
        /// <summary>
        /// 全局键盘钩子
        /// </summary>
        private KeyboardHook k_hook = new KeyboardHook();

        [Import]
        public List<EventTrigger<Key, bool>> events { get; set; } = new List<EventTrigger<Key, bool>>();
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
            foreach (var item in events)
            {
                if (item.Match(e.Key))
                {
                    if (item.Trigger(e.Key))
                    {
                        ((Action)done)();
                        return;
                    }
                }
            }
        }


        public void Destroy()
        {
            k_hook.Stop();
        }


        public void Init()
        {
            k_hook.KeyDownEvent += new KeyEventHandler(hook_KeyPress);//钩住键按下
            k_hook.Start();//安装键盘钩子
        }
    }
}
