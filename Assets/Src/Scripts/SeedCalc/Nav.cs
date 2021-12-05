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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SeedCalc {
  // The nav bar to show the current level of the space grid.
  public class Nav : MonoBehaviour {
    // The nav bar uses a different level system compared with the main cutting board, so that there
    // is flexibility to map more than one cutting board levels to the same nav bar level. The
    // rainbow texture also follows the nav bar level, each nav bar level corresponding to a unique
    // rainbow color section.
    public const int MinLevel = -11;
    public const int MaxLevel = 9;
    public const int DefaultLevel = 0;
    public const string DefaultMarkerValueString = "0.5";

    public Sprite[] Sprites;

    private TextMeshProUGUI _scaleMarker;

    public void SetNavLevel(int navLevel, string scaleMarkerValueString) {
      Debug.Assert(navLevel >= MinLevel && navLevel <= MaxLevel);
      GetComponent<Image>().sprite = Sprites[navLevel - MinLevel];
      _scaleMarker.text = scaleMarkerValueString;
    }

    public void Show(bool visible) {
      gameObject.SetActive(visible);
    }

    void Start() {
      var t = transform.Find("Scale").Find("ScaleMarker");
      Debug.Assert(!(t is null));
      _scaleMarker = t.gameObject.GetComponent<TextMeshProUGUI>();
    }
  }
}
