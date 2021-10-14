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
using System.Text;
using AgileMvvm;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedCalc {
  // The supported states of the calculator.
  public enum CalculatorState {
    Okay,
    // The length of the input string exceeds the buffer limit, or the calculation exceeds the math
    // or data type limit.
    Overflow,
    // The expression has one or more syntax errors.
    Syntax,
    // Division-by-zero error.
    DivBy0,
  }

  public static class CalculatorStateHelper {
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

  // Supported calculator inputs.
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
    public const string N0 = "0";
    public const string N1 = "1";
    public const string N2 = "2";
    public const string N3 = "3";
    public const string N4 = "4";
    public const string N5 = "5";
    public const string N6 = "6";
    public const string N7 = "7";
    public const string N8 = "8";
    public const string N9 = "9";
  }

  // The calculator class. It is a ViewModel class that manages the underlying calculation states,
  // e.g., via a SeedLang executor.
  public class Calculator : ViewModel {
    private class Visualizer : IVisualizer<BinaryEvent>, IVisualizer<EvalEvent> {
      // The code range that is under execution.
      public TextRange SourceRange { get; private set; }
      // The result of the current execution step.
      public IValue Result { get; private set; }

      public void On(BinaryEvent be) {
        if (be.Range is TextRange range) {
          SourceRange = range;
        }
        Result = be.Result;
      }

      public void On(EvalEvent ee) {
        if (ee.Range is TextRange range) {
          SourceRange = range;
        }
        Result = ee.Value;
      }
    }

    private const string _moduleName = "SeedCalc";
    private const string _evalPrefix = "eval ";
    private const int _maxChars = 100;

    [BindableProperty]
    public CalculatorState State {
      get => _state;
      set => MvvmSetter(ref _state, value);
    }

    [BindableProperty]
    public DisplayContent DisplayContent {
      get => _displayContent;
      set => MvvmSetter(ref _displayContent, value);
    }

    private CalculatorState _state;
    private readonly StringBuilder _cachedInput = new StringBuilder();
    private DisplayContent _displayContent = null;

    public void OnInput(string input) {
      // A simple state machine.
      if (State.IsError()) {
        switch (input) {
          case CalculatorInput.AllClear:
            ResetState();
            AllClear();
            break;
          case CalculatorInput.Del:
            ResetState();
            Backspace();
            Parse();
            break;
          case CalculatorInput.Equal:
          default:
            // Neither a printable input nor the Equal key is accepted if the calculator is in an
            // error state.
            break;
        }
      } else {
        switch (input) {
          case CalculatorInput.AllClear:
            AllClear();
            break;
          case CalculatorInput.Del:
            Backspace();
            Parse();
            break;
          case CalculatorInput.Equal:
            Execute();
            break;
          case CalculatorInput.Dot:
            // Adds the leading zero before a single dot.
            if (_cachedInput.Length <= 0) {
              Append(CalculatorInput.N0);
            }
            Append(CalculatorInput.Dot);
            if (State.IsOk()) {
              Parse();
            }
            break;
          default:
            Append(input);
            if (State.IsOk()) {
              Parse();
            }
            break;
        }
      }
    }

    // Resets the calculator state to OK.
    private void ResetState() {
      State = CalculatorState.Okay;
    }

    // Clears the input buffer and the tokens for display.
    private void AllClear() {
      _cachedInput.Clear();
      DisplayContent = null;
    }

    // Appends the input character to the buffer.
    private void Append(string input) {
      _cachedInput.Append(input);
      if (_cachedInput.Length > _maxChars) {
        State = CalculatorState.Overflow;
      }
    }

    // Removes the last character from the buffer.
    private void Backspace() {
      if (_cachedInput.Length > 0) {
        _cachedInput.Remove(_cachedInput.Length - 1, 1);
      }
    }

    // Parses the input string to an expression.
    private void Parse() {
      string expression = _cachedInput.ToString();
      if (string.IsNullOrEmpty(expression)) {
        DisplayContent = null;
        return;
      }
      var collection = new DiagnosticCollection();
      var executor = new Executor();
      executor.Parse(expression, _moduleName, SeedXLanguage.Python, collection);
      DisplayContent = new DisplayContent(expression, executor.SyntaxTokens, null);
    }

    // Executes the current expression.
    private void Execute() {
      string expression = _cachedInput.ToString();
      if (string.IsNullOrEmpty(expression)) {
        DisplayContent = null;
        return;
      }

      // For now SeedPython only executes full "eval" statements.
      string source = _evalPrefix + " " + expression;
      var executor = new Executor();
      var collection = new DiagnosticCollection();
      if (!executor.Parse(source, _moduleName, SeedXLanguage.Python, collection)) {
        State = CalculatorState.Syntax;
        return;
      }
      var visualizer = new Visualizer();
      executor.Register(visualizer);
      executor.Run(RunType.Ast);
      executor.Unregister(visualizer);

      string resultText = visualizer.Result.ToString();
      var resultRange = new TextRange(1, 0, 1, resultText.Length - 1);
      var resultToken = new SyntaxToken(SyntaxType.Number, resultRange);
      var resultTokens = new List<SyntaxToken> { resultToken };
      DisplayContent = new DisplayContent(resultText, resultTokens, null);
      _cachedInput.Clear();
    }
  }
}
