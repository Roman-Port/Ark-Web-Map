﻿using ArkBridgeSharedEntities.Entities;
using ArkWebMapGateway.Clients;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapGateway
{
    public static class GatewayHttpHandler
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Handle
            Console.WriteLine("Incoming request: " + e.Request.Path);
            try
            {
                //Try and find the endpoint version
                string[] split = e.Request.Path.ToString().Split('/');
                if (split.Length < 3)
                {
                    await Program.QuickWriteToDoc(e, "Invalid path structure.", "text/plain", 400);
                    return;
                }
                string version = split[1];
                string endpoint = split[2];
                if (version != "v1")
                {
                    await Program.QuickWriteToDoc(e, "Unsupported API version.", "text/plain", 404);
                }

                //If this is a WebSocket request, upgrade it
                if (e.WebSockets.IsWebSocketRequest)
                {
                    //Use the endpoint to determine the client type.
                    if (endpoint == "master")
                        await MasterServerGatewayConnection.HandleIncomingConnection(e, version);
                    else if (endpoint == "subserver")
                        await SubServerGatewayConnection.HandleIncomingConnection(e, version);
                    else if (endpoint == "user")
                        await FrontendGatewayConnection.HandleIncomingConnection(e, version);
                    else if (endpoint == "notifications")
                        await NotificationConnection.HandleIncomingConnection(e, version);
                    else
                        await Program.QuickWriteToDoc(e, "Unknown endpoint.", "text/plain", 404);
                } else
                {
                    //Use the endpoint to determine type
                    if (endpoint == "send_notification")
                        await OnHttpRequestSendNotification(e);
                    else
                        await Program.QuickWriteToDoc(e, "WebSocket expected to the GATEWAY.", "text/plain", 429);
                }
            } catch (Exception ex)
            {
                Console.WriteLine("Error handling request at " + e.Request.Path + ": " + ex.Message + ex.StackTrace);
                try
                {
                    await Program.QuickWriteToDoc(e, "ERROR " + ex.Message + " AT " + ex.StackTrace, "text/plain", 500);
                }
                catch { }
            }
        }

        public static int latestNotificationId = 0;

        public static async Task OnHttpRequestSendNotification(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //TODO: VERIFY AND AUTHENTICATE SERVER
            string serverId = "x5wyzx9myzU3AKkdzlWHBzAt";

            //Deserialize POST and prepare message
            ArkV2NotificationRequest request = Program.DecodePostBody<ArkV2NotificationRequest>(e);
            if (request.payload == null)
                return;
            request.payload.uuid = latestNotificationId++;
            request.payload.serverName = ServerNameCache.GetServerName(serverId);
            string message = JsonConvert.SerializeObject(request.payload);

            int targetTribeId = request.targetTribeId;

            //Find clients to send to and send them the message
            var clients = ConnectionHolder.notificationClients.Where(x => x.serverIds.ContainsKey(serverId));
            foreach(var c in clients)
            {
                int clientTribeId = c.serverIds[serverId];
                if(clientTribeId == targetTribeId)
                {
                    c.SendMsg(message);
                }
            }

            //Send OK
            await Program.QuickWriteToDoc(e, "{\"ok\":true}", "application/json");
        }
    }
}