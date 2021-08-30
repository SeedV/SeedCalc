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

using System.Text;

// The calculator that executes input expressions.
public class Calculator {
  private const int _maxChars = 100;
  private readonly CalculatorScreen _screen = null;
  private readonly StringBuilder _cachedInput = new StringBuilder();

  public bool HasError { get; private set; } = false;

  public Calculator(CalculatorScreen screen) {
    _screen = screen;
  }

  public void OnInput(string input) {
    // If the calculator is in an error state, only the "AC" button can reset it.
    if (input == CalculatorInput.AllClear) {
      _cachedInput.Clear();
      _screen.Clear();
      HasError = false;
    }
    if (HasError) {
      return;
    }

    if (CalculatorInput.TryGetPrintable(input, out string printable)) {
      _cachedInput.Append(printable);
      if (_cachedInput.Length > _maxChars) {
        ReportError(CalculatorScreen.Error.Overflow);
      } else {
        // TODO: invoke the underlying SeedLang engine to validate the cached expression.
        _screen.Print(_cachedInput.ToString());
      }
    } else if (input == CalculatorInput.Del) {
      if (_cachedInput.Length > 0) {
        _cachedInput.Remove(_cachedInput.Length - 1, 1);
      }
      _screen.Print(_cachedInput.ToString());
    } else if (input == CalculatorInput.Equal) {
      // TODO: invoke the underlying SeedLang engine to executed the cached expression.
    }
  }

  private void ReportError(CalculatorScreen.Error error) {
    HasError = true;
    _screen.PrintError(error);
  }
}
