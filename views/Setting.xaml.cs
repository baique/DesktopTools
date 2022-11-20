using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using static DesktopTools.util.SettingUtil;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;
namespace DesktopTools.views
{
    /// <summary>
    /// SettingUtil.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            this.Width = 0;
            this.Height = 0;
            InitializeComponent();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch
            {

            }
        }

        private void WinLoaded(object sender, RoutedEventArgs e)
        {
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Left = SystemParameters.WorkArea.Width / 2 - this.Width / 2;
            this.Top = SystemParameters.WorkArea.Height / 2 - this.Height / 2;

            //全局主题
            this.GlobalTheme.SelectedIndex = int.Parse(GetSetting(GlobalThemeKey, "0"));
            //小娱乐
            this.EnableGameTime.IsChecked = "1".Equals(GetSetting(EnableGameTimeKey));
            //隐藏时间浮窗
            this.HiddenTimeWindow.IsChecked = "1".Equals(GetSetting(HiddenTimeWindowKey));
            //禁止自动锁屏
            this.EnableDisableLockScreen.IsChecked = "1".Equals(GetSetting(EnableDisableLockScreenKey, "1"));
            this.ChangeEnableDisableLockScreen.Text = GetSettingOrDefValueIfNotExists(ChangeEnableDisableLockScreenKey, "LeftCtrl + LeftAlt + Space");
            //必应壁纸
            this.EnableBiYingBackground.IsChecked = "1".Equals(GetSetting(EnableBiYingKey));
            this.ChangeBiYingBackground.Text = GetSettingOrDefValueIfNotExists(ChangeBiYingBackgroundKey, "LeftCtrl + LeftAlt + B + N");
            //窗体绑定/切换
            this.WindowBindOrChange.Text = GetSettingOrDefValueIfNotExists(WindowBindOrChangeKey, "LeftCtrl");
            //强制绑定
            this.ForceWindowBindOrChange.Text = GetSettingOrDefValueIfNotExists(ForceWindowBindOrChangeKey, "LeftCtrl + LeftAlt");
            //解除绑定
            this.UnWindowBindOrChange.Text = GetSettingOrDefValueIfNotExists(UnWindowBindOrChangeKey, "LeftCtrl + LeftAlt + Back");
            //紧急避险
            this.ErrorMode.Text = GetSettingOrDefValueIfNotExists(ErrorModeKey, "LeftCtrl + LeftShift + Space");
            //下班提醒
            this.EnableGoodbyeMode.IsChecked = "1".Equals(GetSetting(EnableGoodbyeModeKey));
            this.EnableGoodbyeH.Text = GetSetting(EnableGoodbyeHKey, "17");
            this.EnableGoodbyeM.Text = GetSetting(EnableGoodbyeMKey, "30");
            this.GoodbyeModeType.Text = GetSetting(GoodbyeModeTypeKey, "正常");
            this.RandomGoodbyeModeTheme.IsChecked = "1".Equals(GetSetting(RandomGoodbyeModeThemeKey));
            //透明度
            this.OpacityValue.Value = double.Parse(GetSetting(OpacityValueKey, "0.5"));
            //呼吸效果
            this.EnableViewHeartbeat.IsChecked = "1".Equals(GetSetting(EnableViewHeartbeatKey));
            //任务栏位置
            this.FlowMode.SelectedIndex = int.Parse(GetSetting(FlowModeKey, "0"));
        }

        private void SaveChange(object sender, RoutedEventArgs e)
        {
            //全局主题
            SetSetting(GlobalThemeKey, "" + this.GlobalTheme.SelectedIndex);
            //小娱乐
            SetSetting(EnableGameTimeKey, this.EnableGameTime.IsChecked.Value ? "1" : "0");
            SetSetting(HiddenTimeWindowKey, this.HiddenTimeWindow.IsChecked.Value ? "1" : "0");
            //禁止自动锁屏
            SetSetting(EnableDisableLockScreenKey, this.EnableDisableLockScreen.IsChecked.Value ? "1" : "0");
            SetSetting(ChangeEnableDisableLockScreenKey, this.ChangeEnableDisableLockScreen.Text);
            //必应壁纸
            SetSetting(EnableBiYingKey, this.EnableBiYingBackground.IsChecked.Value ? "1" : "0");
            SetSetting(ChangeBiYingBackgroundKey, this.ChangeBiYingBackground.Text);
            //窗体绑定/切换
            SetSetting(WindowBindOrChangeKey, this.WindowBindOrChange.Text);
            //强制窗体绑定
            SetSetting(ForceWindowBindOrChangeKey, this.ForceWindowBindOrChange.Text);
            //解除绑定
            SetSetting(UnWindowBindOrChangeKey, this.UnWindowBindOrChange.Text);
            //紧急避险
            SetSetting(ErrorModeKey, this.ErrorMode.Text);
            //下班提醒
            SetSetting(EnableGoodbyeModeKey, this.EnableGoodbyeMode.IsChecked.Value ? "1" : "0");
            SetSetting(EnableGoodbyeHKey, this.EnableGoodbyeH.Text);
            SetSetting(EnableGoodbyeMKey, this.EnableGoodbyeM.Text);
            SetSetting(GoodbyeModeTypeKey, this.GoodbyeModeType.Text);
            SetSetting(RandomGoodbyeModeThemeKey, this.RandomGoodbyeModeTheme.IsChecked.Value ? "1" : "0");
            //透明度设置
            SetSetting(OpacityValueKey, this.OpacityValue.Value + "");
            //呼吸效果
            SetSetting(EnableViewHeartbeatKey, this.EnableViewHeartbeat.IsChecked.Value ? "1" : "0");
            SetSetting(FlowModeKey, this.FlowMode.SelectedIndex + "");
            MainWindow.Notify.ShowBalloonTip(300, "修改成功", "新的配置已被应用", ToolTipIcon.Info);
            this.Close();
        }

        private void KeyBind(object sender, KeyEventArgs e)
        {
            List<Key> keys = new List<Key>();
            foreach (Key item in Enum.GetValues(typeof(Key)))
            {
                try
                {
                    if (Keyboard.IsKeyDown(item))
                    {
                        keys.Add(item);
                    }
                }
                catch
                {

                }
            }
            e.Handled = false;
            ((TextBox)sender).Text = String.Join(" + ", keys.Select(f => f.ToString()).DistinctBy(f => f).ToArray());
        }

        private void OpacityValueChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (Application.Current.Resources["mainBackColor"] as Brush).Opacity = this.OpacityValue.Value;
        }

        private void OnCloseReset(object sender, EventArgs e)
        {
            App.RefreshOpacityValue();
        }

        private void ChangeTheme(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ResourceDictionary resource = new ResourceDictionary();
            resource.Source = new Uri("pack://application:,,,/resource/ColorTheme" + this.GlobalTheme.SelectedIndex + ".xaml");
            Application.Current.Resources.MergedDictionaries[0] = resource;
            OpacityValueChange(null, null);
            this.OpacityValue.Minimum = 0.05;
            //try
            //{
            //    var minOv = Application.Current.TryFindResource("minOpacityValue");
            //    if (minOv != null)
            //    {
            //        this.OpacityValue.Minimum = (double)minOv;
            //        if (this.OpacityValue.Value < this.OpacityValue.Minimum)
            //        {
            //            this.OpacityValue.Value = this.OpacityValue.Minimum;
            //            OpacityValueChange(null, null);
            //        }
            //    }
            //}
            //catch { }
            //try
            //{
            //    var goodOv = Application.Current.TryFindResource("goodOpacityValue");
            //    if (goodOv != null)
            //    {
            //        this.OpacityValue.Value = (double)goodOv;
            //        OpacityValueChange(null, null);
            //    }
            //}
            //catch { }


        }

        private void UnbindKey(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)sender).Text = "";
        }
    }
}
