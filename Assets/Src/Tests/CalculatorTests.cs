using NUnit.Framework;

public class CalculatorTests {
  [Test]
  public void TestCalculator() {
    var calculator = new Calculator();
    Assert.True(calculator.State.IsOk());
    calculator.OnInput("1");
    Assert.AreEqual("1", calculator.Content);
    calculator.OnInput("+");
    Assert.AreEqual("1+", calculator.Content);
    calculator.OnInput("2");
    Assert.AreEqual("1+2", calculator.Content);
    Assert.True(calculator.State.IsOk());
    calculator.OnInput("AC");
    Assert.AreEqual("", calculator.Content);
    Assert.True(calculator.State.IsOk());

    for (int i = 0; i < 100; i++) {
      calculator.OnInput("9");
    }
    Assert.True(calculator.State.IsOk());
    calculator.OnInput("9");
    Assert.True(calculator.State.IsError());
    Assert.AreEqual(CalculatorState.Overflow, calculator.State);
  }
}
