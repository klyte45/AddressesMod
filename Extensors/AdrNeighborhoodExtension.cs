using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Klyte.Commons.Interfaces;
using System.Collections;
using ColossalFramework.Threading;

namespace Klyte.Addresses.Extensors
{

    internal class AdrNeighborhoodExtension : ExtensionInterfaceListImpl<AdrConfigWarehouse, AdrConfigWarehouse.ConfigIndex, NeighborOption, AdrNeighborhoodExtension>
    {
        public override AdrConfigWarehouse.ConfigIndex ConfigIndexKey => AdrConfigWarehouse.ConfigIndex.NEIGHBOR_CONFIG_AZIMUTHS_STOPS;
        protected override bool AllowGlobal => false;

        public int getStopsCount(bool global = false)
        {
            return SafeGet(global).Count;
        }

        #region Azimuth
        public uint GetAzimuth(int idx)
        {
            string saved = SafeGet(idx, NeighborOption.AZIMUTH_STOP);
            if (uint.TryParse(saved, out uint result))
            {
                return result % 360;
            }
            return 0;
        }

        public void SetAzimuth(int idx, uint value)
        {
            idx = SafeSet(idx, NeighborOption.AZIMUTH_STOP, (value % 360).ToString());
            if (SafeGet(idx, NeighborOption.RANDOM_SEED) == null)
            {
                SetSeed(idx, new Randomizer(new System.Random().Next()).UInt32(0xFFFAFFFF));
            }
        }

        public Randomizer? GetRandomizerAt(float angle, bool global = false)
        {
            var list = SafeGet(global).Where(x => x.ContainsKey(NeighborOption.AZIMUTH_STOP) && x.ContainsKey(NeighborOption.RANDOM_SEED) && int.TryParse(x[NeighborOption.AZIMUTH_STOP], out int b) && uint.TryParse(x[NeighborOption.RANDOM_SEED], out uint c)).Select(x => Tuple.New(int.Parse(x[NeighborOption.AZIMUTH_STOP]), uint.Parse(x[NeighborOption.RANDOM_SEED]))).ToList();
            if (list.Count < 2)
            {
                return null;
            }
            list.Sort((x, y) => x.First - y.First);
            if (angle < list[0].First) return new Randomizer(list[list.Count - 1].Second);
            for (int i = 1; i < list.Count; i++)
            {
                if (angle < list[i].First) return new Randomizer(list[i - 1].Second);
            }
            return new Randomizer(list[list.Count - 1].Second);
        }
        #endregion

        #region Seed
        public uint GetSeed(int idx)
        {
            string saved = SafeGet(idx, NeighborOption.RANDOM_SEED);
            if (uint.TryParse(saved, out uint result))
            {
                return result;
            }
            return new Randomizer(new System.Random().Next()).UInt32(0xFFDFFFFF);
        }

        public void SetSeed(int idx, uint value)
        {
            SafeSet(idx, NeighborOption.RANDOM_SEED, (value).ToString());
        }
        #endregion
    }

    internal enum NeighborOption
    {
        AZIMUTH_STOP,
        RANDOM_SEED
    }
}
