﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Misc
{
    public static class ServerValidation
    {
        /// <summary>
        /// Validates a server and returns some information about it.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Get payload
            ServerValidationRequestPayload payload = Program.DecodePostBody<ServerValidationRequestPayload>(e);
            byte[] creds = Convert.FromBase64String(payload.server_creds);
            string id = payload.server_id;

            //Find servers matching these
            var server = ArkWebMapMasterServer.Servers.ArkSlaveServerSetup.GetCollection().FindOne(x => x._id == id);
            if(server != null)
            {
                if(CompareCreds(server.server_creds, creds))
                {
                    //Ok.
                    return Program.QuickWriteJsonToDoc<ServerValidationResponsePayload>(e, new ServerValidationResponsePayload
                    {
                        has_icon = server.has_custom_image,
                        icon_url = server.image_url,
                        server_id = server._id,
                        server_name = server.display_name,
                        server_owner_id = server.owner_uid
                    });
                }
            }
            return Program.QuickWriteJsonToDoc<ServerValidationResponsePayload>(e, null);
        }

        private static bool CompareCreds(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for(int i = 0; i<a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        class ServerValidationRequestPayload
        {
            public string server_id;
            public string server_creds; //Base 64 encoded creds for the server
        }

        class ServerValidationResponsePayload
        {
            public string server_id;
            public string server_name;
            public string server_owner_id;
            public bool has_icon;
            public string icon_url;
        }
    }
}