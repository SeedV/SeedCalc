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
  public class LevelConfigsFixtureData {
    public static IEnumerable FixtureParams {
      get {
        yield return new TestFixtureData(1e-11, false, -1);
        yield return new TestFixtureData(1e-10, true, 0);
        yield return new TestFixtureData(1.1e-10, true, 0);
        yield return new TestFixtureData(1, true, 10);
        yield return new TestFixtureData(99999, true, 15);
        yield return new TestFixtureData(1e+10, true, 20);
        yield return new TestFixtureData(1e+11, false, -1);
      }
    }
  }

  [TestFixtureSource(typeof(LevelConfigsFixtureData),
                     nameof(LevelConfigsFixtureData.FixtureParams))]
  public class LevelConfigsTests {
    private double _value;
    private bool _visualizable;
    private int _level;

    public LevelConfigsTests(double value, bool visualizable, int level) {
      _value = value;
      _visualizable = visualizable;
      _level = level;
    }

    [Test]
    public void TestLevelConfigs() {
      Assert.AreEqual(_visualizable, LevelConfigs.IsVisualizable(_value));
      Assert.AreEqual(_level, LevelConfigs.MapNumberToLevel(_value));
    }
  }
}
