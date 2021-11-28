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
  // The cutting board to show visualizable numbers and reference objects.
  public class CuttingBoard : MonoBehaviour {
    public const int MinLevel = -11;
    public const int MaxLevel = 9;

    private static readonly Color _activeColor = new Color(.8f, .8f, .8f);
    private static readonly Color _inactiveColor = new Color(.14f, .27f, .26f);

    public Texture RainbowTexture;
    public GameObject LightingMask;
    public Nav Nav;
    public Indicator Indicator;

    private bool _active = false;
    private int _level = 0;

    public bool Active {
      get => _active;
      set {
        GetComponent<Renderer>().material.mainTexture = value ? RainbowTexture : null;
        GetComponent<Renderer>().material.color =  value ? _activeColor : _inactiveColor;
        LightingMask.SetActive(value);
        Nav.Show(value);
        Indicator.Show(value);
        _active = value;
      }
    }

    public int Level {
      get => _level;
      set {
        SetLevel(value);
        _level = value;
      }
    }

    void Start() {
      // This is only for demonstrating UI design. Will be removed from production code.
      StartCoroutine(AnimateAllLevels(1f));
    }

    // This is only for demonstrating UI design. Will be removed from production code.
    private IEnumerator AnimateAllLevels(float intervalInSeconds) {
      Active = true;
      for (int i = MinLevel; i <= MaxLevel; i++) {
        Level = i;
        yield return new WaitForSeconds(intervalInSeconds);
      }
      Level = 0;
      Active = false;
    }

    private void SetLevel(int level) {
      ScrollRainbowTo(level);
      Nav.SetNavLevel(level);
    }

    private void ScrollRainbowTo(int level) {
      Debug.Assert(level >= MinLevel && level <= MaxLevel);
      float texOffsetX = (1f / (MaxLevel - MinLevel)) * (level - MinLevel + 1);
      GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(texOffsetX, 0));
    }
  }
}