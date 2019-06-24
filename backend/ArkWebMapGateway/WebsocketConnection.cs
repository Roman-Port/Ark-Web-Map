﻿using ArkWebMapGatewayClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkWebMapGateway
{
    public delegate void OnWebsocketCreatedCallback();

    public abstract class WebsocketConnection
    {
        public abstract Task<bool> OnMsg(string msg);
        public abstract Task<bool> OnOpen(Microsoft.AspNetCore.Http.HttpContext e);
        public abstract Task<bool> OnClose(WebSocketCloseStatus? status);

        public WebSocket sock;
        public Task bgTask;

        public WebsocketConnection()
        {
            
        }

        public async Task Run(Microsoft.AspNetCore.Http.HttpContext e, OnWebsocketCreatedCallback readyCallback)
        {
            //Accept WebSocket
            WebSocket wc = await e.WebSockets.AcceptWebSocketAsync();
            sock = wc;
            bgTask = BgSender();
            readyCallback();
            try
            {
                byte[] buffer = new byte[1024 * 16];
                WebSocketReceiveResult result = await sock.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                OnOpen(e).GetAwaiter();
                try
                {
                    while (!result.CloseStatus.HasValue)
                    {
                        //Read buffer and call handler
                        string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        OnMsg(msg).GetAwaiter();

                        //Get next result
                        result = await sock.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("Shutting down connection because of an error: " + ex.Message + ex.StackTrace);
                }
                await sock.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                await OnClose(result.CloseStatus);
            } catch (Exception ex)
            {
                await OnClose(null);
            }
        }

        public async Task<bool> Close(byte[] reason)
        {
            await sock.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
            return true;
        }

        public void SendMsg(string msg)
        {
            lock (sendQueue)
                sendQueue.Enqueue(msg);
        }

        public void SendMsg(GatewayMessageBase msg)
        {
            SendMsg(JsonConvert.SerializeObject(msg));
        }

        private Queue<string> sendQueue = new Queue<string>();

        private async Task BgSender()
        {
            while(sock.State == WebSocketState.Open)
            {
                if(sendQueue.TryDequeue(out string msg))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(msg);
                    await sock.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                } else
                {
                    await Task.Delay(3);
                }
            }
        }
    }
}