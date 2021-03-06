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

using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

using AgileMvvm;
using SeedLang.Common;

namespace SeedCalc {
  // The UI component of the calculator screen.
  public class CalculatorScreen : MonoBehaviour {
    private const string _errPrefix = "ERR:";
    private const string _colorHighlighted = "#f60";
    private const string _colorNumber = "#fff";
    private const string _colorMulDiv = "#ffbc33";
    private const string _colorAddSub = "#88c882";
    private const string _colorParenthesis = "#958dff";
    private const string _colorInvalid = "#666";
    private const string _colorBeingCalculated = "#e56663";
    private static readonly string _initialText = $"<color={_colorNumber}>0</color>";

    private static readonly Dictionary<string, string> _displayOperators =
        new Dictionary<string, string> {
      { "*", "\u00D7" },
      { "/", "\u00F7" },
      // Replaces the hyphen-minus sign U+002D with the minus sign U+2212, so that the minus sign in
      // the expression won't be treated as a hyphen when Unity wraps the top line text. Please
      // double check that the font used for screen texts does support the character U+2212,
      { "-", "\u2212" },
    };

    private TextMeshProUGUI _topLine;
    private TextMeshProUGUI _bottomLine;

    // Clears the screen.
    public void Clear() {
      PrintToTop(null);
      PrintToBottom(null);
    }

    // Prints an error message.
    public void PrintError(string errorMessage) {
      PrintToBottom($"{_errPrefix}<color={_colorHighlighted}>{errorMessage.ToUpper()}</color>");
    }

    // Prints a message to the top line.
    public void PrintToTop(string text) {
      PrintTo(_topLine, text is null ? "" : text);
    }

    // Prints a message to the bottom line.
    public void PrintToBottom(string text) {
      PrintTo(_bottomLine, text is null ? "" : text);
    }

    public void OnCalculatorStateUpdated(object sender, UpdatedEvent.Args args) {
      if (args.Value is CalculatorState state) {
        if (state.IsError()) {
          PrintError(state.ToString());
        }
      }
    }

    public void OnCalculatorParsedExpressionUpdated(object sender, UpdatedEvent.Args args) {
      if (args.Value is null) {
        PrintToTop(_initialText);
        PrintToBottom(_initialText);
        return;
      }
      var parsedExpression = args.Value as ParsedExpression;
      if (parsedExpression.SyntaxTokens.Count <= 0) {
        PrintToTop(_initialText);
        PrintToBottom(_initialText);
        return;
      }
      var textBuffer = new StringBuilder();
      foreach (var token in parsedExpression.SyntaxTokens) {
        string tokenText = parsedExpression.ExtractTokenText(token);
        string color;
        if (parsedExpression.HighlightedRange != null &&
            parsedExpression.HighlightedRange.Start <= token.Range.Start &&
            parsedExpression.HighlightedRange.End >= token.Range.End) {
          color = _colorBeingCalculated;
          if (token.Type == SyntaxType.Operator &&
              _displayOperators.TryGetValue(tokenText, out string displayToken)) {
            tokenText = displayToken;
          }
        } else {
          switch (token.Type) {
            case SyntaxType.Operator:
              if (tokenText == "*" || tokenText == "/") {
                color = _colorMulDiv;
              } else {
                color = _colorAddSub;
              }
              if (_displayOperators.TryGetValue(tokenText, out string displayToken)) {
                tokenText = displayToken;
              }
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
      }
      PrintToTop(textBuffer.ToString());
      if (!parsedExpression.BeingCalculated) {
        if (parsedExpression.TryParseLastTokenToNumber(out double lastNumber)) {
          PrintToBottom(NumberFormatter.Format((double)lastNumber));
        } else {
          PrintToBottom(null);
        }
      }
    }

    public void OnCalculatorResultUpdated(object sender, UpdatedEvent.Args args) {
      double? result = args.Value as double?;
      if (result is null) {
        PrintToBottom(null);
      } else {
        PrintToBottom(NumberFormatter.Format((double)result));
      }
    }

    void Start() {
      _topLine = transform.Find("TopLine").GetComponent<TextMeshProUGUI>();
      _bottomLine = transform.Find("BottomLine").GetComponent<TextMeshProUGUI>();
      Clear();
    }

    private void PrintTo(TextMeshProUGUI where, string text) {
      if (string.IsNullOrEmpty(text)) {
        where.text = _initialText;
      } else {
        where.text = text;
      }
    }
  }
}
