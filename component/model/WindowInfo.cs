using DesktopTools.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Media;

namespace DesktopTools.component.model
{
    public class WindowInfo : INotifyPropertyChanged
    {
        private string _title;
        private IntPtr _ptr;
        private Process _p;
        private int _pid;
        private ImageSource? _icon;
        public DateTime CloseTime { get; set; }
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }
        public IntPtr Ptr
        {
            get { return _ptr; }
            set
            {
                if (value != _ptr)
                {
                    _ptr = value;
                    OnPropertyChanged("Ptr");
                }
            }
        }
        public Process P
        {
            get { return _p; }
            set
            {
                if (value != _p)
                {
                    _p = value;
                    OnPropertyChanged("P");
                }
            }
        }
        public int Pid
        {
            get { return _pid; }
            set
            {
                if (value != _pid)
                {
                    _pid = value;
                    OnPropertyChanged("Pid");
                }
            }
        }
        public ImageSource Icon
        {
            get { return _icon; }
            set
            {
                if (value != _icon)
                {
                    _icon = value;
                    OnPropertyChanged("Icon");
                }
            }
        }

        public WindowInfo(IntPtr ptr, int pid, Process p)
        {
            Pid = pid;
            Ptr = ptr;
            P = p;
            LoadTitle();
        }

        public void LoadTitle()
        {
            try
            {
                var title = Win32.GetText(_ptr);
                if (string.IsNullOrWhiteSpace(title)) if (_p.ProcessName != null) this.Title = _p.ProcessName;
                this.Title = title;
            }
            catch
            {
                this.Title = string.Empty;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object? obj)
        {
            return obj is WindowInfo info &&
                   EqualityComparer<IntPtr>.Default.Equals(Ptr, info.Ptr);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Ptr);
        }
    }
}
