﻿using ArkWebMapMasterServer.Services.Servers.Admin;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.ServiceDefinitions.Servers.Admin
{
    public class AdminServerTeleportDinoDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/servers/{SERVER}/admin/teleport_dino";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new AdminServerTeleportDinoRequest(conn, e);
        }
    }
}
