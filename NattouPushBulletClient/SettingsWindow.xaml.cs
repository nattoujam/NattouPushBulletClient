using System;
using System.ComponentModel;
using System.Windows;

namespace NattouPushBulletClient
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.AccessTokenTextBox.Text = Properties.Settings.Default.AccessToken;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Console.WriteLine(e.Cancel);
            Properties.Settings.Default.AccessToken = this.AccessTokenTextBox.Text;
			Properties.Settings.Default.Save();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
