using System;
using System.Collections.Generic;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace DesktopTools.component
{
    public class GlobalKeyboardEvent
    {
        public interface Event
        {
            public string Key();
            public bool Handler(KeyEventArgs e);
        }
        /// <summary>
        /// 全局键盘钩子
        /// </summary>
        private static KeyboardHook k_hook = new KeyboardHook();
        /// <summary>
        /// 全局键盘事件是否响应
        /// </summary>
        public static bool GlobalKeybordEventStatus { get; set; } = true;
        /// <summary>
        /// 键盘事件监听
        /// </summary>
        private static List<Event> events = new List<Event>();

        public GlobalKeyboardEvent()
        {
            InitKeyWatch();
        }

        private void InitKeyWatch()
        {
            k_hook.KeyDownEvent += new System.Windows.Forms.KeyEventHandler(hook_KeyDown);//钩住键按下
            k_hook.Start();//安装键盘钩子
        }

        private void hook_KeyDown(object done, KeyEventArgs e)
        {
            if (!GlobalKeybordEventStatus)
            {
                return;
            }
            foreach (var item in events)
            {
                if (CheckKeyDown(item.Key(), e))
                {
                    if (item.Handler(e))
                    {
                        ((Action)done)();
                    }
                    return;
                }
            }
        }

        private bool CheckKeyDown(string v, KeyEventArgs eventArgs)
        {
            var keys = v.Split(" + ");
            foreach (var k in keys)
            {
                try
                {
                    Key key = (Key)Enum.Parse(typeof(Key), k);
                    if (key == Key.LeftCtrl || key == Key.RightCtrl)
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            continue;
                        }
                        return false;
                    }
                    if (key == Key.LeftShift || key == Key.RightShift)
                    {
                        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        {
                            continue;
                        }
                        return false;
                    }
                    if (key == Key.LeftAlt || key == Key.RightAlt)
                    {
                        if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                        {
                            continue;
                        }
                        return false;
                    }//`
                    if (key == Key.OemTilde && eventArgs.KeyCode == System.Windows.Forms.Keys.Oemtilde)
                    {
                        return true;
                    }
                    if (eventArgs.KeyCode.ToString().Equals(key.ToString()) || Keyboard.IsKeyDown(key))
                    {
                        continue;
                    }
                    return false;
                }
                catch (Exception e) { throw e; }

            }
            return true;
        }

        public static void Register(Func<string> keyPath, Func<KeyEventArgs, bool> action)
        {
            events.Add(new DefaultEvent(keyPath, action));
        }

        public static void Register(Event customEvent)
        {
            events.Add(customEvent);
        }

        public static void close()
        {
            k_hook.Stop();
        }

        private class DefaultEvent : Event
        {
            private Func<string> keyPath;
            private Func<KeyEventArgs, bool> action;

            public DefaultEvent(Func<string> keyPath, Func<KeyEventArgs, bool> action)
            {
                this.keyPath = keyPath;
                this.action = action;
            }

            public bool Handler(KeyEventArgs e)
            {
                return this.action.Invoke(e);

            }


            public string Key()
            {
                return keyPath.Invoke();
            }
        }
    }
}
