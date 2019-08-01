using System;

namespace Klyte.Addresses.ModShared
{
    public static class AdrEvents
    {
        public static event Action EventZeroMarkerBuildingChange;
        public static event Action EventRoadNamingChange;
        public static event Action EventDistrictColorChanged;
        public static event Action EventBuildingNameStrategyChanged;


        public static void TriggerZeroMarkerBuildingChange() => EventZeroMarkerBuildingChange?.Invoke();
        public static void TriggerRoadNamingChange() => EventRoadNamingChange?.Invoke();
        public static void TriggerDistrictColorChanged() => EventDistrictColorChanged?.Invoke();
        public static void TriggerBuildingNameStrategyChanged() => EventBuildingNameStrategyChanged?.Invoke();
    }
}