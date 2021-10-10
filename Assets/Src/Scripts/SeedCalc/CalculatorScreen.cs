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
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using AgileMvvm;
using SeedCalc;
using SeedLang.Common;

namespace SeedCalc {
  // The UI component of the calculator screen.
  public class CalculatorScreen : MonoBehaviour {
    private const string _errPrefix = "ERR:";
    private const string _colorHighlighted = "#f30";
    private const string _colorNumber = "#03f";
    private const string _colorOperator = "#090";
    private const string _colorParenthesis = "#555";
    private const string _colorInvalid = "#ccc";
    private const string _colorBeingCalculated = "#f90";
    private const string _initialText = "0";

    // The font size changes among three choices, depending on the length of the message.
    private static readonly (int minChar, int fontSize)[] _fontSizes = new (int, int)[] {
      (0, 280),
      (13, 140),
      (50, 90),
    };

    private static readonly Dictionary<string, string> _displayOperators =
        new Dictionary<string, string> {
      { "*", "\u00D7" },
      { "/", "\u00F7" },
    };

    private Text _text;

    // Clears the screen.
    public void Clear() {
      Print(null, 0);
    }

    // Prints an error message.
    public void PrintError(string errorMessage) {
      _text.text = $"{_errPrefix}<color={_colorHighlighted}>{errorMessage.ToUpper()}</color>";
      _text.fontSize = _fontSizes[0].fontSize;
    }

    // Prints a message. The parameter plainTextLength specifies the plain text length of message,
    // excluding style tags such as "<color=...>".
    public void Print(string message, int plainTextLength) {
      if (string.IsNullOrEmpty(message)) {
        _text.text = $"<color={_colorNumber}>{_initialText}</color>";
        _text.fontSize = _fontSizes[0].fontSize;
      } else {
        _text.text = message;
        for (int i = _fontSizes.Length - 1; i >= 0; i--) {
          if (plainTextLength > _fontSizes[i].minChar) {
            _text.fontSize = _fontSizes[i].fontSize;
            break;
          }
        }
      }
    }

    public void OnCalculatorStateUpdated(object sender, UpdatedEvent.Args args) {
      if (args.Value is CalculatorState state) {
        if (state.IsError()) {
          PrintError(state.ToString());
        }
      }
    }

    public void OnCalculatorDisplayTokensUpdated(object sender, UpdatedEvent.Args args) {
      if (args.Value is null) {
        Print(null, 0);
        return;
      }
      var displayContent = args.Value as DisplayContent;
      var textBuffer = new StringBuilder();
      int plainTextLength = 0;
      foreach (var token in displayContent.SyntaxTokens) {
        string tokenText = GetTokenText(displayContent.Expression, token.Range);
        string color;
        if (displayContent.BeingCalculatedRange != null &&
            displayContent.BeingCalculatedRange.Start <= token.Range.Start &&
            displayContent.BeingCalculatedRange.End >= token.Range.End) {
          color = _colorBeingCalculated;
        } else {
          switch (token.Type) {
            case SyntaxType.Operator:
              if (_displayOperators.TryGetValue(tokenText, out string displayToken)) {
                tokenText = displayToken;
              }
              color = _colorOperator;
              break;
            case SyntaxType.Parenthesis:
              color = _colorParenthesis;
              break;
            case SyntaxType.Unknown:
              color = _colorInvalid;
              break;
            case SyntaxType.Number:
            default:
              color = _colorNumber;
              break;
          }
        }
        textBuffer.Append($"<color={color}>{tokenText}</color>");
        plainTextLength += tokenText.Length;
      }
      Print(textBuffer.ToString(), plainTextLength);
    }

    void Awake() {
      _text = GetComponent<Text>();
      Clear();
    }

    private string GetTokenText(string expression, TextRange range) {
      return expression.Substring(range.Start.Column, range.End.Column - range.Start.Column + 1);
    }
  }
}
