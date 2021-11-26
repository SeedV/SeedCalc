using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SeedCalc {
  public class Nav : MonoBehaviour {
    public Sprite[] mSprites;

    private const int _minLevel = -11;
    private const int _maxLevel = 10;
    private const int _defaultLevel = 0;

    public void SetNavLevel(int level) {
      Debug.Assert(level >= _minLevel && level <= _maxLevel);
      GetComponent<Image>().sprite = mSprites[level + -_minLevel];
    }

    void Start() {
      StartCoroutine(AnimateAllLevels(.05f));
    }

    private IEnumerator AnimateAllLevels(float intervalInSeconds) {
      for (int i = _minLevel; i <= _maxLevel; i++) {
        SetNavLevel(i);
        yield return new WaitForSeconds(intervalInSeconds);
      }
      SetNavLevel(_defaultLevel);
    }
  }
}
