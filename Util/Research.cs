using System.Collections.Generic;

namespace ResearchSync.Util
{
    internal class Research
    {

        public static Dictionary<int, int> GetCurrent(ResearchSyncPlayer player)
        {
            return player.Player.creativeTracker.ItemSacrifices.SacrificesCountByItemIdCache;
        }

        public static Dictionary<int, int> GetDiff(ResearchSyncPlayer player)
        {
            var current = GetCurrent(player);
            var prev = player.PrevResearch;
            var diff = new Dictionary<int, int>();
            if (prev == null)
            {
                return diff;
            }
            foreach (var item in current)
            {
                int prevValue = 0;
                prev.TryGetValue(item.Key, out prevValue);
                if (prevValue < item.Value)
                {
                    diff[item.Key] = item.Value - prevValue;
                }
            }
            return diff;
        }

        public static void CompareSetHighest(ResearchSyncPlayer player, Dictionary<int, int> fullResearch)
        {
            var research = player.Player.creativeTracker.ItemSacrifices;
            var prev = player.PrevResearch;
            foreach (var item in fullResearch)
            {
                var currentCount = research.GetSacrificeCount(item.Key);
                if (currentCount < item.Value)
                {
                    // other player has more research here, can't set directly, add diff instead
                    research.RegisterItemSacrifice(item.Key, item.Value - currentCount);
                    prev[item.Key] = item.Value;
                }
            }
        }

        public static void AddDiff(ResearchSyncPlayer player, Dictionary<int, int> diff)
        {
            var research = player.Player.creativeTracker.ItemSacrifices;
            var prev = player.PrevResearch;
            foreach (var item in diff)
            {
                research.RegisterItemSacrifice(item.Key, item.Value);
                prev.TryGetValue(item.Key, out var prevCount);
                prev[item.Key] = prevCount + item.Value;
            }
        }
    }
}
