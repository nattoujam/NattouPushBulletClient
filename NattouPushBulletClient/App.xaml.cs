﻿using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using NattouPushBulletClient.Components;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Windows.Devices.Lights.Effects;
using WinUIXaml = Windows.UI.Xaml;

namespace NattouPushBulletClient
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
    {
		private readonly string APP_ID = "NattouPushBulletClient-Key";
		private readonly NotifyIcon notifyIcon;
		private readonly ToastNotificationSender sender;
		private readonly PushBulletMessageReceiver receiver;
		private string failedToConnectToastNotificationId = string.Empty;

		public App()
		{
			this.notifyIcon = new NotifyIcon();
			this.notifyIcon.RunMenuItemClick += NotifyIcon_RunMenuItemClick;
			this.notifyIcon.StopMenuItemClick += NotifyIcon_StopMenuItemClick;
			this.notifyIcon.ResetMenuItemClick += NotifyIcon_ResetMenuItemClick;
			this.notifyIcon.ExitMenuItemClick += NotifyIcon_ExitMenuItemClick;
			this.sender = new ToastNotificationSender(this.APP_ID);
			this.receiver = new PushBulletMessageReceiver();
			this.receiver.FailedToConnectEventHander += Receiver_FailedToConnectEventHander;
			this.receiver.ReceiveMirrorEpemeral += Receiver_ReceiveMirrorEpemeral;
			this.receiver.ReceiveDismissalEphemeral += Receiver_ReceiveDismissalEphemeral;

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
		}
		
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
			Debug.WriteLine("start application");
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

			CreateShortcut();
			_ = StartMainTask();
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

		private string GetShortcutPath()
		{
			var shortcutName = Assembly.GetExecutingAssembly().GetName().Name;
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\" + shortcutName + ".lnk";
		}

		private void CreateShortcut()
		{
			var shortcutPath = GetShortcutPath();
			Debug.WriteLine(shortcutPath);
			if (!File.Exists(shortcutPath))
			{
				InstallShortcut(shortcutPath);
			}
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


		#region event

		private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			switch (e.Mode)
			{
				// スリープ
				case PowerModes.Suspend:
					Debug.WriteLine("suspend");
					NotifyIcon_StopMenuItemClick(this, EventArgs.Empty);
					break;
				// 再開
				case PowerModes.Resume:
					Debug.WriteLine("resume");
					_ = StartMainTask();
					break;
				case PowerModes.StatusChange:
					break;
			}
		}

		private void NotifyIcon_RunMenuItemClick(object sender, EventArgs e)
		{
			_ = StartMainTask();
		}
		private void NotifyIcon_StopMenuItemClick(object sender, EventArgs e)
		{
			this.receiver.Close();
			this.notifyIcon.IsRunning = false;
		}
		private void NotifyIcon_ResetMenuItemClick(object sender, EventArgs e)
		{
			var shortcutPath = GetShortcutPath();
			if (File.Exists(shortcutPath))
			{
				if (this.notifyIcon.IsRunning)
					NotifyIcon_StopMenuItemClick(this, EventArgs.Empty);

				// ショートカットを削除
				File.Delete(shortcutPath);

				// ショートカットを生成
				CreateShortcut();

				if (!this.notifyIcon.IsRunning)
					NotifyIcon_RunMenuItemClick(this, EventArgs.Empty);
			}
		}
		private void NotifyIcon_ExitMenuItemClick(object sender, EventArgs e)
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
		private void Receiver_ReceiveMirrorEpemeral(object sender, PushBulletEphemerals.MirrorEphemeral e)
		{
			// トースト通知発行
			this.sender.SendToastNotification(e);
		}
		private void Receiver_ReceiveDismissalEphemeral(object sender, PushBulletEphemerals.DismissalEphemeral e)
		{
			// 確認済みの通知をアクションセンターから削除
			this.sender.RemoveToastNotification(e.Id);
		}

        #endregion
    }
}
