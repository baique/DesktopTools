using System;
using System.Windows.Input;

namespace DesktopTools.util
{
    public class KeyUtil
    {
        public static bool CheckKeyDown(string? v, Key eventArgs)
        {
            try
            {
                if (v == null || string.IsNullOrWhiteSpace(v)) return false;
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
                        }
                        if (eventArgs == key || Keyboard.IsKeyDown(key))
                        {
                            continue;
                        }
                        return false;
                    }
                    catch { continue; }

                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
