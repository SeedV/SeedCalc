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
        yield return new TestFixtureData(1e-10, "0.0000000001");
        yield return new TestFixtureData(1.1e-10, "0.00000000011");
        yield return new TestFixtureData(1, "1");
        yield return new TestFixtureData(3.14, "3.14");
        yield return new TestFixtureData(31.4, "31.4");
        yield return new TestFixtureData(100, "100");
        yield return new TestFixtureData(99999, "99999");
        yield return new TestFixtureData(3.14159265358979e+9, "3141592653.59");
        yield return new TestFixtureData(9.999e+9, "9999000000");
        yield return new TestFixtureData(1e+10, "10000000000");
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
      Assert.AreEqual(_result, NumberFormatter.Format(_value, 13));
    }
  }
}
