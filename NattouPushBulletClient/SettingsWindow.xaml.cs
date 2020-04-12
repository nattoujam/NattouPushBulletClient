using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
