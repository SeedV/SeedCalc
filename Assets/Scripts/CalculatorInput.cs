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

// Supported calculator commands and their corresponding printable strings.
public static class CalculatorInput {
  public const string AllClear = "AC";
  public const string LeftParenthsis = "(";
  public const string RightParenthsis = ")";
  public const string Del = "Del";
  public const string Add = "+";
  public const string Sub = "-";
  public const string Mul = "*";
  public const string Div = "/";
  public const string Dot = ".";
  public const string Equal = "=";

  // A map from input commands to the corresponding printable strings. Inputs like "AC" or "Del"
  // have no printable strings.
  private static readonly Dictionary<string, string> _printableInputs =
      new Dictionary<string, string>();

  static CalculatorInput() {
    _printableInputs.Add(LeftParenthsis, LeftParenthsis);
    _printableInputs.Add(RightParenthsis, RightParenthsis);
    _printableInputs.Add(Add, Add);
    _printableInputs.Add(Sub, Sub);
    _printableInputs.Add(Mul, "\u00D7");
    _printableInputs.Add(Div, "\u00F7");
    _printableInputs.Add(Dot, Dot);
    for (int i = 0; i <= 9; i++) {
      _printableInputs.Add($"{i}", $"{i}");
    }
  }

  public static bool IsPrintable(string input) {
    return _printableInputs.ContainsKey(input);
  }

  public static bool TryGetPrintable(string input, out string printable) {
    return _printableInputs.TryGetValue(input, out printable);
  }
}
