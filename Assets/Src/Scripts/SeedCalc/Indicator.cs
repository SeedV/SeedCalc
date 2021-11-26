using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SeedCalc {
  public class Indicator : MonoBehaviour {
    private const int _minLevel = -11;
    private const int _maxLevel = 10;
    private const int _defaultLevel = 0;
    private const float _fullHeight = 1440f;

    public Text IndicatorValue;

    private float _value;

    public float Value {
      get => _value;
      set {
        _value = value;
        IndicatorValue.text = value.ToString();
        transform.localScale = new Vector3(1f, value / 100.0f, 1f);
      }
    }

    void Start() {
      StartCoroutine(AnimateAllLevels(.05f));
    }

    private IEnumerator AnimateAllLevels(float intervalInSeconds) {
      for (float f = 1f; f < 100f; f += 1.5f) {
        Value = f;
        yield return new WaitForSeconds(intervalInSeconds);
      }
      Value = 100f;
    }
  }
}
