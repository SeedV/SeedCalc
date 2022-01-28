// Copyright 2021-2022 The SeedV Lab.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.Settings;

namespace SeedCalc {
  public static class LocalizationUtils {
    public const string ChineseLangCode = "zh-CN";
    public const string EnglishLangCode = "en";

    // The preference key to record the user selected locale.
    private const string _userPrefKeyUserSelectedLocale = "selected-locale";

    // The preference key to record the last system locale.
    private const string _userPrefKeySystemLocale = "system-locale";

    // The Unity's default System Locale Selector has a bug and always selects "en" on macOS. See
    // https://github.com/SeedV/SeedCalc/issues/26 for more details.
    //
    // This method detects the locale per the following logic:
    //
    // 1. Detects the current system locale with Application.systemLanguage.
    // 2. Detects if user has changed locale with the setting dialog. The locale set with the
    //    setting dialog will be logged at _userPrefKeyUserSelectedLocale in the player prefs.
    // 3. If user sets the locale with the setting dialog first, then changes the system locale,
    //    follows the system locale.
    public static void DetectAndSetLocale() {
      string langCode = GetSystemLangCode(out bool systemLocaleChanged);
      string userSelectedlangCode = PlayerPrefs.GetString(_userPrefKeyUserSelectedLocale, null);
      if (!systemLocaleChanged && !(userSelectedlangCode is null)) {
        langCode = userSelectedlangCode;
        Debug.Log($"Using user selected locale: {langCode}.");
      } else {
        Debug.Log($"Using system locale: {langCode}.");
      }
      SetLocale(langCode, false);
    }

    public static bool SetLocale(string langCode, bool updateUserSelectedLocale) {
      var locale = LocalizationSettings.AvailableLocales.GetLocale(langCode);
      if (!(locale is null)) {
        LocalizationSettings.SelectedLocale = locale;
        if (updateUserSelectedLocale) {
          SaveLocale(_userPrefKeyUserSelectedLocale, langCode);
        }
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

    // Sets a game object's active state, and updates its localized assets if it is set to active.
    public static void SetActiveAndUpdate(GameObject gameObject, bool show) {
      gameObject.SetActive(show);
      if (show) {
        UpdateLocalizedAssets(gameObject, true);
      }
    }

    private static void SaveLocale(string key, string langCode) {
      Debug.Assert(!string.IsNullOrEmpty(langCode));
      PlayerPrefs.SetString(key, langCode);
      // Flushes the prefs cache.
      PlayerPrefs.Save();
    }

    // Gets the system locale code. hasChanged will be set to true if the system locale is different
    // with the one detected in the last time.
    private static string GetSystemLangCode(out bool hasChanged) {
      string systemLangCode =
          (Application.systemLanguage == SystemLanguage.ChineseSimplified ||
              Application.systemLanguage == SystemLanguage.Chinese) ?
                ChineseLangCode : EnglishLangCode;
      string lastSystemLangCode = PlayerPrefs.GetString(_userPrefKeySystemLocale, null);
      SaveLocale(_userPrefKeySystemLocale, systemLangCode);
      hasChanged = lastSystemLangCode != systemLangCode;
      return systemLangCode;
    }
  }
}
