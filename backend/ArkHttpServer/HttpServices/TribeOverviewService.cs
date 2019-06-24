﻿using ArkBridgeSharedEntities.Entities;
using ArkBridgeSharedEntities.Requests;
using ArkHttpServer.Entities;
using ArkHttpServer.NetEntities.TribeOverview;
using ArkSaveEditor.ArkEntries;
using ArkSaveEditor.World;
using ArkWebMapLightspeedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkHttpServer.HttpServices
{
    public class TribeOverviewService
    {
        public static async Task OnHttpRequest(LightspeedRequest e, ArkWorld world, int tribeId)
        {
            //Generate reply
            TribeOverviewReply reply = GenerateReply(world, tribeId);

            //Serialize and send
            await e.DoRespondJson(reply);
        }

        public static TribeOverviewReply GenerateReply(ArkWorld world, int tribeId)
        {
            //Create object
            TribeOverviewReply reply = new TribeOverviewReply();
            reply.tribemates = new List<TribeOverviewPlayer>();
            reply.dinos = new List<TribeOverviewDino>();
            reply.baby_dinos = new List<ArkDinoReply>();
            reply.tribeName = world.tribes.Where(x => x.tribeId == tribeId).First().tribeName;

            //First, grab all of the tribe players and convert them
            var playerProfiles = world.players.Where(x => x.tribeId == tribeId);
            foreach (var p in playerProfiles)
            {
                reply.tribemates.Add(new TribeOverviewPlayer
                {
                    arkName = p.playerName,
                    arkId = p.arkPlayerId.ToString(),
                    steamId = p.steamPlayerId
                });
            }

            //Start the conversion of the Ark profiles.
            Task<List<TribeOverviewPlayer>> playerProfileLookupTask = MassFetchPlayerData(reply.tribemates);

            //Convert all tribe dinos
            var tribeDinos = world.dinos.Where(x => x.tribeId == tribeId);
            foreach (var t in tribeDinos)
            {
                ArkDinoEntry ent = t.dino_entry;
                if (ent == null)
                    continue;
                TribeOverviewDino d = new TribeOverviewDino
                {
                    classDisplayName = ent.screen_name,
                    displayName = t.tamedName,
                    id = t.dinosaurId.ToString(),
                    img = ent.icon_url,
                    level = t.level
                };
                reply.dinos.Add(d);
            }

            //Grab all baby dinos
            var babyDinos = world.dinos.Where(x => x.tribeId == tribeId && x.isBaby == true && x.babyAge < 1);
            foreach (var b in babyDinos)
                reply.baby_dinos.Add(new ArkDinoReply(b, world));

            //Wait until the player fetch data is finished
            reply.tribemates = playerProfileLookupTask.GetAwaiter().GetResult();

            return reply;
        }

        private static Task<List<TribeOverviewPlayer>> MassFetchPlayerData(List<TribeOverviewPlayer> players)
        {
            //Find IDs to download
            List<string> ids = new List<string>();
            foreach (var p in players)
                ids.Add(p.steamId);

            //Request new IDs
            List<SteamProfile> results = Tools.SteamIdLookup.MassFetchPlayerData(ids).GetAwaiter().GetResult();

            //Match this up with profiles
            foreach(var p in results)
            {
                foreach(var pp in players)
                {
                    if(pp.steamId == p.steamid)
                    {
                        pp.steamUrl = p.profileurl;
                        pp.steamName = p.personaname;
                        pp.img = p.avatarfull;
                    }
                }
            }

            //Return results
            return Task.FromResult(players);
        }
    }
}