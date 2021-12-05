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

using System.Collections;
using NUnit.Framework;

namespace SeedCalc.Tests {
  public class FixtureData {
    public static IEnumerable FixtureParams {
      get {
        yield return new TestFixtureData(-0.1, false, null);
        yield return new TestFixtureData(0, false, null);
        yield return new TestFixtureData(9.999e-11, false, null);
        yield return new TestFixtureData(1e+11, false, null);
        yield return new TestFixtureData(1e-10, true, "0.0000000001");
        yield return new TestFixtureData(1.1e-10, true, "0.00000000011");
        yield return new TestFixtureData(1, true, "1");
        yield return new TestFixtureData(3.14, true, "3.14");
        yield return new TestFixtureData(31.4, true, "31.4");
        yield return new TestFixtureData(100, true, "100");
        yield return new TestFixtureData(99999, true, "99999");
        yield return new TestFixtureData(3.14159265358979e+9, true, "3141592653.59");
        yield return new TestFixtureData(9.999e+9, true, "9999000000");
        yield return new TestFixtureData(1e+10, true, "10000000000");
      }
    }
  }

  [TestFixtureSource(typeof(FixtureData), nameof(FixtureData.FixtureParams))]
  public class VisualizableNumbersTests {
    private double _value;
    private bool _test;
    private string _result;

    public VisualizableNumbersTests(double value, bool test, string result) {
      _value = value;
      _test = test;
      _result = result;
    }

    [Test]
    public void TestRangeAndFormatter() {
      Assert.AreEqual(_test, VisualizableNumber.IsVisualizable(_value));
      if (_test) {
        Assert.AreEqual(_result, VisualizableNumber.Format(_value));
      }
    }
  }
}
