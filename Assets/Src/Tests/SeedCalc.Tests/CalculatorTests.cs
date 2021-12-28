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

using NUnit.Framework;

namespace SeedCalc.Tests {
  public class CalculatorTests {
    [Test]
    public void TestCalculator() {
      var calculator = new Calculator(CalculationMode.CalculateImmediately, null);
      Assert.True(calculator.State.IsOk());

      calculator.OnInput("1");
      calculator.OnInput("=");
      Assert.AreEqual("1", calculator.ParsedExpression.Expression);
      Assert.True(calculator.State.IsOk());
      calculator.OnInput("AC");
      Assert.Null(calculator.ParsedExpression);
      Assert.True(calculator.State.IsOk());

      calculator.OnInput("1");
      Assert.AreEqual("1", calculator.ParsedExpression.Expression);
      calculator.OnInput("+");
      Assert.AreEqual("1+", calculator.ParsedExpression.Expression);
      calculator.OnInput("2");
      Assert.AreEqual("1+2", calculator.ParsedExpression.Expression);
      Assert.True(calculator.State.IsOk());
      calculator.OnInput("=");
      Assert.AreEqual("1+2", calculator.ParsedExpression.Expression);
      Assert.AreEqual(3, calculator.Result);
      Assert.True(calculator.State.IsOk());
    }

    [Test]
    public void TestDel() {
      var calculator = new Calculator(CalculationMode.CalculateImmediately, null);
      calculator.OnInput("1");
      calculator.OnInput("Del");
      calculator.OnInput("Del");
      Assert.Null(calculator.ParsedExpression);
      calculator.OnInput("1");
      calculator.OnInput("2");
      calculator.OnInput("3");
      calculator.OnInput("(");
      calculator.OnInput(")");
      calculator.OnInput("(");
      calculator.OnInput(")");
      calculator.OnInput("Del");
      calculator.OnInput("Del");
      Assert.AreEqual("123()", calculator.ParsedExpression.Expression);
    }

    [Test]
    public void TestDot() {
      var calculator = new Calculator(CalculationMode.CalculateImmediately, null);
      calculator.OnInput(".");
      Assert.AreEqual("0.", calculator.ParsedExpression.Expression);
      calculator.OnInput(".");
      calculator.OnInput(".");
      calculator.OnInput(".");
      Assert.AreEqual("0.", calculator.ParsedExpression.Expression);
      calculator.OnInput("3");
      Assert.AreEqual("0.3", calculator.ParsedExpression.Expression);
      calculator.OnInput("AC");
      calculator.OnInput("3");
      calculator.OnInput(".");
      calculator.OnInput(".");
      calculator.OnInput("5");
      calculator.OnInput("+");
      calculator.OnInput(".");
      Assert.AreEqual("3.5+0.", calculator.ParsedExpression.Expression);
      calculator.OnInput(".");
      Assert.AreEqual("3.5+0.", calculator.ParsedExpression.Expression);
      calculator.OnInput("5");
      Assert.AreEqual("3.5+0.5", calculator.ParsedExpression.Expression);
      calculator.OnInput("=");
      Assert.AreEqual("3.5+0.5", calculator.ParsedExpression.Expression);
      Assert.AreEqual(4, calculator.Result);
      Assert.True(calculator.State.IsOk());
    }

    [Test]
    public void TestZero() {
      var calculator = new Calculator(CalculationMode.CalculateImmediately, null);
      calculator.OnInput("0");
      Assert.AreEqual("0", calculator.ParsedExpression.Expression);
      calculator.OnInput("0");
      Assert.AreEqual("0", calculator.ParsedExpression.Expression);
      calculator.OnInput("00");
      Assert.AreEqual("0", calculator.ParsedExpression.Expression);
      calculator.OnInput("00");
      Assert.AreEqual("0", calculator.ParsedExpression.Expression);
      calculator.OnInput("3");
      Assert.AreEqual("3", calculator.ParsedExpression.Expression);
      calculator.OnInput("+");
      calculator.OnInput("0");
      Assert.AreEqual("3+0", calculator.ParsedExpression.Expression);
      calculator.OnInput("3");
      Assert.AreEqual("3+3", calculator.ParsedExpression.Expression);
      calculator.OnInput("0");
      Assert.AreEqual("3+30", calculator.ParsedExpression.Expression);
      calculator.OnInput("0");
      Assert.AreEqual("3+300", calculator.ParsedExpression.Expression);
      calculator.OnInput("00");
      Assert.AreEqual("3+30000", calculator.ParsedExpression.Expression);
      Assert.True(calculator.State.IsOk());
      calculator.OnInput("AC");
      calculator.OnInput("00");
      Assert.AreEqual("0", calculator.ParsedExpression.Expression);
      calculator.OnInput("AC");
      calculator.OnInput("3");
      calculator.OnInput("*");
      calculator.OnInput("00");
      Assert.AreEqual("3*0", calculator.ParsedExpression.Expression);
      calculator.OnInput("3");
      Assert.AreEqual("3*3", calculator.ParsedExpression.Expression);
      calculator.OnInput("AC");
      calculator.OnInput("00");
      calculator.OnInput("0");
      calculator.OnInput("*");
      calculator.OnInput("3");
      Assert.AreEqual("0*3", calculator.ParsedExpression.Expression);
      // Auto-added "0" for operators at the beginning.
      calculator.OnInput("AC");
      calculator.OnInput("*");
      Assert.AreEqual("0*", calculator.ParsedExpression.Expression);
      calculator.OnInput("3");
      Assert.AreEqual("0*3", calculator.ParsedExpression.Expression);
      calculator.OnInput("=");
      Assert.AreEqual(0, calculator.Result);
      calculator.OnInput("AC");
      calculator.OnInput("/");
      Assert.AreEqual("0/", calculator.ParsedExpression.Expression);
      calculator.OnInput("3");
      Assert.AreEqual("0/3", calculator.ParsedExpression.Expression);
      calculator.OnInput("=");
      Assert.AreEqual(0, calculator.Result);
    }

    [Test]
    public void TestPositiveAndNegative() {
      var calculator = new Calculator(CalculationMode.CalculateImmediately, null);
      calculator.OnInput("+");
      calculator.OnInput("3");
      Assert.AreEqual("0+3", calculator.ParsedExpression.Expression);
      calculator.OnInput("=");
      Assert.AreEqual(3, calculator.Result);
      calculator.OnInput("AC");
      calculator.OnInput("-");
      calculator.OnInput("3");
      Assert.AreEqual("0-3", calculator.ParsedExpression.Expression);
      calculator.OnInput("=");
      Assert.AreEqual(-3, calculator.Result);
      calculator.OnInput("AC");
      calculator.OnInput("3");
      calculator.OnInput("+");
      calculator.OnInput("-");
      calculator.OnInput("3");
      Assert.AreEqual("3+-3", calculator.ParsedExpression.Expression);
      calculator.OnInput("=");
      Assert.AreEqual(0, calculator.Result);
      calculator.OnInput("AC");
      calculator.OnInput("3");
      calculator.OnInput("-");
      calculator.OnInput("+");
      calculator.OnInput("3");
      Assert.AreEqual("3-+3", calculator.ParsedExpression.Expression);
      calculator.OnInput("=");
      Assert.AreEqual(0, calculator.Result);
    }

    [Test]
    public void TestAfterCalculation() {
      var calculator = new Calculator(CalculationMode.CalculateImmediately, null);
      calculator.OnInput("3");
      calculator.OnInput("+");
      calculator.OnInput("3");
      calculator.OnInput("=");
      Assert.AreEqual("3+3", calculator.ParsedExpression.Expression);
      Assert.AreEqual(6, calculator.Result);
      // After a calculation, a new number input triggers a new expression.
      calculator.OnInput("0");
      Assert.AreEqual("0", calculator.ParsedExpression.Expression);
      calculator.OnInput("+");
      calculator.OnInput("8");
      calculator.OnInput("=");
      Assert.AreEqual("0+8", calculator.ParsedExpression.Expression);
      Assert.AreEqual(8, calculator.Result);
      // While other characters continue to append on to the existing expression.
      calculator.OnInput("*");
      Assert.AreEqual("0+8*", calculator.ParsedExpression.Expression);
      calculator.OnInput("3");
      Assert.AreEqual("0+8*3", calculator.ParsedExpression.Expression);
      calculator.OnInput("=");
      Assert.AreEqual(24, calculator.Result);
      // Del modifies the existing expression inline.
      calculator.OnInput("Del");
      Assert.AreEqual("0+8*", calculator.ParsedExpression.Expression);
      calculator.OnInput("Del");
      Assert.AreEqual("0+8", calculator.ParsedExpression.Expression);
      calculator.OnInput("Del");
      Assert.AreEqual("0+", calculator.ParsedExpression.Expression);
      calculator.OnInput("(");
      calculator.OnInput("8");
      calculator.OnInput("-");
      calculator.OnInput("3");
      calculator.OnInput(")");
      calculator.OnInput("=");
      Assert.AreEqual("0+(8-3)", calculator.ParsedExpression.Expression);
      Assert.AreEqual(5, calculator.Result);
    }

    [Test]
    public void TestSyntaxError() {
      var calculator = new Calculator(CalculationMode.CalculateImmediately, null);
      calculator.OnInput("1");
      calculator.OnInput("+");
      calculator.OnInput("=");
      Assert.True(calculator.State.IsError());
      Assert.AreEqual(CalculatorState.Syntax, calculator.State);
      calculator.OnInput("(");
      calculator.OnInput("(");
      calculator.OnInput(".");
      calculator.OnInput(")");
      Assert.True(calculator.State.IsError());
      Assert.AreEqual(CalculatorState.Syntax, calculator.State);
    }

    [Test]
    public void TestDivByZero() {
      var calculator = new Calculator(CalculationMode.CalculateImmediately, null);
      calculator.OnInput("1");
      calculator.OnInput("/");
      calculator.OnInput("0");
      calculator.OnInput("=");
      Assert.True(calculator.State.IsError());
      Assert.AreEqual(CalculatorState.DivBy0, calculator.State);
    }

    [Test]
    public void TestInputOverflow() {
      var calculator = new Calculator(CalculationMode.CalculateImmediately, null);
      for (int i = 0; i < 40; i++) {
        calculator.OnInput("9");
      }
      Assert.True(calculator.State.IsOk());
      calculator.OnInput("9");
      Assert.True(calculator.State.IsError());
      Assert.AreEqual(CalculatorState.Overflow, calculator.State);
    }
  }
}
