using System;

namespace Klyte.Addresses.ModShared
{
    public static class AdrEvents
    {
        public static event Action EventZeroMarkerBuildingChange;

        public static void TriggerZeroMarkerBuildingChange() => EventZeroMarkerBuildingChange?.Invoke();
    }
}