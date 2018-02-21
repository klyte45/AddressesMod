using ColossalFramework.Globalization;
using Klyte.Addresses.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Klyte.Addresses.i18n
{
    internal class AdrLocaleUtils
    {
        private const string lineSeparator = "\r\n";
        private const string kvSeparator = "=";
        private const string idxSeparator = ">";
        private const string localeKeySeparator = "|";
        private const string commentChar = "#";
        private static string language = "";
        private static string[] locales = new string[] { "en", "pt", "ko", "de", "cn", "pl", "nl" };

        public static string loadedLanguage
        {
            get {
                return language;
            }
        }

        public static string[] getLanguageIndex()
        {
            Array8<string> saida = new Array8<string>((uint)locales.Length + 1);
            saida.m_buffer[0] = Locale.Get("ADR_GAME_DEFAULT_LANGUAGE");
            for (int i = 0; i < locales.Length; i++)
            {
                saida.m_buffer[i + 1] = Locale.Get("ADR_LANG", locales[i]);
            }
            return saida.m_buffer;
        }

        public static string getSelectedLocaleByIndex(int idx)
        {
            if (idx <= 0 || idx > locales.Length)
            {
                return "en";
            }
            return locales[idx - 1];
        }

        public static void loadLocale(string localeId, bool force)
        {
            if (force)
            {
                LocaleManager.ForceReload();
            }
            loadLocaleIntern(localeId, true);
        }
        private static void loadLocaleIntern(string localeId, bool setLocale)
        {
            Locale target = AdrUtils.GetPrivateField<Locale>(LocaleManager.instance, "m_Locale");
            string load = ResourceLoader.loadResourceString("UI.i18n." + localeId + ".properties");
            if (load == null)
            {
                AdrUtils.doErrorLog("FILE " + "UI.i18n." + localeId + ".properties" + " NOT LOADED!!!!");
                load = ResourceLoader.loadResourceString("UI.i18n.en.properties");
                if (load == null)
                {
                    AdrUtils.doErrorLog("LOCALE NOT LOADED!!!!");
                    return;
                }
                localeId = "en";
            }

            LoadIntoLocale(target, load, "ADR_");

            if (localeId != "en")
            {
                loadLocaleIntern("en", false);
            }
            if (setLocale)
            {
                language = localeId;
            }

        }

        public static void LoadIntoLocale(Locale target, string load, string prefix = "", string fixedName = null)
        {
            Locale.Key k;
            int idxFn = 0;
            foreach (var line in load.Split(new string[] { lineSeparator }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith(commentChar)) continue;
                string identifier, localeKey, value;
                int idx;
                if (fixedName == null)
                {
                    if (!line.Contains(kvSeparator)) continue;
                    var array = line.Split(kvSeparator.ToCharArray(), 2);
                    value = array[1];
                    idx = 0;
                    localeKey = null;
                    if (array[0].Contains(idxSeparator))
                    {
                        var arrayIdx = array[0].Split(idxSeparator.ToCharArray());
                        if (!int.TryParse(arrayIdx[1], out idx))
                        {
                            continue;
                        }
                        array[0] = arrayIdx[0];

                    }
                    if (array[0].Contains(localeKeySeparator))
                    {
                        array = array[0].Split(localeKeySeparator.ToCharArray());
                        localeKey = array[1];
                    }
                    identifier = prefix + array[0];
                }
                else
                {
                    identifier = fixedName;
                    localeKey = null;
                    idx = idxFn++;
                    value = line;
                }
                k = new Locale.Key()
                {
                    m_Identifier = identifier,
                    m_Key = localeKey,
                    m_Index = idx
                };
                if (!target.Exists(k))
                {
                    target.AddLocalizedString(k, value.Replace("\\n", "\n"));
                }
                else
                {
                    AdrUtils.doLog("Entrada já existente: {0}", k);
                }
            }
        }
    }
}
