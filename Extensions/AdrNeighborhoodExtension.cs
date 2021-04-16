using ColossalFramework.Math;
using Klyte.Addresses.ModShared;
using Klyte.Addresses.Xml;
using System.Linq;


namespace Klyte.Addresses.Extensions
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
            AdrFacade.TriggerBuildingNameStrategyChanged();
        }
        public static void SetFixedName(int idx, string value)
        {
            if (idx < NeighborhoodConfig.Neighbors.Count)
            {
                NeighborhoodConfig.Neighbors[idx].FixedName = value;
            }
            else
            {
                NeighborhoodConfig.AddToNeigborsListAt(idx, new AdrNeighborDetailConfig
                {
                    Azimuth = 0,
                    Seed = new Randomizer(new System.Random().Next()).UInt32(0xFFFAFFFF),
                    FixedName = value
                });
            }
            AdrFacade.TriggerBuildingNameStrategyChanged();
        }
        public static string GetFixedName(int idx) => NeighborhoodConfig.Neighbors.ElementAtOrDefault(idx)?.FixedName;

        public static void GetNeighborParams(float angle, out Randomizer? randomizer, out string fixedName)
        {
            randomizer = null;
            fixedName = null;
            if (NeighborhoodConfig.Neighbors.Count == 0)
            {
                return;
            }

            AdrNeighborDetailConfig[] sortedArray = NeighborhoodConfig.Neighbors.OrderBy(x => x.Azimuth).ToArray();
            AdrNeighborDetailConfig item =
                NeighborhoodConfig.Neighbors.Count == 1 ? sortedArray[0]
                : angle < sortedArray[0].Azimuth ? sortedArray[sortedArray.Length - 1] :
                NeighborhoodConfig.Neighbors.OrderBy(x => x.Azimuth).Where(x => x.Azimuth <= angle).LastOrDefault();
            if (item.FixedName != null)
            {
                fixedName = item.FixedName;
            }
            else
            {
                randomizer = new Randomizer(item.Seed);
            }

        }
        #endregion

        #region Seed
        public static uint GetSeed(int idx) => (NeighborhoodConfig.Neighbors.ElementAtOrDefault(idx)?.Seed ?? 0);

        public static void SetSeed(int idx, uint value)
        {
            if (idx < NeighborhoodConfig.Neighbors.Count)
            {
                NeighborhoodConfig.Neighbors[idx].Seed = value;
                NeighborhoodConfig.Neighbors[idx].FixedName = null;
            }
            AdrFacade.TriggerBuildingNameStrategyChanged();
        }

        public static void SafeCleanEntry(int idx) => NeighborhoodConfig.RemoveNeighborAtIndex(idx);
        #endregion
    }

}
