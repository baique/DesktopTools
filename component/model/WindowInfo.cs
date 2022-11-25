using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;

namespace DesktopTools.component.model
{
    public class WindowInfo : INotifyPropertyChanged
    {
        private string _title;
        private IntPtr _ptr;
        private Process _p;
        private int _pid;
        private ImageSource _icon;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value != _title)
                {
                    _title = value;
                }
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
                }
            }
        }
        public Process P {
            get { return _p; }
            set
            {
                if (value != _p)
                {
                    _p = value;
                }
            }
        }
        public int Pid {
            get { return _pid; }
            set
            {
                if (value != _pid)
                {
                    _pid = value;
                }
            }
        }
        public ImageSource Icon {
            get { return _icon; }
            set
            {
                if (value != _icon)
                {
                    _icon = value;
                }
            }
        }

        public WindowInfo(string title, IntPtr ptr, int pid, Process p, ImageSource icon)
        {
            Title = title;
            Pid = pid;
            Ptr = ptr;
            P = p;
            Icon = icon;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
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
