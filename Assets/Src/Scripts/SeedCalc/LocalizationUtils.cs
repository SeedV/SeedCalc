using UnityEngine;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.Settings;

namespace SeedCalc {
  public static class LocalizationUtils {
    public const string ChineseLangCode = "zh-CN";
    public const string EnglishLangCode = "en-US";

    // The preference key used by the player prefs selector.
    private const string _userPrefKeySelectedLocale = "selected-locale";

    public static bool SetLocale(string langCode) {
      var locale = LocalizationSettings.AvailableLocales.GetLocale(langCode);
      if (!(locale is null)) {
        LocalizationSettings.SelectedLocale = locale;
        SaveLocale(langCode);
        Debug.Log($"Locale is set to {langCode}.");
        return true;
      } else {
        return false;
      }
    }

    public static string GetCurrentLocale() {
      return LocalizationSettings.SelectedLocale.Identifier.Code;
    }

    // Applies the current locale to a game object and/or its children.
    //
    // UnityEngine.Localization has an issue - the localized assets (e.g. strings) of an inactive
    // object will not be updated when the selected locale changes in runtime.
    //
    // A solution is to explicitly update the localized assets immediately after the object is set
    // to active:
    //
    //   obj.SetActive(true);
    //   LocalizationUtils.UpdateLocalizedAssets(obj);
    //
    // Or, if the children that are localized should also be updated recursively:
    //
    //   obj.SetActive(true);
    //   LocalizationUtils.UpdateLocalizedAssets(obj, true);
    public static void UpdateLocalizedAssets(GameObject gameObject, bool recursive = false) {
      if (gameObject is null) {
        return;
      }
      var localizer = gameObject.GetComponent<GameObjectLocalizer>();
      if (!(localizer is null)) {
        localizer.ApplyLocaleVariant(LocalizationSettings.SelectedLocale);
      }
      if (recursive && !(gameObject.transform is null)) {
        foreach (Transform child in gameObject.transform) {
          UpdateLocalizedAssets(child.gameObject, recursive);
        }
      }
    }

    private static void SaveLocale(string langCode) {
      Debug.Assert(!string.IsNullOrEmpty(langCode));
      PlayerPrefs.SetString(_userPrefKeySelectedLocale, langCode);
      // Flushes the prefs cache.
      PlayerPrefs.Save();
    }
  }
}
