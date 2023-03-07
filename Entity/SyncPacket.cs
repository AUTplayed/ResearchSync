using System;
using System.Collections.Generic;
using System.IO;

namespace ResearchSync.Entity
{
	internal class SyncPacket
	{
		public Type Type { get; set; }
		public sbyte Player { get; set; }   // either sender or target, depending on Type and if it's coming from the server or client
		public Dictionary<int, int> Research { get; set; }

		public SyncPacket(Type type, int player, Dictionary<int, int> research)
		{
			Type = type;
			Player = Convert.ToSByte(player);
			Research = research;
		}

		public SyncPacket(BinaryReader reader)
		{
			var type = reader.ReadByte();
			Type = (Type)type;
			Player = reader.ReadSByte();
			var size = reader.Read7BitEncodedInt();
			var research = new Dictionary<int, int>(size);
			for (int i = 0; i < size; i++)
			{
				var key = reader.Read7BitEncodedInt();
				var value = reader.Read7BitEncodedInt();
				research[key] = value;
			}
			Research = research;
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write((byte)Type);
			writer.Write(Player);
			writer.Write7BitEncodedInt(Research.Count);
			foreach (var item in Research)
			{
				writer.Write7BitEncodedInt(item.Key);
				writer.Write7BitEncodedInt(item.Value);
			}
		}

		public static SyncPacket ToRedirect(BinaryReader reader, int sender, out int target)
		{
			var packet = new SyncPacket(reader);
			target = -1;
			if (packet.Type == Type.FULL || packet.Type == Type.FULL_RESPONSE)
			{
				// player in packet is target
				target = Convert.ToInt32(packet.Player);
				// change packet to origin
				packet.Player = Convert.ToSByte(sender);
			}
			return packet;
		}
	}

	internal enum Type
	{
		FULL = 0, FULL_RESPONSE = 1, DIFF = 2
	}
}
