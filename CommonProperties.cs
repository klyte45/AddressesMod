using Klyte.Addresses;
using Klyte.Addresses.Utils;

namespace Klyte.Commons
{
    public static class CommonProperties
    {
        public static bool DebugMode => AddressesMod.DebugMode;
        public static string Version => AddressesMod.Version;
        public static string ModName => AddressesMod.Instance.SimpleName;
        public static string Acronym => "ADR";
        public static string ModRootFolder => AddressesMod.FOLDER_NAME;
        public static string ModDllRootFolder { get; } = AddressesMod.RootFolder;
        public static string ModIcon => AddressesMod.Instance.IconName;
    }
}