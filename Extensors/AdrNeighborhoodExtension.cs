using ColossalFramework.Math;
using Klyte.Addresses.Xml;
using System.Linq;


namespace Klyte.Addresses.Extensors
{

    internal static class AdrNeighborhoodExtension
    {
        private static AdrNeighborhoodConfig NeighborhoodConfig => AdrController.CurrentConfig.GlobalConfig.NeighborhoodConfig;
        public static int GetStopsCount() => NeighborhoodConfig.Neighbors.Count;

        #region Azimuth
        public static uint GetAzimuth(int idx) => (NeighborhoodConfig.Neighbors.ElementAtOrDefault(idx)?.Azimuth ?? 0) % 360u;

        public static void SetAzimuth(int idx, ushort value)
        {
            if (idx < NeighborhoodConfig.Neighbors.Count)
            {
                NeighborhoodConfig.Neighbors[idx].Azimuth = value;
            }
            else
            {
                NeighborhoodConfig.AddToNeigborsListAt(idx, new AdrNeighborDetailConfig
                {
                    Azimuth = value,
                    Seed = new Randomizer(new System.Random().Next()).UInt32(0xFFFAFFFF)
                });
            }
        }

        public static Randomizer? GetRandomizerAt(float angle)
        {
            if (NeighborhoodConfig.Neighbors.Count == 0)
            {
                return null;
            }

            AdrNeighborDetailConfig[] sortedArray = NeighborhoodConfig.Neighbors.OrderBy(x => x.Azimuth).ToArray();
            if (angle < sortedArray[0].Azimuth)
            {
                return new Randomizer(sortedArray[sortedArray.Length - 1].Seed);
            }
            return new Randomizer(NeighborhoodConfig.Neighbors.OrderBy(x => x.Azimuth).Where(x => x.Azimuth < angle).LastOrDefault()?.Seed ?? 0);

        }
        #endregion

        #region Seed
        public static uint GetSeed(int idx) => (NeighborhoodConfig.Neighbors.ElementAtOrDefault(idx)?.Seed ?? 0);

        public static void SetSeed(int idx, uint value)
        {
            if (idx < NeighborhoodConfig.Neighbors.Count)
            {
                NeighborhoodConfig.Neighbors[idx].Seed = value;
            }
        }

        public static void SafeCleanEntry(int idx) => NeighborhoodConfig.RemoveNeighborAtIndex(idx);
        #endregion
    }

}
