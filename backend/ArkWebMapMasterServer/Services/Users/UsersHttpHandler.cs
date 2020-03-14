﻿
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services.Users
{
    public class UsersHttpHandler
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string path)
        {
            //Get method
            var method = Program.FindRequestMethod(e);

            //Every path here requies authentication. Do it.
            string userToken = ApiTools.GetBearerToken(e);
            DbUser user = await ApiTools.AuthenticateUser(userToken, true);

            //Check path
            if (path == "@me/report_issue")
                await IssueCreator.OnHttpRequest(e, user);
            else if (path == "@me/tokens/@this/devalidate")
                await TokenDevalidateService.OnSingleDevalidate(e, user, userToken);
            else if (path == "@me/servers")
                await ServerListRequest.OnHttpRequest(e, user);
            else if (path == "@me/tokens/@all/devalidate")
                await TokenDevalidateService.OnAllDevalidate(e, user);
            else if (path == "@me/user_settings")
                await PutUserSettingsRequest.OnHttpRequest(e, user);
            else if (path == "@me/archive")
                await UserDataDownloader.OnCreateRequest(e, user, userToken);
            else if (path == "@me/delete")
                await UserDataRemover.OnHttpRequest(e, user, userToken);
            else if (path == "@me/push_token" && method == RequestHttpMethod.post)
                await NotificationTokenRequest.OnHttpRequest(e, user);
            else if (path == "@me/applications/")
                await OAuthApplicationsHandler.OnBaseHTTPRequest(e, user);
            else if (path.StartsWith("@me/applications/"))
                await OAuthApplicationsHandler.OnAppHTTPRequest(e, user, path.Substring("@me/applications/".Length));
            else if (path == "@me/clusters")
                await UserClustersRequest.OnHttpRequest(e, user);
            else if (path == "@me/" || path == "@me")
                await UsersMeHandler.OnHttpRequest(e, user);
            else
                throw new StandardError("Not Found", StandardErrorCode.NotFound);
        }
    }
}
