# NattouPushBulletClient
[PushBullet](https://www.pushbullet.com/)の通知ミラーリングによりAndroid端末などから送られた通知を，Windowsのトースト通知として表示します。

タスクバーの通知領域に常駐します。

# 使い方
## 初めに
1. 通知領域のアイコンを右クリックし，Settingsを選択します。
1. PushBulletで発行されるAccess Tokenを入力します。
1. 通知が送信されるのを待ちましょう。

## その他
### コンテキストメニュー
- **Run/Stop**: トースト通知の表示をON/OFFできます。
- **Reset**: 通知が来ない時などにアプリケーションをリセットします。
- **Exit**: アプリケーションを終了します。

# 注意
以下に対応していません。
- End-to-End Encryption
- SMS及びUniversal Copy/Pasten
- Chat
- WindowsからPushBulletへの通知の送信
- トースト通知のアクティベイト

# Libraries
- [Microsoft.Toolkit.Uwp.Notifications](https://github.com/windows-toolkit/WindowsCommunityToolkit) (v6.0.0)
- [Microsoft.windows.SDK.Contracts](https://www.nuget.org/packages/Microsoft.Windows.SDK.Contracts) (v10.0.18362.2005)
- Microsoft.WindowsAPICodePack-Shell (v.1.1.0)
- [Newtonsoft.Json](https://www.newtonsoft.com/json) (v12.0.3)
