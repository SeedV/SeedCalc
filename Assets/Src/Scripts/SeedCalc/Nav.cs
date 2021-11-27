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
