﻿using ArkWebMapMasterServer.ServiceTemplates;
using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class RenameDinoRequest : MasterArkRpcService
    {
        public RenameDinoRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<RpcCommand?> BuildArkRpcEvent()
        {
            //TODO: Check permissions

            //Parse
            RequestData request = await ReadPOSTContentChecked<RequestData>();
            if (request == null)
                return null;

            //Parse dino ID
            if (!ulong.TryParse(request.dino_id, out ulong dinoId))
            {
                await WriteString("Dino ID is invalid.", "text/plain", 400);
                return null;
            }

            //Unzipper ID
            DbDino.UnzipperDinoId(dinoId, out uint id1, out uint id2);

            //Create command
            var command = new CommandData
            {
                dino_id_1 = id1,
                dino_id_2 = id2,
                new_name = request.new_name
            };

            return new RpcCommand
            {
                opcode = 4,
                payload = command,
                persist = false
            };
        }

        class RequestData
        {
            public string dino_id;
            public string new_name;
        }

        class CommandData
        {
            public long dino_id_1; //actually a uint
            public long dino_id_2; //actually a uint
            public string new_name;
        }
    }
}
