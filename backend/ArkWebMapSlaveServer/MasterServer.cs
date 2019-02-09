﻿using ArkBridgeSharedEntities.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ArkWebMapSlaveServer
{
    public static class MasterServer
    {
        public static T SendRequestToMaster<T>(string action, object request)
        {
            //Get full URL
            string fullURL = "https://ark.romanport.com/api/bridge/" + action;

            //Send the HTTP request with our special headers. We'll worry about authentication later.
            T reply;
            using(HttpClient client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(request));

                content.Headers.Add("X-Ark-Slave-Server-ID", ArkWebMapServer.config.auth.id);
                content.Headers.Add("X-Ark-Slave-Server-Validation", ArkWebMapServer.config.auth.id);

                var response = client.PostAsync(fullURL, content).GetAwaiter().GetResult();
                var replyString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                
                if(response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    //Parse error and throw it
                    throw JsonConvert.DeserializeObject<StandardError>(replyString);
                } else if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Continue normally
                    reply = JsonConvert.DeserializeObject<T>(replyString);
                } else
                {
                    //Unknown
                    throw new Exception("Server reply was not valid.");
                }
            }
            return reply;
        }
    }
}