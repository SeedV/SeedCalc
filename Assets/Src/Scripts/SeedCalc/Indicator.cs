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
using TMPro;
using UnityEngine;

namespace SeedCalc {
  // The indicator bar and its display value to show the current visualizable number.
  public class Indicator : MonoBehaviour {
    private const float _fullHeight = 1440f;
    private const float _canvasHeight = 2048f;
    private const float _indicatorValueOriginY = -210f;
    private const float _indicatorUnitSize = 45f;
    private const string _wireLightAnimOnTrigger = "Active";
    private const string _wireLightAnimOffTrigger = "Inactive";
    private const float _delayAfterWireLightAnim = 0f;

    private bool _visible = false;

    public GameObject WireLightSprite;
    public TextMeshProUGUI IndicatorValue;

    public void SetValue(double maxValueOfCurrentLevel, double value) {
      Debug.Assert(maxValueOfCurrentLevel > 0 && value > 0 && value <= maxValueOfCurrentLevel);
      IndicatorValue.text = NumberFormatter.Format(value);
      float percentage = GetPercentage(maxValueOfCurrentLevel, value);
      transform.localScale = new Vector3(1f, percentage, 1f);
      HideIndicatorValue();
      Visible = true;
      // The indicator value's UI element uses ContentFilterSize to auto-adjust its size. When the
      // text content is set, the rect size will not be updated until the next frame. Thus we have
      // to wait for the next frame to calculate and set the position of the indicator value.
      StartCoroutine(PositionIndicatorValueInNextFrameCoroutine(percentage));
    }

    public IEnumerator SetValueWithAnim(double maxValueOfCurrentLevel, double value) {
      Debug.Assert(maxValueOfCurrentLevel > 0 && value > 0 && value <= maxValueOfCurrentLevel);
      IndicatorValue.text = NumberFormatter.Format(value);
      float percentage = GetPercentage(maxValueOfCurrentLevel, value);
      Visible = false;
      // If there is still a previous anim being played, this yield line will break it.
      yield return null;
      HideIndicatorValue();
      Visible = true;
      transform.localScale = new Vector3(1f, 0f, 1f);
      WireLightSprite.GetComponent<Animator>().SetTrigger(_wireLightAnimOnTrigger);
      yield return new WaitForSeconds(_delayAfterWireLightAnim);
      float velocity = 0;
      float scale = 0;
      while (!MathUtils.EqualsApproximately(scale, percentage, 0.005f)) {
        // The indicator's animation uses the same TransitionSmoothTime as the cutting board's scale
        // transition animation.
        scale =
            Mathf.SmoothDamp(scale, percentage, ref velocity, CuttingBoard.TransitionSmoothTime);
        transform.localScale = new Vector3(1f, scale, 1f);
        if (!_visible) {
          // Stops the animation immediately if the indicator is turned off by the outter loop.
          WireLightSprite.GetComponent<Animator>().SetTrigger(_wireLightAnimOffTrigger);
          yield break;
        }
        yield return null;
      }
      transform.localScale = new Vector3(1f, percentage, 1f);
      WireLightSprite.GetComponent<Animator>().SetTrigger(_wireLightAnimOffTrigger);
      yield return null;
      PositionIndicatorValueInNextFrame(percentage);
    }

    public bool Visible {
      get => _visible;
      set {
        IndicatorValue.gameObject.SetActive(value);
        gameObject.gameObject.SetActive(value);
        if (value) {
          LocalizationUtils.UpdateLocalizedAssets(IndicatorValue.gameObject, true);
        }
        _visible = value;
      }
    }

    private float GetPercentage(double maxValueOfCurrentLevel, double value) {
      return (float)(value / maxValueOfCurrentLevel);
    }

    private void HideIndicatorValue() {
      var indicatorValueRect = IndicatorValue.GetComponent<RectTransform>();
      float indicatorValueX = indicatorValueRect.anchoredPosition.x;
      indicatorValueRect.anchoredPosition = new Vector2(indicatorValueX, -_canvasHeight);
    }

    private IEnumerator PositionIndicatorValueInNextFrameCoroutine(float percentage) {
      yield return new WaitForEndOfFrame();
      PositionIndicatorValueInNextFrame(percentage);
    }

    private void PositionIndicatorValueInNextFrame(float percentage) {
      var indicatorValueRect = IndicatorValue.GetComponent<RectTransform>();
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
