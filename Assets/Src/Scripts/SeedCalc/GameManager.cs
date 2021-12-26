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

using System;
using UnityEngine;
using UnityEngine.UI;
using AgileMvvm;

namespace SeedCalc {
  public class GameManager : MonoBehaviour {
    public CalculatorScreen Screen;
    public CuttingBoard CuttingBoard;
    public GameObject SettingsDialog;
    public GameObject AboutDialog;
    public Sprite ActiveButtonSprite;
    public Sprite InactiveButtonSprite;

    private Calculator _calculator = null;
    private Button _chineseButton = null;
    private Button _englishButton = null;
    private Button _soundSwitchButton = null;
    private GameObject _soundOnText = null;
    private GameObject _soundOffText = null;

    public void OnClickButton(string input) {
      if (_calculator.AcceptingInput) {
        PlayClickSound();
        _calculator.OnInput(input);
      }
    }

    public void OnOpenAbout() {
      PlayClickSound();
      LocalizationUtils.SetActiveAndUpdate(AboutDialog, true);
    }

    public void OnCloseAbout() {
      PlayClickSound();
      AboutDialog.SetActive(false);
    }

    public void OnOpenSettings() {
      PlayClickSound();
      LocalizationUtils.SetActiveAndUpdate(SettingsDialog, true);
      string langCode = LocalizationUtils.GetCurrentLocale();
      if (langCode == LocalizationUtils.ChineseLangCode) {
        SetButtonState(_chineseButton, true);
        SetButtonState(_englishButton, false);
      } else if (langCode == LocalizationUtils.EnglishLangCode) {
        SetButtonState(_chineseButton, false);
        SetButtonState(_englishButton, true);
      } else {
        Debug.Assert(false, $"{LocalizationUtils.GetCurrentLocale()}");
      }
      SetSoundSwitchState();
    }

    public void OnCloseSettings() {
      PlayClickSound();
      SettingsDialog.SetActive(false);
    }

    public void OnSetChinese() {
      PlayClickSound();
      if (LocalizationUtils.SetLocale(LocalizationUtils.ChineseLangCode)) {
        SetButtonState(_chineseButton, true);
        SetButtonState(_englishButton, false);
      }
    }

    public void OnSetEnglish() {
      PlayClickSound();
      if (LocalizationUtils.SetLocale(LocalizationUtils.EnglishLangCode)) {
        SetButtonState(_chineseButton, false);
        SetButtonState(_englishButton, true);
      }
    }

    public void OnSoundSwitch() {
      AudioListener.volume = AudioListener.volume > 0.0f ? 0.0f : 1.0f;
      SetSoundSwitchState();
      if (AudioListener.volume > 0.0f) {
        PlayClickSound();
      }
    }

    void Awake() {
      // Binds the ViewModel class to all the View classes.
      _calculator = new Calculator(CalculationMode.DemoCalculationSteps, this);
      _calculator.Bind(
          nameof(_calculator.State),
          new EventHandler<UpdatedEvent.Args>(Screen.OnCalculatorStateUpdated));
      _calculator.Bind(
          nameof(_calculator.ParsedExpression),
          new EventHandler<UpdatedEvent.Args>(Screen.OnCalculatorParsedExpressionUpdated));
      _calculator.Bind(
          nameof(_calculator.Result),
          new EventHandler<UpdatedEvent.Args>(Screen.OnCalculatorResultUpdated));
      _calculator.Bind(
          nameof(_calculator.ParsedExpression),
          new EventHandler<UpdatedEvent.Args>(CuttingBoard.OnCalculatorParsedExpressionUpdated));
      _calculator.Bind(
          nameof(_calculator.Result),
          new EventHandler<UpdatedEvent.Args>(CuttingBoard.OnCalculatorResultUpdated));

      var bk = SettingsDialog.transform.Find("Background");
      Debug.Assert(!(bk is null));
      _chineseButton = bk.Find("ButtonChinese")?.GetComponent<Button>();
      Debug.Assert(!(_chineseButton is null));
      _englishButton = bk.Find("ButtonEnglish")?.GetComponent<Button>();
      Debug.Assert(!(_englishButton is null));
      _soundSwitchButton = bk.Find("ButtonSoundSwitch")?.GetComponent<Button>();
      Debug.Assert(!(_soundSwitchButton is null));
      _soundOnText = _soundSwitchButton.transform.Find("SoundOnText")?.gameObject;
      Debug.Assert(!(_soundOnText is null));
      _soundOffText = _soundSwitchButton.transform.Find("SoundOffText")?.gameObject;
      Debug.Assert(!(_soundOffText is null));
    }

    private void SetButtonState(Button button, bool active) {
      button.GetComponent<Image>().sprite = active ? ActiveButtonSprite : InactiveButtonSprite;
    }

    private void SetSoundSwitchState() {
      SetButtonState(_soundSwitchButton, AudioListener.volume > 0.0f);
      if (AudioListener.volume > 0.0f) {
        LocalizationUtils.SetActiveAndUpdate(_soundOnText, true);
        _soundOffText.SetActive(false);
      } else {
        _soundOnText.SetActive(false);
        LocalizationUtils.SetActiveAndUpdate(_soundOffText, true);
      }
    }

    private void PlayClickSound() {
      GetComponent<AudioSource>().Play();
    }
  }
}
