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

namespace SeedCalc {
  public class CuttingBoard : MonoBehaviour {
    private const int _numLevel = 20;
    private static readonly Color _onColor = new Color(.8f, .8f, .8f);
    private static readonly Color _offColor = new Color(.14f, .27f, .26f);

    public Texture RainbowTexture;
    public GameObject _lightingMask;

    public void TurnOn(int level) {
      _lightingMask.SetActive(true);
      GetComponent<Renderer>().material.mainTexture = RainbowTexture;
      GetComponent<Renderer>().material.color = _onColor;
      ScrollRainbowTo(level);
    }

    public void TurnOff() {
      _lightingMask.SetActive(false);
      GetComponent<Renderer>().material.color = _offColor;
      GetComponent<Renderer>().material.mainTexture = null;
    }

    void Start() {
      StartCoroutine(AnimateAllLevels(.2f));
    }

    private IEnumerator AnimateAllLevels(float intervalInSeconds) {
      TurnOn(0);
      for (int i = 0; i < _numLevel; i++) {
        ScrollRainbowTo(i);
        yield return new WaitForSeconds(intervalInSeconds);
      }
      TurnOff();
    }

    private void ScrollRainbowTo(int level) {
      Debug.Assert(level >= 0 && level < _numLevel);
      float texOffsetX = 0.05f * (level + 1);
      GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(texOffsetX, 0));
    }
  }
}
