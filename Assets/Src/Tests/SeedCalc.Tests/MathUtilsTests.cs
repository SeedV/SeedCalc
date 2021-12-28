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
using UnityEngine;
using NUnit.Framework;

namespace SeedCalc.Tests {
  public class EqualsApproximatelyFloatFixtureData {
    public static IEnumerable FixtureParams {
      get {
        yield return new TestFixtureData(1f, 1.1f, 0.01f, false);
        yield return new TestFixtureData(1f, 1.1f, 0.2f, true);
        yield return new TestFixtureData(0f, 0.01f, 0.1f, true);
        yield return new TestFixtureData(0f, 0.01f, 0.001f, false);
      }
    }
  }

  [TestFixtureSource(typeof(EqualsApproximatelyFloatFixtureData),
                     nameof(EqualsApproximatelyFloatFixtureData.FixtureParams))]
  public class EqualsApproximatelyFloatTests {
    private float _a;
    private float _b;
    private float _epsilon;
    private bool _result;

    public EqualsApproximatelyFloatTests(float a, float b, float epsilon, bool result) {
      _a = a;
      _b = b;
      _epsilon = epsilon;
      _result = result;
    }

    [Test]
    public void TestEqualsApproximatelyFloat() {
      Assert.AreEqual(_result, MathUtils.EqualsApproximately(_a, _b, _epsilon));
    }
  }

  public class EqualsApproximatelyVector3FixtureData {
    public static IEnumerable FixtureParams {
      get {
        yield return new TestFixtureData(
            new Vector3(0, 0, 0), new Vector3(0.1f, 0.1f, 0.1f), 0.01f, false);
        yield return new TestFixtureData(
            new Vector3(0, 0, 0), new Vector3(0.001f, 0.001f, 0.001f), 0.01f, true);
      }
    }
  }

  [TestFixtureSource(typeof(EqualsApproximatelyVector3FixtureData),
                     nameof(EqualsApproximatelyVector3FixtureData.FixtureParams))]
  public class EqualsApproximatelyVector3Tests {
    private Vector3 _a;
    private Vector3 _b;
    private float _epsilon;
    private bool _result;

    public EqualsApproximatelyVector3Tests(Vector3 a, Vector3 b, float epsilon, bool result) {
      _a = a;
      _b = b;
      _epsilon = epsilon;
      _result = result;
    }

    [Test]
    public void TestEqualsApproximatelyVector3() {
      Assert.AreEqual(_result, MathUtils.EqualsApproximately(_a, _b, _epsilon));
    }
  }
}
