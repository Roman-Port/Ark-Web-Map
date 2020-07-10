﻿using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkWebMapMasterServer.NetEntities
{
    /// <summary>
    /// Returns the Ark server in a reply safe to show anyone.
    /// </summary>
    public class ArkServerReply
    {
        public string display_name;
        public string image_url;
        public string owner_uid;
        public string id;

        public ArkServerReply(DbServer s, DbUser u, int timeoutMs = 2000)
        {
            //Set the simple stuff
            display_name = s.display_name;
            image_url = s.image_url;
            owner_uid = s.owner_uid?.ToString();
            id = s.id;
        }
    }
}
