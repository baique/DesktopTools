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
            public void Handler(KeyEventArgs e);
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

        private void hook_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!GlobalKeybordEventStatus)
            {
                return;
            }
            foreach (var item in events)
            {
                if (CheckKeyDown(item.Key(), e))
                {
                    item.Handler(e);
                    break;
                }
            }
        }

        private bool CheckKeyDown(string v, KeyEventArgs eventArgs)
        {
            var keys = v.Split(" + ");
            foreach (var k in keys)
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
                }
                if (eventArgs.KeyCode.ToString().Equals(key.ToString()) || Keyboard.IsKeyDown(key))
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        public static void Register(Func<string> keyPath, Action<KeyEventArgs> action)
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
            private Action<KeyEventArgs> action;

            public DefaultEvent(Func<string> keyPath, Action<KeyEventArgs> action)
            {
                this.keyPath = keyPath;
                this.action = action;
            }

            public void Handler(KeyEventArgs e)
            {
                this.action.Invoke(e);
            }


            public string Key()
            {
                return keyPath.Invoke();
            }
        }
    }
}
