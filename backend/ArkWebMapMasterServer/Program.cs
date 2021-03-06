﻿using ArkWebMapMasterServer.ServiceDefinitions.Auth;
using ArkWebMapMasterServer.ServiceDefinitions.Auth.NewAuth;
using ArkWebMapMasterServer.ServiceDefinitions.Misc;
using ArkWebMapMasterServer.ServiceDefinitions.Servers;
using ArkWebMapMasterServer.ServiceDefinitions.Servers.Admin;
using ArkWebMapMasterServer.ServiceDefinitions.User;
using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.MiscNet;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer
{
    class Program
    {
        public static DeltaConnection connection;

        public const int APP_VERISON_MAJOR = 0;
        public const int APP_VERISON_MINOR = 14;

        static void Main(string[] args)
        {
            connection = DeltaConnection.InitDeltaManagedApp(args, DeltaCoreNetServerType.API_MASTER, APP_VERISON_MAJOR, APP_VERISON_MINOR);
            V2SetupServer().GetAwaiter().GetResult();
        }

        public static async Task V2SetupServer()
        {
            var server = new DeltaWebServer(connection, connection.GetUserPort(0));

            //Misc
            server.AddService(new MapListDefinition());
            server.AddService(new ServerCreateConfigDefinition());
            server.AddService(new FetchPlatformProfileDefinition());

            //Auth
            server.AddService(new ValidateBetaKeyDefinition());
            server.AddService(new NewAuthBeginDefinition());
            server.AddService(new NewAuthAuthorizeDefinition());
            server.AddService(new NewAuthAppValidateDefinition());

            //Server
            server.AddService(new PutUserPrefsDefinition());
            server.AddService(new PutDinoPrefsDefinition());
            server.AddService(new LeaveServerDefinition());
            server.AddService(new ServerTribesDefinition());
            server.AddService(new AdminServerPlayerListDefinition());
            server.AddService(new AdminServerDeleteDefinition());
            server.AddService(new AdminServerBanUserDefinition());
            server.AddService(new AdminServerSetIconDefinition());
            server.AddService(new AdminServerStatsDefinition());
            server.AddService(new ForceRemoteRefreshDefinition());
            server.AddService(new AdminServerTransferDinoOwnershipDefinition());
            server.AddService(new AdminServerTeleportDinoDefinition());
            server.AddService(new AdminServerUpdateDefinition());

            //User
            server.AddService(new UsersMeDefinition());
            server.AddService(new PutUserSettingsDefinition());
            server.AddService(new TokenDevalidateDefinition());
            server.AddService(new UserClustersDefinition());

            //Start
            await server.RunAsync();
        }
    }
}
