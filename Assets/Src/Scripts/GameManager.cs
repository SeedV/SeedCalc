// Copyright 2021 The Aha001 Team.
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
using System.Collections.Generic;
using UnityEngine;
using AgileMvvm;
using SeedCalc;

public class GameManager : MonoBehaviour {
  private const string _audioSourcePrefix = "Audio";
  private readonly Dictionary<string, AudioSource> _audioTable =
      new Dictionary<string, AudioSource>();
  private const string _buttonClickAudioName = "Click";
  private Calculator _calculator = null;

  // The UI component of the calculator screen must be associated to this field in Unity.
  public CalculatorScreen Screen;

  public void OnClickButton(string input) {
    PlayAudio(_buttonClickAudioName);
    _calculator.OnInput(input);
  }

  public void PlayAudio(string name) {
    if (_audioTable.TryGetValue(name, out var audioSource) && !audioSource.isPlaying) {
      audioSource.Play();
    }
  }

  void Awake() {
    LocateAudioSources();
    // Binds the ViewModel class to all View classes.
    _calculator = new Calculator();
    _calculator.Bind(
        nameof(_calculator.State),
        new EventHandler<UpdatedEvent.Args>(Screen.OnCalculatorStateUpdated));
    _calculator.Bind(
        nameof(_calculator.DisplayContent),
        new EventHandler<UpdatedEvent.Args>(Screen.OnCalculatorDisplayTokensUpdated));
  }

  // Collects all the audio sources that associate with the GameManager instance.
  private void LocateAudioSources() {
    foreach (Transform child in transform) {
      var childObject = child.gameObject;
      if (childObject.name.StartsWith(_audioSourcePrefix)) {
        string audioName = childObject.name.Substring(_audioSourcePrefix.Length);
        var audioSource = childObject.GetComponent<AudioSource>();
        _audioTable.Add(audioName, audioSource);
      }
    }
  }
}
