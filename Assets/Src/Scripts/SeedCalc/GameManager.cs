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
using UnityEngine;
using AgileMvvm;

namespace SeedCalc {
  public class GameManager : MonoBehaviour {
    private Calculator _calculator = null;

    // The UI component of the calculator screen must be associated to this field in Unity.
    public CalculatorScreen Screen;
    public CuttingBoard CuttingBoard;

    public void OnClickButton(string input) {
      if (_calculator.AcceptingInput) {
        GetComponent<AudioSource>().Play();
        _calculator.OnInput(input);
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
    }
  }
}
