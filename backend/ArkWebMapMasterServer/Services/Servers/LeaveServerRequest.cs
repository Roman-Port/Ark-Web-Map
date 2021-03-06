﻿using ArkWebMapMasterServer.ServiceTemplates;
using LibDeltaSystem;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Servers
{
    public class LeaveServerRequest : MasterTribeServiceTemplate
    {
        public LeaveServerRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Confirm method
            if(GetMethod() != LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST)
            {
                await WriteString("Only POST expected!", "text/plain", 400);
                return;
            }

            //Get player profile
            await server.DeleteUserPlayerProfile(conn, user);

            //Remove user from admin list
            if(server.CheckIsUserAdmin(user))
            {
                await server.GetUpdateBuilder(conn)
                    .RemoveAdmin(user)
                    .Apply();
            }

            await WriteStatus(true);
        }
    }
}
