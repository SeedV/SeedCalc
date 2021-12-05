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
using System.Collections.Generic;
using UnityEngine;

namespace SeedCalc {
  // The cutting board to show visualizable numbers and reference objects.
  public class CuttingBoard : MonoBehaviour {
    private class SlideAnimConfig {
      public GameObject Actor;
      public Vector3 FromPosition;
      public Vector3 ToPosition;
      public Vector3 FromScale;
      public Vector3 ToScale;
    }

    // The active and inactive colors for the CuttingBoard material.
    private static readonly Color _activeColor = new Color(.6f, .6f, .6f);
    private static readonly Color _inactiveColor = new Color(.14f, .27f, .26f);
    // The initial x offset of the rainbow texture.
    private const float _rainbowTexInitOffsetX = 0.05f;
    // The numbers of large rows and columns of the grid.
    private const int _LargeCellRows = 4;
    private const int _LargeCellCols = 6;
    // The trigger name to play the active animation when user clicks or touches on the object.
    private const string _activeAnimTriggerName = "Active";
    // The nubmer of animation steps (frames) when transitioning to the neighbor level.
    private const int _transitionAnimSteps = 30;

    public GameObject RootOfRefObjs;
    public GameObject RootOfDescPanels;
    public Texture RainbowTexture;
    public GameObject LightingMask;
    public Nav Nav;
    public Indicator Indicator;

    private bool _active = false;
    private int _currentLevel = -1;
    // Queued numbers to be visualized.
    private readonly Queue<double> _numberQueue = new Queue<double>();
    // Map from the level index to the control object of every desc panel.
    private Dictionary<int, GameObject> _descPanels = new Dictionary<int, GameObject>();
    // Map from the reference object name to its container object and its own game object.
    private Dictionary<string, (GameObject Container, GameObject Obj)> _refObjs =
        new Dictionary<string, (GameObject Container, GameObject Obj)>();

    // Turns the cutting board on/off.
    public bool Active {
      get => _active;
      set {
        GetComponent<Renderer>().material.mainTexture = value ? RainbowTexture : null;
        GetComponent<Renderer>().material.color =  value ? _activeColor : _inactiveColor;
        LightingMask.SetActive(value);
        Nav.Show(value);
        Indicator.Show(value);
        ShowCurrentRefObjs(value);
        if (value && _currentLevel < 0) {
          Indicator.Show(false);
          Nav.SetNavLevel(Nav.DefaultLevel, Nav.DefaultMarkerValueString);
        }
        _active = value;
      }
    }

    // Queues a new number to be visualized. A transition between two visualization levels might not
    // be completed within one frame, thus a queue is used to hold the numbers to be visualized. A
    // coroutine LevelTransitionLoop keeps running to peek numbers from the queue and play the
    // transition animation for them.
    public void QueueNewNumber(double number) {
      // No thread-safe protection is considered for now. All the accesses to the queue are from
      // either the main loop or a coroutine started by the main loop.
      _numberQueue.Enqueue(number);
    }

    void Start() {
      Active = false;
      SetupRefObjs();
      SetupDescPanels();
      StartCoroutine(LevelTransitionLoop());
    }

    void Update() {
      if (Input.GetMouseButtonDown(0)) {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100.0f)) {
          if (_refObjs.TryGetValue(hit.transform.name,
                                   out (GameObject Container, GameObject Obj) hitted)) {
            hitted.Obj.GetComponent<Animator>()?.SetTrigger(_activeAnimTriggerName);
          } else {
            // TODO: This on-click logic is for testing only. Remove it before production.
            int targetLevel = (_currentLevel + 1) % LevelConfigs.Levels.Count;
            QueueNewNumber(LevelConfigs.Levels[targetLevel].MinVisualizableNumber);
          }
        }
      }
      // TODO: This on-right-click logic is for testing only. Remove it before production.
      if (Input.GetMouseButtonDown(1)) {
        int targetLevel = _currentLevel - 1;
        if (targetLevel < 0) {
          targetLevel = LevelConfigs.Levels.Count - 1;
        }
        QueueNewNumber(LevelConfigs.Levels[targetLevel].MinVisualizableNumber);
      }
    }

    // The transition loop coroutine keeps running, peeking queued numbers and playing transition
    // animations.
    private IEnumerator LevelTransitionLoop() {
      while (true) {
        // So far the loop uses a simple strategy - only the last number is visualized if there are
        // more than one numbers have been queued during the last transition period.
        while (_numberQueue.Count > 1) {
          _numberQueue.Dequeue();
        }
        if (_numberQueue.Count == 1) {
          double number = _numberQueue.Dequeue();
          int level = LevelConfigs.MapNumberToLevel(number);
          if (level >= 0) {
            if (!Active) {
              Active = true;
            }
            // Hides indicator and desc panels during the transition.
            Indicator.Show(false);
            ShowCurrentDescPanels(false);
            if (_currentLevel >= 0 && (_currentLevel == level + 1 || _currentLevel == level - 1)) {
              // Slides to the left/right neighbor level.
              var animConfigs = PrepareSlideTransition(level, out GameObject objectToHideAfterAnim);
              for (int i = 1; i <= _transitionAnimSteps; i++) {
                // Adjusts positions and scales one step per frame.
                foreach (var animConfig in animConfigs) {
                  animConfig.Actor.transform.localPosition =
                      Vector3.Lerp(animConfig.FromPosition, animConfig.ToPosition,
                                   (float)i / (float)_transitionAnimSteps);
                  animConfig.Actor.transform.localScale =
                      Vector3.Lerp(animConfig.FromScale, animConfig.ToScale,
                                   (float)i / (float)_transitionAnimSteps);
                }
                yield return null;
              }
              if (!(objectToHideAfterAnim is null)) {
                objectToHideAfterAnim.SetActive(false);
              }
            } else if (_currentLevel != level) {
              // Jumps to the target level directly. For now there is no transition animation when
              // jumping to a non-neighbor level.
              JumpToLevel(level);
            }
            // Shows everything up once the transition is done.
            _currentLevel = level;
            Nav.SetNavLevel(LevelConfigs.Levels[level].NavLevel,
                            LevelConfigs.Levels[level].ScaleMarkerValueString);
            ScrollRainbowTo(LevelConfigs.Levels[level].NavLevel);
            Indicator.Show(true);
            double indicatorMax = LevelConfigs.Levels[level].ScalePerLargeUnit * _LargeCellRows;
            Indicator.SetValue(indicatorMax, number);
            ShowCurrentDescPanels(true);
          } else {
            // When a number is not able to be visualized, we do not turn the whole board to its
            // inactive mode. Instead, we simply hide the number indicator while keeping the
            // reference objects live on the board.
            _currentLevel = -1;
            Indicator.Show(false);
          }
        }
        yield return null;
      }
    }

    private void SetupRefObjs() {
      _refObjs.Clear();
      foreach (var config in LevelConfigs.Levels) {
        foreach (var refObjectConfig in config.RefObjs) {
          if (!_refObjs.ContainsKey(refObjectConfig.ObjName)) {
            var container = RootOfRefObjs.transform.Find(refObjectConfig.ContainerName);
            Debug.Assert(!(container is null));
            var obj = container.transform.Find(refObjectConfig.ObjName);
            obj.gameObject.SetActive(true);
            container.gameObject.SetActive(false);
            _refObjs.Add(refObjectConfig.ObjName, (container.gameObject, obj.gameObject));
          }
        }
      }
    }

    private void SetupDescPanels() {
      _descPanels.Clear();
      for (int level = 0; level < LevelConfigs.Levels.Count; level++) {
        var panel = RootOfDescPanels.transform.Find($"Level_{level}");
        if (!(panel is null)) {
          panel.gameObject.SetActive(false);
          _descPanels.Add(level, panel.gameObject);
        }
      }
    }

    private void JumpToLevel(int level) {
      ShowCurrentRefObjs(false);
      for (int i = 0; i < LevelConfigs.Levels[level].RefObjs.Length; i++) {
        var refObjectConfig = LevelConfigs.Levels[level].RefObjs[i];
        Debug.Assert(_refObjs.ContainsKey(refObjectConfig.ObjName));
        var container = _refObjs[refObjectConfig.ObjName].Container;
        container.transform.localPosition = refObjectConfig.InitialPosition;
        container.transform.localScale = LevelConfigs.CalcInitialScale(level, i);
        container.SetActive(true);
      }
    }

    private IReadOnlyList<SlideAnimConfig> PrepareSlideTransition(
        int level, out GameObject objectToHideAfterAnim) {
      var configs = new List<SlideAnimConfig>();
      if (_currentLevel == level + 1) {
        // Slides to left.
        int numLeftObjs = LevelConfigs.Levels[level].RefObjs.Length;
        if (numLeftObjs >= 2) {
          var leftConfig = LevelConfigs.Levels[level].RefObjs[0];
          var leftScale = LevelConfigs.CalcInitialScale(level, 0);
          var leftContainer = _refObjs[leftConfig.ObjName].Container;
          leftContainer.transform.localPosition = leftConfig.VanishingPosition;
          leftContainer.transform.localScale = leftScale / 10.0f;
          leftContainer.SetActive(true);
          configs.Add(new SlideAnimConfig {
            Actor = leftContainer,
            FromPosition = leftContainer.transform.localPosition,
            ToPosition = leftConfig.InitialPosition,
            FromScale = leftContainer.transform.localScale,
            ToScale = leftScale,
          });
        }

        var midConfig = LevelConfigs.Levels[_currentLevel].RefObjs[0];
        var midScale = LevelConfigs.CalcInitialScale(_currentLevel, 0);
        var targetConfig = LevelConfigs.Levels[level].RefObjs[numLeftObjs - 1];
        var targetScale = LevelConfigs.CalcInitialScale(level, numLeftObjs - 1);
        var midContainer = _refObjs[midConfig.ObjName].Container;
        configs.Add(new SlideAnimConfig {
          Actor = midContainer,
          FromPosition = midConfig.InitialPosition,
          ToPosition = targetConfig.InitialPosition,
          FromScale = midScale,
          ToScale = targetScale,
        });

        int numCurrentObjs = LevelConfigs.Levels[_currentLevel].RefObjs.Length;
        if (numCurrentObjs >= 2) {
          var rightConfig = LevelConfigs.Levels[_currentLevel].RefObjs[1];
          var rightScale = LevelConfigs.CalcInitialScale(_currentLevel, 1);
          var rightContainer = _refObjs[rightConfig.ObjName].Container;
          configs.Add(new SlideAnimConfig {
            Actor = rightContainer,
            FromPosition = rightConfig.InitialPosition,
            ToPosition = rightConfig.VanishingPosition,
            FromScale = rightScale,
            ToScale = rightScale * 10.0f,
          });
          objectToHideAfterAnim = rightContainer;
        } else {
          objectToHideAfterAnim = null;
        }
      } else if (_currentLevel == level - 1) {
        // Slides to right.
        int numCurrentObjs = LevelConfigs.Levels[_currentLevel].RefObjs.Length;
        if (numCurrentObjs >= 2) {
          var leftConfig = LevelConfigs.Levels[_currentLevel].RefObjs[0];
          var leftScale = LevelConfigs.CalcInitialScale(_currentLevel, 0);
          var leftContainer = _refObjs[leftConfig.ObjName].Container;
          configs.Add(new SlideAnimConfig {
            Actor = leftContainer,
            FromPosition = leftConfig.InitialPosition,
            ToPosition = leftConfig.VanishingPosition,
            FromScale = leftScale,
            ToScale = leftScale / 10.0f,
          });
          objectToHideAfterAnim = leftContainer;
        } else {
          objectToHideAfterAnim = null;
        }

        var midConfig = LevelConfigs.Levels[_currentLevel].RefObjs[numCurrentObjs - 1];
        var midScale = LevelConfigs.CalcInitialScale(_currentLevel, numCurrentObjs - 1);
        var targetConfig = LevelConfigs.Levels[level].RefObjs[0];
        var targetScale = LevelConfigs.CalcInitialScale(level, 0);
        var midContainer = _refObjs[midConfig.ObjName].Container;
        configs.Add(new SlideAnimConfig {
          Actor = midContainer,
          FromPosition = midConfig.InitialPosition,
          ToPosition = targetConfig.InitialPosition,
          FromScale = midScale,
          ToScale = targetScale,
        });

        int numRightObjs = LevelConfigs.Levels[level].RefObjs.Length;
        if (numRightObjs >= 2) {
          var rightConfig = LevelConfigs.Levels[level].RefObjs[1];
          var rightScale = LevelConfigs.CalcInitialScale(level, 1);
          var rightContainer = _refObjs[rightConfig.ObjName].Container;
          rightContainer.transform.localPosition = rightConfig.VanishingPosition;
          rightContainer.transform.localScale = rightScale * 10.0f;
          rightContainer.SetActive(true);
          configs.Add(new SlideAnimConfig {
            Actor = rightContainer,
            FromPosition = rightContainer.transform.localPosition,
            ToPosition = rightConfig.InitialPosition,
            FromScale = rightContainer.transform.localScale,
            ToScale = rightScale,
          });
        }
      } else {
        throw new System.ArgumentException();
      }
      return configs;
    }

    private void ShowCurrentRefObjs(bool show) {
      if (_currentLevel >= 0) {
        LevelConfig config = LevelConfigs.Levels[_currentLevel];
        foreach (var refObjConfig in config.RefObjs) {
          var container = _refObjs[refObjConfig.ObjName].Container;
          container.SetActive(show);
        }
      }
    }

    private void ShowCurrentDescPanels(bool show) {
      if (_descPanels.TryGetValue(_currentLevel, out GameObject panel)) {
        panel.SetActive(show);
      }
    }

    private void ScrollRainbowTo(int navLevel) {
      Debug.Assert(navLevel >= Nav.MinLevel && navLevel <= Nav.MaxLevel);
      int intervals = Nav.MaxLevel - Nav.MinLevel;
      float texOffsetX = (1.0f - _rainbowTexInitOffsetX) /
          intervals * (navLevel - Nav.MinLevel) + _rainbowTexInitOffsetX;
      if (texOffsetX > 1.0f) {
        texOffsetX -= 1.0f;
      }
      GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(texOffsetX, 0));
    }
  }
}
