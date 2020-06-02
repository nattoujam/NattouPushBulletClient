using Microsoft.Toolkit.Uwp.Notifications;
using NattouPushBulletClient.PushBulletEphemerals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace NattouPushBulletClient
{
	class ToastNotificationSender
    {
		private readonly string appId;
        private readonly Dictionary<string, string> iconPathPair;
		private readonly Dictionary<string, string> groupNamePair;
        public ToastNotificationSender(string appId)
        {
			this.appId = appId;
            this.iconPathPair = new Dictionary<string, string>();
			this.groupNamePair = new Dictionary<string, string>();
        }

		/// <summary>
		/// Send mirroring notification as toast notification.
		/// </summary>
		/// <param name="me"></param>
		public void SendToastNotification(MirrorEphemeral me)
		{
			// decode icon (64bit jpeg)
			var b = Convert.FromBase64String(me.Icon);
			var iconPath = GetIconPathFromCache(me.ApplicationName, b);

			// toast生成
			var toast = CreateToastNotification(me.Title, me.Body, iconPath, me.ApplicationName, true);
			toast.Tag = me.Id;
			toast.Group = me.ApplicationName;
			this.groupNamePair.Add(toast.Tag, me.ApplicationName);

			// toast発行
			ToastNotificationManager.CreateToastNotifier(this.appId).Show(toast);
		}

		public void SendInformationToastNotification(string id, string body, DateTimeOffset duration)
		{
			// toast生成
			var toast = CreateToastNotification("業務連絡", body, Assembly.GetExecutingAssembly().GetName().Name);
			toast.ExpirationTime = duration;
			toast.Tag = id;
			toast.Group = "Information";
			this.groupNamePair.Add(id, toast.Group);

			// toast発行
			ToastNotificationManager.CreateToastNotifier(this.appId).Show(toast);
		}

		public void RemoveToastNotification(string id)
		{
			if (this.groupNamePair.ContainsKey(id))
			{
				ToastNotificationManager.History.Remove(id, this.groupNamePair[id], appId);
				this.groupNamePair.Remove(id);
			}
		}

		private string GetIconPathFromCache(string applicationName, byte[] b)
		{
			if (this.iconPathPair.ContainsKey(applicationName))
				return this.iconPathPair[applicationName];
			else
			{
				Debug.WriteLine("create new Icon");
				var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "\\IconCache";
				var iconPath = dir + "\\" + applicationName + ".jpeg";
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
				File.WriteAllBytes(iconPath, b);
				this.iconPathPair.Add(applicationName, iconPath);
				return iconPath;
			}
		}

		private ToastNotification CreateToastNotification(string title, string body, string applicationName)
		{
			return CreateToastNotification(title, body, Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "\\Resources\\Icon.ico", applicationName, false);
		}

		private ToastNotification CreateToastNotification(string title, string body, string iconPath, string applicationName, bool isCircleIcon)
		{
			// construct toast norification
			var toastContent = new ToastContent()
			{
				Launch = "empty",
				Visual = new ToastVisual()
				{
					BindingGeneric = new ToastBindingGeneric()
					{
						Children =
						{
							new AdaptiveText()
							{
								Text = title,
								HintMaxLines = 1 //行数を制限
							},
							new AdaptiveText()
							{
								Text = body
							}
						},
						AppLogoOverride = new ToastGenericAppLogo()
						{
							Source = iconPath,
							HintCrop = isCircleIcon ? ToastGenericAppLogoCrop.Circle : ToastGenericAppLogoCrop.Default //画像を丸くトリミング
						},
						Attribution = new ToastGenericAttributionText()
						{
							Text = applicationName
						}
					}
				}
			};

			var xml = new XmlDocument();
			xml.LoadXml(toastContent.GetContent());
			return  new ToastNotification(xml);
		}
	}
}
