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
  // The nav bar to show the current level of the space grid.
  public class Nav : MonoBehaviour {
    public Sprite[] mSprites;

    private Text _scaleMarker;

    public void SetNavLevel(int level) {
      Debug.Assert(level >= CuttingBoard.MinLevel && level <= CuttingBoard.MaxLevel);
      GetComponent<Image>().sprite = mSprites[level + -CuttingBoard.MinLevel];
      _scaleMarker.text = $"1E{level + 1}";
    }

    public void Show(bool visible) {
      gameObject.SetActive(visible);
    }

    void Start() {
      _scaleMarker = transform.Find("Scale").Find("ScaleMarker").gameObject.GetComponent<Text>();
    }
  }
}
