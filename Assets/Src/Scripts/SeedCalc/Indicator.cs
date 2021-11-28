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
  // The indicator bar and its display value to show the current visualizable number.
  public class Indicator : MonoBehaviour {
    private const float _fullHeight = 1440f;
    private const float _canvasHeight = 2048f;
    private const float _indicatorValueOriginY = -210f;
    private const float _indicatorUnitSize = 45f;

    public Text IndicatorValue;

    public void UpdateValue(double maxValueOfCurrentLevel, double value) {
      Debug.Assert(maxValueOfCurrentLevel > 0 && value > 0 && value <= maxValueOfCurrentLevel);
      if (VisualizableNumbers.TryFormatVisualizableValue(value, out string s)) {
        IndicatorValue.text = s;
        float percentage = (float)(value / maxValueOfCurrentLevel);
        transform.localScale = new Vector3(1f, percentage, 1f);
        // Hides the indicator value until it's correctly positioned in the next frame.
        var indicatorValueRect = IndicatorValue.GetComponent<RectTransform>();
        float indicatorValueX = indicatorValueRect.anchoredPosition.x;
        indicatorValueRect.anchoredPosition = new Vector2(indicatorValueX, -_canvasHeight);
        // The indicator value UI.Text uses ContentFilterSize to auto-adjust its size. When the text
        // content is set, the rect size will not be updated until the next frame. Thus we have to
        // wait for the next frame to calculate and set the position of the indicator value.
        StartCoroutine(PositionIndicatorValueInNextFrame(percentage, indicatorValueRect));
      }
    }

    public void Show(bool visible) {
      IndicatorValue.gameObject.SetActive(visible);
      gameObject.gameObject.SetActive(visible);
    }

    void Start() {
      // This is only for demonstrating UI design. Will be removed from production code.
      StartCoroutine(AnimateAllLevels(1f));
    }

    // This is only for demonstrating UI design. Will be removed from production code.
    private IEnumerator AnimateAllLevels(float intervalInSeconds) {
      yield return new WaitForEndOfFrame();
      for (double value = 1.0; value < 9.0; value += 0.34788) {
        UpdateValue(10.0, value);
        yield return new WaitForSeconds(intervalInSeconds);
      }
      UpdateValue(10.0, 9.0506070809);
    }

    private IEnumerator PositionIndicatorValueInNextFrame(
        float percentage, RectTransform indicatorValueRect) {
      // Waits for the next frame.
      yield return new WaitForEndOfFrame();
      float indicatorHeight = _fullHeight * percentage;
      float indicatorValueWidth = indicatorValueRect.sizeDelta.x;
      float indicatorValueX = indicatorValueRect.anchoredPosition.x;
      float indicatorValueY = _indicatorValueOriginY;
      if (indicatorValueWidth + _indicatorUnitSize <= indicatorHeight) {
        // If the value text is shorter than the indicator bar, the text (as rotated by 90 degree)
        // will be left-aligned to the top end of the bar.
        indicatorValueY -= _fullHeight - indicatorHeight;
      } else {
        // If the value text is longer than the indicator bar, the text (as rotated by 90 degree)
        // will be right-aligned to the bottom end of the bar.
        indicatorValueY -= _fullHeight - indicatorValueWidth - _indicatorUnitSize;
      }
      indicatorValueRect.anchoredPosition = new Vector2(indicatorValueX, indicatorValueY);
    }
  }
}
