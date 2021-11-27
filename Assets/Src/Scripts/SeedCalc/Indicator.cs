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
using UnityEngine;
using UnityEngine.UI;

namespace SeedCalc {
  public class Indicator : MonoBehaviour {
    private const float _fullHeight = 1440f;

    public Text IndicatorValue;

    private double _value;
    private string _localizedUnitString;

    public double Value {
      get => _value;
      set {
        if (VisualizableNumbers.TryFormatVisualizableValue(value, out string s)) {
          IndicatorValue.gameObject.SetActive(true);
          gameObject.SetActive(true);
          IndicatorValue.text = s + _localizedUnitString;
          transform.localScale = new Vector3(1f, (float)value / 100.0f, 1f);
          _value = value;
        } else {
          _value = 0;
          IndicatorValue.gameObject.SetActive(false);
          gameObject.SetActive(false);
        }
      }
    }

    void Start() {
      // TODO: localize this unit string via separating the unit string from the value string.
      _localizedUnitString = "<size=40>m</size>";
      StartCoroutine(AnimateAllLevels(.2f));
    }

    private IEnumerator AnimateAllLevels(float intervalInSeconds) {
      for (double v = 1; v < 100; v += 1.5) {
        Value = v;
        yield return new WaitForSeconds(intervalInSeconds);
      }
      Value = 9.0506070809;
    }
  }
}
