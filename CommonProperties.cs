using Klyte.Addresses;
using UnityEngine;

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
        public static string GitHubRepoPath { get; } = "klyte45/AddressesMod";

        public static string[] AssetExtraFileNames => null;

        public static string[] AssetExtraDirectoryNames => null;

        public static float UIScale { get; } = 1f;
        public static Color ModColor { get; } = new Color32(8, 70, 0, 255);
        public static MonoBehaviour Controller => AddressesMod.Controller;
    }
}