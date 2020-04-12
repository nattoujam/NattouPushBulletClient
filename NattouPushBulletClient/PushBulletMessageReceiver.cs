﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NattouPushBulletClient
{
    class PushBulletMessageReceiver
    {
        public Action<string> ReceiveCallBack { set; private get; }
        public event EventHandler FailedToConnectEventHander;

        private ClientWebSocket ws;
        private CancellationTokenSource connectTokenSource;
        private CancellationTokenSource receiveTokenSource;
        public async Task<bool> ConnectAsync()
        {
            ws = new ClientWebSocket();
            var accessToken = Properties.Settings.Default.AccessToken;
            var uri = new Uri("wss://stream.pushbullet.com/websocket/" + accessToken);
            try
            {
                using (this.connectTokenSource = new CancellationTokenSource())
                {
                    await ws.ConnectAsync(uri, this.connectTokenSource.Token);
                }
            }   
            catch(OperationCanceledException)
            {
                Debug.WriteLine("Cancel to connect");
                return false;
            }
            catch(WebSocketException)
            {
                Debug.WriteLine("Failed to connect");
                this.FailedToConnectEventHander.Invoke(this, EventArgs.Empty);
                return false;

            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            Debug.WriteLine("Connect");
            return true;
        }

        public async Task StartReceiveLoopAsync()
        {
            Debug.WriteLine("run receive loop");
            try
            {
                using (this.receiveTokenSource = new CancellationTokenSource())
                {
                    while (!this.receiveTokenSource.IsCancellationRequested)
                    {
                        var mes = await ReceiveAsync(this.receiveTokenSource.Token);
                        if(mes.Length != 0)
                            this.ReceiveCallBack.Invoke(mes);
                    }
                }

            }
            catch(OperationCanceledException)
            {
                Debug.WriteLine("cancel receive loop");
            }
            Debug.WriteLine("stop receive loop");
        }

        public void Close()
        {
            if(this.receiveTokenSource != null && !this.receiveTokenSource.IsCancellationRequested)
                this.receiveTokenSource.Cancel();
            if(this.ws.State == WebSocketState.Open)
                this.ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Exiting Application", CancellationToken.None);
        }

        private async Task<string> ReceiveAsync(CancellationToken token)
        {

            Debug.WriteLine("Waiting.....");
            var buf = new byte[1024];
            var seg = new ArraySegment<byte>(buf);
            var r = await ws.ReceiveAsync(seg, token);
            Debug.WriteLine($"Receive: {r.MessageType}");
            if (r.MessageType == WebSocketMessageType.Text)
            {
                var sb = new StringBuilder();
                sb.Append(Encoding.UTF8.GetString(seg.ToArray(), 0, r.Count));
                //メッセージの最後まで取得
                while (!r.EndOfMessage)
                {
                    seg = new ArraySegment<byte>(buf);
                    r = await ws.ReceiveAsync(seg, token);
                    sb.Append(Encoding.UTF8.GetString(seg.ToArray(), 0, r.Count));
                }

                return sb.ToString();
            }
            else
                return string.Empty;
        }
    }
}