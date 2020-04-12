using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using NattouPushBulletClient.Components;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace NattouPushBulletClient
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
    {
		private readonly string APP_ID = "NattouBulletClient-Key";
		private readonly NotifyIcon notifyIcon;
		private readonly ToastNotificationSender sender;
		private readonly PushBulletMessageReceiver receiver;
		private string failedToConnectToastNotificationId = string.Empty;
		
		public App()
		{
			this.notifyIcon = new NotifyIcon();
			this.notifyIcon.RunMenuItemClick += NotifyIcon_RunMenuItemClick;
			this.notifyIcon.StopMenuItemClick += NotifyIcon_StopMenuItemClick;
			this.notifyIcon.ExitMenuItemClick += NotifyIcon_CloesMenuItemClick;
			this.sender = new ToastNotificationSender(this.APP_ID);
			this.receiver = new PushBulletMessageReceiver();
			this.receiver.FailedToConnectEventHander += Receiver_FailedToConnectEventHander;
			this.receiver.ReceiveCallBack = s =>
			{
				this.sender.SendToastNotification(s);
			};
		}

		private void NotifyIcon_RunMenuItemClick(object sender, EventArgs e)
		{
			Task.Run(() => StartMainTask());
		}

		private void NotifyIcon_StopMenuItemClick(object sender, EventArgs e)
		{
			this.receiver.Close();
			this.notifyIcon.IsRunning = false;
		}
		private void NotifyIcon_CloesMenuItemClick(object sender, EventArgs e)
		{
			this.notifyIcon.Dispose();
			this.receiver.Close();
			this.sender.SendInformationToastNotification("Information-ApplicationExit-ID", "アプリケーションを終了します。", DateTime.Now.AddSeconds(10));
			Current.Shutdown();
		}
		private void Receiver_FailedToConnectEventHander(object sender, EventArgs e)
		{
			this.failedToConnectToastNotificationId = "Information-FailedToConnect-ID";
			this.sender.RemoveToastNotification(this.failedToConnectToastNotificationId);
			this.sender.SendInformationToastNotification(this.failedToConnectToastNotificationId, "サーバーに接続できません。AccessTokenの設定やネットワークの設定を確認してください。", DateTime.Now.AddDays(1));
		}

		protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

			Task.Run(() => TryCreateShortcut()).Wait();

			Task.Run(() => StartMainTask());
        }

		private async Task StartMainTask()
		{
			var connectionEstablished = await this.receiver.ConnectAsync();

			if (connectionEstablished)
			{
				// 送信済みのコネクション確立失敗の通知を削除
				if (!this.failedToConnectToastNotificationId.Equals(string.Empty))
					this.sender.RemoveToastNotification(this.failedToConnectToastNotificationId);

				this.notifyIcon.IsRunning = true;

				Debug.WriteLine("start receiving");
				await this.receiver.StartReceiveLoopAsync();
				Debug.WriteLine("end receiving");
			}
		}

		private bool TryCreateShortcut()
		{
			var shortcutName = "NattouPushBulletClient";
			var shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\" + shortcutName + ".lnk";
			Debug.WriteLine(shortcutPath);
			if (!File.Exists(shortcutPath))
			{
				InstallShortcut(shortcutPath);
				return true;
			}
			return false;
		}


		private void InstallShortcut(string shortcutPath)
		{
			// Find the path to the current executable
			var exePath = Process.GetCurrentProcess().MainModule.FileName;
			IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

			// Create a shortcut to the exe
			ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
			ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));

			// Open the shortcut property store, set the AppUserModelId property
			IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

			using (PropVariant appId = new PropVariant(APP_ID))
			{
				ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
				ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
			}

			// Commit the shortcut to disk
			IPersistFile newShortcutSave = (IPersistFile)newShortcut;

			ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
		}
	}
}
