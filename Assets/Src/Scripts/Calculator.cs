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
using System.Text;
using AgileMvvm;

// The supported calculator states.
public enum CalculatorState {
  Okay,
  Overflow,
}

public static class ErrorMessageHelper {
  public static string ToString(this CalculatorState state) {
    return Enum.GetName(typeof(CalculatorState), state);
  }
  public static bool IsOk(this CalculatorState state) {
    return state == CalculatorState.Okay;
  }
  public static bool IsError(this CalculatorState state) {
    return !IsOk(state);
  }
}

// The calculator class. It is a ViewModel class that manages the underlying calculation states,
// e.g., via a SeedLang engine.
public class Calculator : ViewModel {
  private const int _maxChars = 100;
  private readonly StringBuilder _cachedInput = new StringBuilder();
  private CalculatorState _state;
  private string _content;

  [BindableProperty]
  public CalculatorState State {
    get => _state;
    set => MvvmSetter(ref _state, value);
  }

  [BindableProperty]
  public string Content {
    get => _content;
    set => MvvmSetter(ref _content, value);
  }

  public void OnInput(string input) {
    // If the calculator is in an error state, only the "AC" button can reset it.
    if (input == CalculatorInput.AllClear) {
      _cachedInput.Clear();
      Content = "";
      State = CalculatorState.Okay;
    }
    if (State.IsError()) {
      return;
    }
    if (CalculatorInput.TryGetPrintable(input, out string printable)) {
      _cachedInput.Append(printable);
      if (_cachedInput.Length > _maxChars) {
        State = CalculatorState.Overflow;
      } else {
        // TODO: invoke the underlying SeedLang engine to validate the cached expression.
        Content = _cachedInput.ToString();
      }
    } else if (input == CalculatorInput.Del) {
      if (_cachedInput.Length > 0) {
        _cachedInput.Remove(_cachedInput.Length - 1, 1);
      }
      Content = _cachedInput.ToString();
    } else if (input == CalculatorInput.Equal) {
      // TODO: invoke the underlying SeedLang engine to executed the cached expression.
    }
  }
}
