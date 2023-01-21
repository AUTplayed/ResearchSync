using System.Collections.Generic;
using ResearchSync.Entity;
using ResearchSync.Util;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Type = ResearchSync.Entity.Type;

namespace ResearchSync
{
    internal class ResearchSyncPlayer : ModPlayer
    {
        public Dictionary<int, int> PrevResearch { get; set; }

        private readonly object syncLock = new object();

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                return;
            }
            Dictionary<int, int> researchDiff;
            lock (syncLock)
            {
                researchDiff = Research.GetDiff(this);
                if (researchDiff.Count > 0)
                {
                    PrevResearch = new Dictionary<int, int>(Research.GetCurrent(this));
                }
            }
            if (researchDiff.Count > 0)
            {
                var packet = new SyncPacket(Type.DIFF, Player.whoAmI, researchDiff);
                SendPacket(packet);
            }
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                return;
            }
            SyncPacket packet;
            lock (syncLock)
            {
                PrevResearch = new Dictionary<int, int>(Research.GetCurrent(this));
                packet = new SyncPacket(Type.FULL, -1, PrevResearch);   // needs to send to all when joining yourself, but only to single if other joins
            }
            SendPacket(packet);
        }

        public override void clientClone(ModPlayer clientClone)
        {
            // unused
        }

        private void SendPacket(SyncPacket syncPacket)
        {
            var packet = Mod.GetPacket();
            syncPacket.Serialize(packet);
            packet.Send();  // always send to server
        }

        public void UpdateResearch(SyncPacket syncPacket)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                return;
            }

            var playerName = Main.player[syncPacket.Player].name;
            if (syncPacket.Type == Type.FULL || syncPacket.Type == Type.FULL_RESPONSE)
            {
                Main.NewText($"Recieved full sync from {playerName}", 150, 250, 150);
            }
            else
            {
                var text = $"{playerName} researched ";
                foreach (var item in syncPacket.Research)
                {
                    var itemName = ContentSamples.ItemsByType[item.Key].Name;
                    text += $"{item.Value} {itemName} ";
                }
                Main.NewText(text, 150, 250, 150);
            }
            SyncPacket response = null;
            lock (syncLock)
            {
                if (syncPacket.Type == Type.FULL || syncPacket.Type == Type.FULL_RESPONSE)
                {
                    Research.CompareSetHighest(this, syncPacket.Research);
                    if (syncPacket.Type == Type.FULL)
                    {
                        // react to new player joining by sending full sync to them
                        response = new SyncPacket(Type.FULL_RESPONSE, syncPacket.Player, PrevResearch);
                    }
                }
                else
                {
                    Research.AddDiff(this, syncPacket.Research);
                }
            }
            if (response != null)
            {
                SendPacket(response);
            }
        }
    }
}
