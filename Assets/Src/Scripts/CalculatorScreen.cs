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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Controls the UI component of the calculator screen.
public class CalculatorScreen : MonoBehaviour {
  // Supported error types and messages.
  public enum Error {
    Overflow,
  }
  private const string _errPrefix = "ERR:";
  private static readonly Dictionary<Error, string> _errorMessages =
      new Dictionary<Error, string>() {
    // All the display error messages must be in upper cases, length <= 8.
    { Error.Overflow, "OVERFLOW" },
  };

  private const string _colorHighlighted = "#f30";
  private const string _initialText = "0";

  // The font size changes among three choices, depending on the length of the message.
  private static readonly (int minChar, int fontSize)[] _fontSizes = new (int, int)[] {
    (0, 280),
    (13, 140),
    (50, 90),
  };

  // Clears the screen.
  public void Clear() {
    var text = GetComponent<Text>();
    text.text = _initialText;
    text.fontSize = _fontSizes[0].fontSize;
  }

  // Prints an error message.
  public void PrintError(Error error) {
    string message = _errorMessages[error];
    var text = GetComponent<Text>();
    text.text = $"{_errPrefix}<color={_colorHighlighted}>{message}</color>";
    text.fontSize = _fontSizes[0].fontSize;
  }

  // Prints a message.
  public void Print(string message) {
    var text = GetComponent<Text>();
    if (string.IsNullOrEmpty(message)) {
      text.text = _initialText;
      text.fontSize = _fontSizes[0].fontSize;
    } else {
      text.text = message;
      for (int i = _fontSizes.Length - 1; i >= 0; i--) {
        if (message.Length > _fontSizes[i].minChar) {
          text.fontSize = _fontSizes[i].fontSize;
          break;
        }
      }
    }
  }

  void Awake() {
    Clear();
  }
}
