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

using System.Collections;
using NUnit.Framework;

namespace SeedCalc.Tests {
  public class NumberFormatterFixtureData {
    public static IEnumerable FixtureParams {
      get {
        yield return new TestFixtureData(-98765432109876, "-9.8765E+013");
        yield return new TestFixtureData(-1234567890123, "-1.2346E+012");
        yield return new TestFixtureData(-333, "-333");
        yield return new TestFixtureData(-0.000001, "-0.000001");
        yield return new TestFixtureData(0, "0");
        yield return new TestFixtureData(0.9e-10, "9.0000E-011");
        yield return new TestFixtureData(1e-10, "0.0000000001");
        yield return new TestFixtureData(1.1e-10, "0.0000000001");
        yield return new TestFixtureData(1, "1");
        yield return new TestFixtureData(3.14, "3.14");
        yield return new TestFixtureData(31.4, "31.4");
        yield return new TestFixtureData(100, "100");
        yield return new TestFixtureData(99999, "99999");
        yield return new TestFixtureData(3.14159265358979e+9, "3141592653.6");
        yield return new TestFixtureData(9.999e+9, "9999000000");
        yield return new TestFixtureData(10000000000, "10000000000");
        yield return new TestFixtureData(10000000000.001, "10000000000");
        yield return new TestFixtureData(60000000000, "60000000000");
        yield return new TestFixtureData(60000000000.00001, "6.0000E+010");
        yield return new TestFixtureData(98765432109876, "9.8765E+013");
      }
    }
  }

  [TestFixtureSource(typeof(NumberFormatterFixtureData),
                     nameof(NumberFormatterFixtureData.FixtureParams))]
  public class NumberFormatterTests {
    private double _value;
    private string _result;

    public NumberFormatterTests(double value, string result) {
      _value = value;
      _result = result;
    }

    [Test]
    public void TestRangeAndFormatter() {
      string formatted = NumberFormatter.Format(_value);
      Assert.AreEqual(_result, formatted);
    }
  }
}
