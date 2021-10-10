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

using NUnit.Framework;

namespace SeedCalc.Tests {
  public class CalculatorTests {
    [Test]
    public void TestCalculator() {
      var calculator = new Calculator();
      Assert.True(calculator.State.IsOk());

      calculator.OnInput("1");
      calculator.OnInput("=");
      Assert.AreEqual("1", calculator.DisplayContent.Expression);
      Assert.True(calculator.State.IsOk());
      calculator.OnInput("AC");
      Assert.IsNull(calculator.DisplayContent);
      Assert.True(calculator.State.IsOk());

      calculator.OnInput("1");
      Assert.AreEqual("1", calculator.DisplayContent.Expression);
      calculator.OnInput("+");
      Assert.AreEqual("1+", calculator.DisplayContent.Expression);
      calculator.OnInput("2");
      Assert.AreEqual("1+2", calculator.DisplayContent.Expression);
      Assert.True(calculator.State.IsOk());
      calculator.OnInput("=");
      Assert.AreEqual("3", calculator.DisplayContent.Expression);
      Assert.True(calculator.State.IsOk());
      calculator.OnInput("AC");

      calculator.OnInput("1");
      calculator.OnInput("+");
      calculator.OnInput("=");
      Assert.True(calculator.State.IsError());
      Assert.AreEqual(CalculatorState.Syntax, calculator.State);
      calculator.OnInput("AC");

      for (int i = 0; i < 100; i++) {
        calculator.OnInput("9");
      }
      Assert.True(calculator.State.IsOk());
      calculator.OnInput("9");
      Assert.True(calculator.State.IsError());
      Assert.AreEqual(CalculatorState.Overflow, calculator.State);
    }
  }
}
