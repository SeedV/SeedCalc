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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AgileMvvm;
using SeedLang.Common;
using SeedLang.Runtime;
using UnityEngine;

namespace SeedCalc {
  // The supported states of the calculator.
  public enum CalculatorState {
    // The ready state.
    Okay,
    // The calculating animation is being played.
    Calculating,
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
      return state == CalculatorState.Okay || state == CalculatorState.Calculating;
    }
    public static bool IsError(this CalculatorState state) {
      return !IsOk(state);
    }
  }

  // The calculation modes that the calculator supports.
  public enum CalculationMode {
    // Completes the calculation immediately.
    CalculateImmediately,
    // Plays an animation to demonstrate the calculation steps.
    DemoCalculationSteps,
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
    public const string N00 = "00";
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
    // The intermediate state of each calculation step.
    private class StepState {
      // The code range that is under execution.
      public TextRange SourceRange { get; set; }
      // The result of the current execution step.
      public double Result { get; set; }
    }

    private class Visualizer :
        IVisualizer<UnaryEvent>, IVisualizer<BinaryEvent>, IVisualizer<EvalEvent> {
      public List<StepState> StepStates { get; private set; } = new List<StepState>();
      public double FinalResult { get; private set; }

      public void On(UnaryEvent e) {
        // Although SeedLang is able to separate unary operations from an expression, we ignore them
        // intentionally so that a positive/negative number will be treated as a whole part.
      }

      public void On(BinaryEvent e) {
        RecordStep(e.Range as TextRange, e.Result.Number);
      }

      public void On(EvalEvent e) {
        FinalResult = e.Value.Number;
      }

      private void RecordStep(TextRange range, double result) {
        StepStates.Add(new StepState {
          SourceRange = range,
          Result = result,
        });
      }
    }

    // TODO: Adjust this limit when the screen size / font design is updated.
    private const int _maxChars = 40;
    private const string _moduleName = "SeedCalc";
    private const int _calcAnimBlinkTimes = 3;
    private const float _calcAnimBlinkInterval = 0.1f;
    private const float _calcAnimInterval = 0.4f;

    [BindableProperty]
    public CalculatorState State {
      get => _state;
      set => MvvmSetter(ref _state, value);
    }

    [BindableProperty]
    public ParsedExpression ParsedExpression {
      get => _parsedExpression;
      set => MvvmSetter(ref _parsedExpression, value);
    }

    [BindableProperty]
    // A nullable value. It is null when there is no calculation result yet.
    public double? Result {
      get => _result;
      set => MvvmSetter(ref _result, value);
    }

    public CalculationMode CalculationMode { get; set; } = CalculationMode.CalculateImmediately;

    public bool AcceptingInput => State != CalculatorState.Calculating;

    public bool HasResult => !(Result is null);

    private GameManager _gameManager;
    private CalculatorState _state;
    private readonly StringBuilder _cachedExpression = new StringBuilder();
    private ParsedExpression _parsedExpression = null;
    private double? _result = null;

    public Calculator(CalculationMode calculationMode, GameManager gameManager) {
      CalculationMode = calculationMode;
      _gameManager = gameManager;
    }

    public void OnInput(string input) {
      Debug.Assert(AcceptingInput);

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
      } else if (HasResult) {
        switch (input) {
          case CalculatorInput.AllClear:
            AllClear();
            break;
          case CalculatorInput.Del:
            // When there is already a calculating result, a Del key modifies the input buffer and
            // switches back to a no-result state - this enables users to edit the expression
            // in-place even after the result has been got.
            Result = null;
            Backspace();
            Parse();
            break;
          case CalculatorInput.Equal:
            break;
          case CalculatorInput.N0:
          case CalculatorInput.N00:
          case CalculatorInput.N1:
          case CalculatorInput.N2:
          case CalculatorInput.N3:
          case CalculatorInput.N4:
          case CalculatorInput.N5:
          case CalculatorInput.N6:
          case CalculatorInput.N7:
          case CalculatorInput.N8:
          case CalculatorInput.N9:
            // For numbers, clears the input buffer and the result, then starts from a new
            // expresion.
            _cachedExpression.Clear();
            Result = null;
            HandleVisibleKeys(input);
            break;
          default:
            Result = null;
            HandleVisibleKeys(input);
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
          default:
            HandleVisibleKeys(input);
            break;
        }
      }
    }

    private void HandleVisibleKeys(string input) {
      switch (input) {
        case CalculatorInput.Dot:
          // Do not repeat dots.
          char lastChar = !(ParsedExpression is null) && ParsedExpression.Expression.Length > 0 ?
              ParsedExpression.Expression[ParsedExpression.Expression.Length - 1] : '\0';
          if (lastChar != CalculatorInput.Dot[0]) {
            // Adds a leading zero if "." is the first input of a number.
            if (ParsedExpression is null || !ParsedExpression.LastTokenIsNumber()) {
              Append(CalculatorInput.N0);
            }
            Append(input);
          }
          break;
        case CalculatorInput.N0:
          // A number can be "0", but cannot be "0123".
          if (ParsedExpression is null || !ParsedExpression.LastTokenIsNumber()) {
            Append(input);
          } else if (ParsedExpression.ExtractLastTokenText() != "0") {
            Append(input);
          }
          break;
        case CalculatorInput.N00:
          // A number can be neither "00" nor "00123".
          if (ParsedExpression is null || !ParsedExpression.LastTokenIsNumber()) {
            // To start a number, only one "0" is appended.
            Append(CalculatorInput.N0);
          } else if (ParsedExpression.ExtractLastTokenText() != "0") {
            Append(input);
          }
          break;
        case CalculatorInput.N1:
        case CalculatorInput.N2:
        case CalculatorInput.N3:
        case CalculatorInput.N4:
        case CalculatorInput.N5:
        case CalculatorInput.N6:
        case CalculatorInput.N7:
        case CalculatorInput.N8:
        case CalculatorInput.N9:
          if (!(ParsedExpression is null) && ParsedExpression.ExtractLastTokenText() == "0") {
            // If there is only one "0" in the current number, and the next input is a non-zero
            // digit, deletes the "0" before appending the next digit.
            Backspace();
          }
          Append(input);
          break;
        case CalculatorInput.Add:
        case CalculatorInput.Sub:
        case CalculatorInput.Mul:
        case CalculatorInput.Div:
          // If starting an expression with an operator, a leading "0" is added. Although "+" and
          // "-" can also be treated as positive/negative operators, we follow this logic to make
          // the expression clearer.
          if (ParsedExpression is null) {
            Append(CalculatorInput.N0);
          }
          Append(input);
          break;
        default:
          Append(input);
          break;
      }
      if (State.IsOk()) {
        // Parses the input and updates ParsedExpression.
        Parse();
      }
    }

    // Resets the calculator state to OK.
    private void ResetState() {
      State = CalculatorState.Okay;
    }

    // Clears the input buffer and the tokens for display.
    private void AllClear() {
      _cachedExpression.Clear();
      ParsedExpression = null;
      Result = null;
    }

    // Appends the input character to the end of the buffer.
    private void Append(string input) {
      _cachedExpression.Append(input);
      if (_cachedExpression.Length > _maxChars) {
        State = CalculatorState.Overflow;
      }
    }

    // Removes the last character from the buffer.
    private void Backspace() {
      if (_cachedExpression.Length > 0) {
        _cachedExpression.Remove(_cachedExpression.Length - 1, 1);
      }
    }

    // Parses the input string to an expression.
    private void Parse() {
      var syntaxTokens = ParseExpression(out string expression);
      if (syntaxTokens is null) {
        ParsedExpression = null;
      } else {
        ParsedExpression = new ParsedExpression(expression, syntaxTokens, null, false);
      }
    }

    private IReadOnlyList<SyntaxToken> ParseExpression(out string expression) {
      expression = _cachedExpression.ToString();
      if (string.IsNullOrEmpty(expression)) {
        return null;
      } else {
        return Executor.ParseSyntaxTokens(expression, _moduleName, SeedXLanguage.SeedCalc, null);
      }
    }

    // Executes the current expression.
    private void Execute() {
      var syntaxTokens = ParseExpression(out string expression);
      if (syntaxTokens is null) {
        ParsedExpression = null;
        return;
      }
      var executor = new Executor();
      var visualizer = new Visualizer();
      executor.Register(visualizer);
      var collection = new DiagnosticCollection();
      if (!executor.Run(expression,
                        _moduleName,
                        SeedXLanguage.SeedCalc,
                        RunType.Ast,
                        collection)) {
        Debug.Assert(collection.Diagnostics.Count > 0);
        switch (collection.Diagnostics[0].MessageId) {
          case Message.RuntimeErrorDivideByZero:
            State = CalculatorState.DivBy0;
            break;
          case Message.RuntimeOverflow:
            State = CalculatorState.Overflow;
            break;
          default:
            State = CalculatorState.Syntax;
            break;
        }
        executor.Unregister(visualizer);
      } else {
        executor.Unregister(visualizer);
        if (CalculationMode == CalculationMode.CalculateImmediately) {
          ParsedExpression =
              new ParsedExpression(expression, syntaxTokens, null, false);
          Result = visualizer.FinalResult;
        } else if (CalculationMode == CalculationMode.DemoCalculationSteps) {
          var coroutine = CalculationSteps(executor,
                                           expression,
                                           syntaxTokens,
                                           visualizer.StepStates,
                                           visualizer.FinalResult);
          _gameManager.StartCoroutine(coroutine);
        }
      }
    }

    private IEnumerator CalculationSteps(Executor executor,
                                         string expression,
                                         IReadOnlyList<SyntaxToken> syntaxTokens,
                                         IReadOnlyList<StepState> stepStates,
                                         double finalResult) {
      State = CalculatorState.Calculating;
      while (stepStates.Count > 0) {
        var highlightedRange = stepStates[0].SourceRange;
        for (int i = 0; i < _calcAnimBlinkTimes; i++) {
          // Shows the highlighted expression.
          ParsedExpression =
              new ParsedExpression(expression, syntaxTokens, highlightedRange, true);
          yield return new WaitForSeconds(_calcAnimBlinkInterval);
          // Shows the un-highlighted expression.
          ParsedExpression = new ParsedExpression(expression, syntaxTokens, null, true);
          yield return new WaitForSeconds(_calcAnimBlinkInterval);
        }
        yield return new WaitForSeconds(_calcAnimInterval);
        // Replaces the calculated part with the intermediate step result.
        expression = UpdateExpressionWithStepResult(expression,
                                                    highlightedRange,
                                                    stepStates[0].Result);
        // The new expression has to be re-parsed and re-calculated.
        var visualizer = new Visualizer();
        syntaxTokens = Executor.ParseSyntaxTokens(expression,
                                                  _moduleName,
                                                  SeedXLanguage.SeedCalc,
                                                  null);
        executor.Register(visualizer);
        executor.Run(expression, _moduleName, SeedXLanguage.SeedCalc, RunType.Ast, null);
        stepStates = visualizer.StepStates;
        executor.Unregister(visualizer);
        // Shows the new expression.
        ParsedExpression = new ParsedExpression(expression, syntaxTokens, null, true);
        if (stepStates.Count > 0) {
          yield return new WaitForSeconds(_calcAnimInterval);
        }
      }
      // UpdateExpressionWithStepResult reformats intermediate results so that the calculation of
      // each step might accumulate precision errors. Thus for the final step, the expression must
      // be reset back to the original final result.
      expression = NumberFormatter.Format(finalResult);
      syntaxTokens = Executor.ParseSyntaxTokens(expression,
                                                _moduleName,
                                                SeedXLanguage.SeedCalc,
                                                null);
      ParsedExpression = new ParsedExpression(expression, syntaxTokens, null, false);
      // Updates the input buffer since the expression could change during the calculation steps.
      _cachedExpression.Clear();
      _cachedExpression.Append(expression);
      // Shows the final result.
      Result = finalResult;
      State = CalculatorState.Okay;
    }

    private string UpdateExpressionWithStepResult(string originalExpression,
                                                  TextRange range,
                                                  double result) {
      return originalExpression.Substring(0, range.Start.Column) +
          NumberFormatter.Format(result) +
          originalExpression.Substring(range.End.Column + 1,
                                       originalExpression.Length - range.End.Column - 1);
    }
  }
}
