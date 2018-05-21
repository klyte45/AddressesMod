using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Addresses.i18n;
using Klyte.Addresses.Utils;
using Klyte.Commons.Extensors;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("1.2.0.*")]

namespace Klyte.Addresses
{
    public class AddressesMod : MonoBehaviour, IUserMod, ILoadingExtension
    {
        public static readonly string FOLDER_NAME = AdrUtils.BASE_FOLDER_PATH + "AddressesNames";
        public const string ROAD_SUBFOLDER_NAME = "Roads";
        public const string ROADPREFIX_SUBFOLDER_NAME = "RoadsPrefix";
        public const string NEIGHBOR_SUBFOLDER_NAME = "RegionCities";
        public const string CONFIG_FILENAME = "Addresses0";
        public const string ROAD_LOCALE_FIXED_IDENTIFIER = "ROAD_NAME";
        public static string minorVersion => majorVersion + "." + typeof(AddressesMod).Assembly.GetName().Version.Build;
        public static string majorVersion => typeof(AddressesMod).Assembly.GetName().Version.Major + "." + typeof(AddressesMod).Assembly.GetName().Version.Minor;
        public static string fullVersion => minorVersion + " r" + typeof(AddressesMod).Assembly.GetName().Version.Revision;
        public static string version
        {
            get {
                if (typeof(AddressesMod).Assembly.GetName().Version.Minor == 0 && typeof(AddressesMod).Assembly.GetName().Version.Build == 0)
                {
                    return typeof(AddressesMod).Assembly.GetName().Version.Major.ToString();
                }
                if (typeof(AddressesMod).Assembly.GetName().Version.Build > 0)
                {
                    return minorVersion;
                }
                else
                {
                    return majorVersion;
                }
            }
        }


        public static AddressesMod instance;

        private SavedBool m_debugMode;
        private SavedString m_selectedRoadFile;
        public bool needShowPopup;
        private static bool isLocaleLoaded = false;

        public static bool LocaleLoaded => isLocaleLoaded;

        private static bool m_isKlyteCommonsLoaded = false;
        public static bool IsKlyteCommonsEnabled()
        {
            if (!m_isKlyteCommonsLoaded)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var assembly = (from a in assemblies
                                where a.GetType("Klyte.Commons.KlyteCommonsMod") != null
                                select a).SingleOrDefault();
                if (assembly != null)
                {
                    m_isKlyteCommonsLoaded = true;
                }
            }
            return m_isKlyteCommonsLoaded;
        }

        public static SavedBool debugMode => instance.m_debugMode;
        public static SavedString selectedRoadFile => instance.m_selectedRoadFile;

        private SavedString currentSaveVersion => new SavedString("AdrSaveVersion", Settings.gameSettingsFile, "null", true);


        private SavedInt currentLanguageId => new SavedInt("AdrLanguage", Settings.gameSettingsFile, 0, true);

        public static bool isCityLoaded => Singleton<SimulationManager>.instance.m_metaData != null;

        public string Name => "Addresses & Names " + version;

        public string Description => "TLMR's Extension which allow road name generation customization. Requires Klyte Commons subscribed and active.";

        public static string roadPath => FOLDER_NAME + Path.DirectorySeparatorChar + ROAD_SUBFOLDER_NAME;
        public static string roadPrefixPath => FOLDER_NAME + Path.DirectorySeparatorChar + ROADPREFIX_SUBFOLDER_NAME;
        public static string neigborsPath => FOLDER_NAME + Path.DirectorySeparatorChar + NEIGHBOR_SUBFOLDER_NAME;

        public void OnCreated(ILoading loading)
        {
        }

        public AddressesMod()
        {
            Debug.LogWarningFormat("Adrv" + majorVersion + " LOADING ADDRESSES ");
            SettingsFile adrSettings = new SettingsFile
            {
                fileName = CONFIG_FILENAME
            };
            Debug.LogWarningFormat("Adrv" + majorVersion + " SETTING FILES");
            try
            {
                GameSettings.AddSettingsFile(adrSettings);
            }
            catch (Exception e)
            {
                SettingsFile tryLoad = GameSettings.FindSettingsFileByName(CONFIG_FILENAME);
                if (tryLoad == null)
                {
                    Debug.LogErrorFormat("Adrv" + majorVersion + " SETTING FILES FAIL!!! ");
                    Debug.LogError(e.Message);
                    Debug.LogError(e.StackTrace);
                }
                else
                {
                    adrSettings = tryLoad;
                }
            }
            m_debugMode = new SavedBool("AdrDebugMode", Settings.gameSettingsFile, typeof(AddressesMod).Assembly.GetName().Version.Major == 0, true);
            if (m_debugMode.value)
                Debug.LogWarningFormat("currentSaveVersion.value = {0}, fullVersion = {1}", currentSaveVersion.value, fullVersion);
            if (currentSaveVersion.value != fullVersion)
            {
                needShowPopup = true;
            }
            m_selectedRoadFile = new SavedString("AdrRoadFileName", Settings.gameSettingsFile, "", true);
            LocaleManager.eventLocaleChanged += new LocaleManager.LocaleChangedHandler(autoLoadAdrLocale);
            if (instance != null) { Destroy(instance); }
            instance = this;
            loadAdrLocale(false);
        }
        public void OnSettingsUI(UIHelperBase helperDefault)
        {
            if (!IsKlyteCommonsEnabled())
            {
                return;
            }
            FileInfo fi = AdrUtils.EnsureFolderCreation(FOLDER_NAME);
            FileInfo fiRoad = AdrUtils.EnsureFolderCreation(roadPath);
            FileInfo fiRoadPrefix = AdrUtils.EnsureFolderCreation(roadPrefixPath);
            FileInfo fiNeighbors = AdrUtils.EnsureFolderCreation(neigborsPath);
            UIHelperExtension helper = new UIHelperExtension((UIHelper)helperDefault);

            void ev()
            {
                foreach (Transform child in helper.self.transform)
                {
                    GameObject.Destroy(child?.gameObject);
                }

                helper.self.eventVisibilityChanged += delegate (UIComponent component, bool b)
                {
                    if (b)
                    {
                        showVersionInfoPopup();
                    }
                };

                UIHelperExtension group8 = helper.AddGroupExtended(Locale.Get("ADR_GENERAL_INFO"));
                group8.AddLabel(Locale.Get("ADR_ROAD_NAME_FILES_PATH_TITLE") + ":");
                var namesFilesButton = ((UIButton)group8.AddButton("/", () => { ColossalFramework.Utils.OpenInFileBrowser(fiRoad.FullName); }));
                namesFilesButton.textColor = Color.yellow;
                AdrUtils.LimitWidth(namesFilesButton, 710);
                namesFilesButton.text = fiRoad.FullName + Path.DirectorySeparatorChar;

                group8.AddLabel(Locale.Get("ADR_ROAD_PREFIX_NAME_FILES_PATH_TITLE") + ":");
                var prefixFilesButton = ((UIButton)group8.AddButton("/", () => { ColossalFramework.Utils.OpenInFileBrowser(fiRoadPrefix.FullName); }));
                prefixFilesButton.textColor = Color.yellow;
                AdrUtils.LimitWidth(prefixFilesButton, 710);
                prefixFilesButton.text = fiRoadPrefix.FullName + Path.DirectorySeparatorChar;

                group8.AddLabel(Locale.Get("ADR_NEIGHBOR_CITIES_NAME_FILES_PATH_TITLE") + ":");
                var neighButton = ((UIButton)group8.AddButton("/", () => { ColossalFramework.Utils.OpenInFileBrowser(fiNeighbors.FullName); }));
                neighButton.textColor = Color.yellow;
                AdrUtils.LimitWidth(neighButton, 710);
                neighButton.text = fiNeighbors.FullName + Path.DirectorySeparatorChar;

                UIHelperExtension group7 = helper.AddGroupExtended(Locale.Get("ADR_ADDITIONAL_FILES_SOURCE"));
                group7.AddLabel(Locale.Get("ADR_GET_FILES_GITHUB"));
                group7.AddButton(Locale.Get("ADR_GO_TO_GITHUB"), () => { Application.OpenURL("https://github.com/klyte45/AddressesFiles"); });


                UIHelperExtension group9 = helper.AddGroupExtended(Locale.Get("ADR_BETAS_EXTRA_INFO"));
                group9.AddDropdownLocalized("ADR_MOD_LANG", AdrLocaleUtils.getLanguageIndex(), currentLanguageId.value, delegate (int idx)
                {
                    currentLanguageId.value = idx;
                    loadAdrLocale(true);
                });
                group9.AddCheckbox(Locale.Get("ADR_DEBUG_MODE"), m_debugMode.value, delegate (bool val) { m_debugMode.value = val; });
                group9.AddLabel("Version: " + fullVersion);
                group9.AddLabel(Locale.Get("ADR_ORIGINAL_TLM_VERSION") + " " + string.Join(".", ResourceLoader.loadResourceString("TLMVersion.txt").Split(".".ToCharArray()).Take(3).ToArray()));
                group9.AddButton(Locale.Get("ADR_RELEASE_NOTES"), delegate ()
                {
                    showVersionInfoPopup(true);
                });

                AdrUtils.doLog("End Loading Options");
            }
            if (IsKlyteCommonsEnabled())
            {
                loadAdrLocale(false);
                ev();
            }
            else
            {
                eventOnLoadLocaleEnd = null;
                eventOnLoadLocaleEnd += ev;
            }
        }



        public bool showVersionInfoPopup(bool force = false)
        {
            if (needShowPopup || force)
            {
                try
                {
                    UIComponent uIComponent = UIView.library.ShowModal("ExceptionPanel");
                    if (uIComponent != null)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        BindPropertyByKey component = uIComponent.GetComponent<BindPropertyByKey>();
                        if (component != null)
                        {
                            string title = "Addresses v" + version;
                            string notes = ResourceLoader.loadResourceString("UI.VersionNotes.txt");
                            string text = "Addresses was updated! Release notes:\r\n\r\n" + notes;
                            string img = "IconMessage";
                            component.SetProperties(TooltipHelper.Format(new string[]
                            {
                            "title",
                            title,
                            "message",
                            text,
                            "img",
                            img
                            }));
                            needShowPopup = false;
                            currentSaveVersion.value = fullVersion;
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        AdrUtils.doLog("PANEL NOT FOUND!!!!");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    AdrUtils.doErrorLog("showVersionInfoPopup ERROR {0} {1}", e.GetType(), e.Message);
                }
            }
            return false;
        }

        public void autoLoadAdrLocale()
        {
            if (currentLanguageId.value == 0)
            {
                loadAdrLocale(false);
            }
        }
        public void loadAdrLocale(bool force)
        {
            if (SingletonLite<LocaleManager>.exists && IsKlyteCommonsEnabled() && (!isLocaleLoaded || force))
            {
                AdrLocaleUtils.loadLocale(currentLanguageId.value == 0 ? SingletonLite<LocaleManager>.instance.language : AdrLocaleUtils.getSelectedLocaleByIndex(currentLanguageId.value), force);
                if (!isLocaleLoaded)
                {
                    isLocaleLoaded = true;
                    eventOnLoadLocaleEnd?.Invoke();
                }
            }
        }
        private delegate void OnLocaleLoadedFirstTime();
        private event OnLocaleLoadedFirstTime eventOnLoadLocaleEnd;

        private bool m_loaded = false;

        public void OnLevelLoaded(LoadMode mode)
        {
            AdrUtils.doLog("LEVEL LOAD");
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame && mode != LoadMode.NewGameFromScenario)
            {
                m_loaded = false;
                AdrUtils.doLog("NOT GAME ({0})", mode);
                return;
            }
            if (!IsKlyteCommonsEnabled())
            {
                throw new Exception("Addresses requires Klyte Commons installed!");
            }
            if (AdrController.taAdr == null)
            {
                AdrController.taAdr = CreateTextureAtlas("UI.Images.sprites.png", "ServiceVehicleManagerSprites", GameObject.FindObjectOfType<UIView>().FindUIComponent<UIPanel>("InfoPanel").atlas.material, 64, 64, new string[] {
                    "AddressesIcon","AddressesIconSmall","ToolbarIconGroup6Hovered","ToolbarIconGroup6Focused","HelicopterIndicator","RemoveUnwantedIcon","24hLineIcon", "PerHourIcon"
                });
            }

            AdrController.instance.enabled = true;

            loadAdrLocale(false);
            m_loaded = true;
        }

        public void OnLevelUnloading()
        {
            if (!m_loaded) { return; }
            //			Log.debug ("LEVELUNLOAD");
            m_loaded = false;
        }

        public void OnReleased()
        {

        }

        UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, Material baseMaterial, int spriteWidth, int spriteHeight, string[] spriteNames)
        {
            Texture2D tex = new Texture2D(spriteWidth * spriteNames.Length, spriteHeight, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Bilinear
            };
            { // LoadTexture
                tex.LoadImage(ResourceLoader.loadResourceData(textureFile));
                tex.Apply(true, true);
            }
            UITextureAtlas atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            { // Setup atlas
                Material material = (Material)Material.Instantiate(baseMaterial);
                material.mainTexture = tex;
                atlas.material = material;
                atlas.name = atlasName;
            }
            // Add sprites
            for (int i = 0; i < spriteNames.Length; ++i)
            {
                float uw = 1.0f / spriteNames.Length;
                var spriteInfo = new UITextureAtlas.SpriteInfo()
                {
                    name = spriteNames[i],
                    texture = tex,
                    region = new Rect(i * uw, 0, uw, 1),
                };
                atlas.AddSprite(spriteInfo);
            }
            return atlas;
        }

    }

}
