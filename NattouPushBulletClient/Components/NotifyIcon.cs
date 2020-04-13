using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NattouPushBulletClient.Components
{
    public partial class NotifyIcon : Component
    {
        public event EventHandler RunMenuItemClick;
        public event EventHandler StopMenuItemClick;
        public event EventHandler ResetMenuItemClick;
        public event EventHandler ExitMenuItemClick;

        private bool isRunning;
        public bool IsRunning 
        {
            set
            {
                this.isRunning = value;
                var appName = Assembly.GetExecutingAssembly().GetName().Name;
                this.notifyIcon1.Icon = value ? Properties.Resources.Icon_run : Properties.Resources.Icon_stop;
                this.notifyIcon1.Text = appName + " - " + (value ? "Running" : "Stop");
                this.RunStopMenuItem.Text = value ? "Stop" : "Run";
            }
            get
            {
                return this.isRunning;
            }
        }

        public NotifyIcon()
        {
            InitializeComponent();
            this.IsRunning = false;
            this.notifyIcon1.Icon = Properties.Resources.Icon_stop;
            RunStopMenuItem.Click += delegate (object sender, EventArgs e)
            {
                if (this.isRunning)
                    this.StopMenuItemClick.Invoke(sender, e);
                else
                    this.RunMenuItemClick.Invoke(sender, e);
            };
            SettingMenuItem.Click += delegate (object sender, EventArgs e)
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Show();
            };
            ResetMenuItem.Click += delegate (object sender, EventArgs e)
            {
                this.ResetMenuItemClick.Invoke(sender, e);
            };
            ExitMenuItem.Click += delegate (object sender, EventArgs e)
            {
                this.ExitMenuItemClick.Invoke(sender, e);
            };
        }

        public NotifyIcon(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
