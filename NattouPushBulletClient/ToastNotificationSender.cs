using Microsoft.Toolkit.Uwp.Notifications;
using NattouPushBulletClient.PushBulletEphemerals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Windows.ApplicationModel.Activation;
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
		/// Send ToastNorification when needed.
		/// </summary>
		/// <param name="str">raw string from PushBullet API</param>
		/// <returns></returns>
		public void SendToastNotification(string str)
		{
			try
			{
				var mes = JsonConvert.DeserializeObject<Ephemeral>(str);
				Debug.WriteLine("----- Mes -----");
				Debug.WriteLine(mes.ToString());
				Debug.WriteLine("----------------");

				if (mes.Type.Equals("push"))
				{
					var tmp = JsonConvert.DeserializeObject<Ephemeral>(mes.Message.ToString());
					switch (tmp.Type)
					{
						case "messaging_extension_reply":
							break;
						case "mirror":
							var mirrorMes = JsonConvert.DeserializeObject<MirrorEphemeral>(mes.Message.ToString());
							Debug.WriteLine("----- Mirror -----");
							Debug.WriteLine(mirrorMes.ToString());
							Debug.WriteLine("------------------");
							
							// toast生成
							var toast = CreateToastNotification(mirrorMes.Title, mirrorMes.Body, mirrorMes.Icon, mirrorMes.ApplicationName);
							toast.Tag = mirrorMes.Id;
							toast.Group = mirrorMes.ApplicationName;
							this.groupNamePair.Add(toast.Tag, mirrorMes.ApplicationName);
							ToastNotificationManager.CreateToastNotifier(this.appId).Show(toast);
							break;
						case "dismissal":
							var dismissalMes = JsonConvert.DeserializeObject<DismissalEphemeral>(mes.Message.ToString());
							Debug.WriteLine("----- Dismiss -----");
							Debug.WriteLine(dismissalMes.ToString());
							Debug.WriteLine("-------------------");

							// 確認済みの通知をアクションセンターから削除
							RemoveToastNotification(dismissalMes.Id);
							break;
						case "clip":
							break;
						default:
							Debug.WriteLine($"Undefined Message: {tmp.Type}");
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		public void SendInformationToastNotification(string id, string body, DateTimeOffset duration)
		{
			var toast = CreateToastNotification("業務連絡", body, Assembly.GetExecutingAssembly().GetName().Name);
			toast.ExpirationTime = duration;
			toast.Tag = id;
			toast.Group = "Information";
			this.groupNamePair.Add(id, toast.Group);
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
						Attribution = new ToastGenericAttributionText()
						{
							Text = applicationName
						}
					}
				}
			};

			var xml = new XmlDocument();
			xml.LoadXml(toastContent.GetContent());
			return new ToastNotification(xml);
		}

		private ToastNotification CreateToastNotification(string title, string body, string iconSource, string applicationName)
		{
			// decode icon (64bit jpeg)
			var b = Convert.FromBase64String(iconSource);

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
							Source = GetIconPathFromCache(applicationName, b),
							HintCrop = ToastGenericAppLogoCrop.Circle //画像を丸くトリミング
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
