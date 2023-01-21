using ResearchSync.Entity;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ResearchSync
{
	public class ResearchSync : Mod
	{

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			if (Main.netMode == NetmodeID.Server)
			{
				var syncPacket = SyncPacket.ToRedirect(reader, whoAmI, out var target);
				var packet = GetPacket();
				syncPacket.Serialize(packet);
				//Logger.Info($"Redirected {syncPacket.Type} package from {whoAmI} to {target}");
				packet.Send(target, whoAmI);
			} else
			{
				var packet = new SyncPacket(reader);
				var player = Main.player[Main.myPlayer].GetModPlayer<ResearchSyncPlayer>();
				player.UpdateResearch(packet);
			}
		}
	}
}