using Klyte.Addresses;
using Klyte.Addresses.Utils;

namespace Klyte.Commons
{
    public static class CommonProperties
    {
        public static bool DebugMode => AddressesMod.DebugMode;
        public static string Version => AddressesMod.Version;
        public static string ModName => AddressesMod.Instance.SimpleName;
        public static string ResourceBasePath => AdrResourceLoader.instance.Prefix;
        public static string Acronym => "ADR";
    }
}