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
using UnityEngine.EventSystems;

using AgileMvvm;
using SeedLang.Common;

namespace SeedCalc {
  // The cutting board to show visualizable numbers and reference objects.
  public class CuttingBoard : MonoBehaviour {
    private class SlideAnimConfig {
      public GameObject Actor;
      public Vector3 FromPosition;
      public Vector3 ToPosition;
      public Vector3 FromScale;
      public Vector3 ToScale;
      public Vector3 PositionVelocity;
      public Vector3 ScaleVelocity;
    }

    // The approximately smooth damping animation time when transitioning to the neighbor level.
    public const float TransitionSmoothTime = 0.25f;

    // The active and inactive colors for the CuttingBoard material.
    private static readonly Color _activeColor = new Color(.6f, .6f, .6f);
    private static readonly Color _inactiveColor = new Color(.5f, .5f, .5f);
    // The initial x offset of the rainbow texture.
    private const float _rainbowTexInitOffsetX = 0.05f;
    // The numbers of large rows and columns of the grid.
    private const int _LargeCellRows = 4;
    private const int _LargeCellCols = 6;
    // The trigger name to play the active animation when user clicks or touches on the object.
    private const string _activeAnimTriggerName = "Active";
    // A number that is not visualizable so that it can be queued to turn off the indicator.
    private const int _nonVisualizableNumber = -1;

    public GameObject RootOfRefObjs;
    public GameObject RootOfDescPanels;
    public Texture RainbowTexture;
    public Texture InactiveTexture;
    public GameObject LightingMask;
    public Nav Nav;
    public Indicator Indicator;

    public AudioClip JumpToSound;
    public AudioClip SlideToSound;
    public AudioClip PlayAnimSound;

    private bool _active = false;
    private int _currentLevel = -1;
    // Queued numbers to be visualized.
    private readonly Queue<double> _numberQueue = new Queue<double>();
    // Map from (level, objName) to the description box of a reference object. A reference object
    // may appear in more than one levels and may have diffrent description boxes in different
    // levels.
    private Dictionary<(int level, string objName), GameObject> _descBoxes =
        new Dictionary<(int level, string objName), GameObject>();
    // The description panels for the left end and the right end.
    private GameObject _descLeftEndPanel = null;
    private GameObject _descRightEndPanel = null;
    // Map from the reference object name to its container object and its own game object.
    private Dictionary<string, (GameObject Container, GameObject Obj)> _refObjs =
        new Dictionary<string, (GameObject Container, GameObject Obj)>();
    // The config indices of the left object and right object on each level. A level's left object
    // and right object can be randomly chosen from a list of candidates. This array is used to hold
    // the current objects showed on each level. leftObjIndex is set to -1 in case the level has
    // only one major object.
    private (int leftObjIndex, int rightObjIndex)[] _levelObjs =
        new (int leftObjIndex, int rightObjIndex)[LevelConfigs.Levels.Count];

    // Queues a new number to be visualized. A transition between two visualization levels might not
    // be completed within one frame, thus a queue is used to hold the numbers to be visualized. A
    // coroutine LevelTransitionLoop keeps running to peek numbers from the queue and play the
    // transition animation for them.
    public void QueueNewNumber(double number) {
      // No thread-safe protection is considered for now. All the accesses to the queue are from
      // either the main loop or a coroutine started by the main loop.
      _numberQueue.Enqueue(number);
    }

    public void OnCalculatorParsedExpressionUpdated(object sender, UpdatedEvent.Args args) {
      if (args.Value is null) {
        // In public interfaces, do NOT update the internal states (e.g., Indicator.Visible = false)
        // directly, since the queued numbers access the internal states in an asynchronous way.
        // It's recommended to queue a non-visualizable number to turn off the indicator.
        QueueNewNumber(_nonVisualizableNumber);
        return;
      }
      var parsedExpression = args.Value as ParsedExpression;
      if (!parsedExpression.BeingCalculated &&
          parsedExpression.SyntaxTokens.Count == 1 &&
          parsedExpression.SyntaxTokens[0].Type == SyntaxType.Number &&
          parsedExpression.TryParseNumber(0, out double number)) {
        QueueNewNumber(number);
      } else {
        QueueNewNumber(_nonVisualizableNumber);
      }
    }

    public void OnCalculatorResultUpdated(object sender, UpdatedEvent.Args args) {
      double? result = args.Value as double?;
      QueueNewNumber(result is null ? _nonVisualizableNumber : (double)result);
    }

    void Start() {
      SetActive(false);
      SetupRefObjs();
      SetupDescPanels();
      StartCoroutine(LevelTransitionLoop());
    }

    void Update() {
      if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100.0f)) {
          if (_refObjs.TryGetValue(hit.transform.name,
                                   out (GameObject Container, GameObject Obj) hitted)) {
            hitted.Obj.GetComponent<Animator>()?.SetTrigger(_activeAnimTriggerName);
            PlaySound(PlayAnimSound);
          }
        }
      }
    }

    // Turns the cutting board on/off.
    private void SetActive(bool active) {
      GetComponent<Renderer>().material.mainTexture = active ? RainbowTexture : InactiveTexture;
      GetComponent<Renderer>().material.color =  active ? _activeColor : _inactiveColor;
      LightingMask.SetActive(active);
      Nav.Visible = active;
      Indicator.Visible = active;
      ShowRefObjsAtLevel(_currentLevel, active);
      if (active && _currentLevel < 0) {
        Indicator.Visible = false;
        Nav.SetNavLevel(Nav.DefaultLevel, Nav.DefaultMarkerValueString);
      }
      _active = active;
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
          if (number <= 0) {
            // Zero and negative numbers cannot be visualized. Turns off the cutting board. We do
            // not turn the whole board to its inactive mode. Instead, we simply hide the number
            // indicator while keeping the reference objects live on the board.
            Indicator.Visible = false;
          } else {
            if (!_active) {
              SetActive(true);
            }
            int level = LevelConfigs.MapNumberToLevel(number);
            if (level >= 0) {
              yield return TransitionToLevel(number, level);
            } else {
              TransitionToLeftOrRightEnd(number);
            }
          }
        }
        yield return null;
      }
    }

    private IEnumerator TransitionToLevel(double number, int level) {
      // Hides desc panels during the transition.
      ShowDescBoxesAtLevel(_currentLevel, false);
      // Plays a separate animation to grow the indicator and show the indicator value.
      double indicatorMax = LevelConfigs.Levels[level].ScalePerLargeUnit * _LargeCellRows;
      StartCoroutine(Indicator.SetValueWithAnim(indicatorMax, number));
      if (_currentLevel >= 0 && (_currentLevel == level + 1 || _currentLevel == level - 1)) {
        // Slides to the left/right neighbor level.
        var animConfigs = PrepareSlideTransition(level, out GameObject objectToHideAfterAnim);
        Debug.Assert(animConfigs.Count > 0);
        foreach (var animConfig in animConfigs) {
          animConfig.PositionVelocity = Vector3.zero;
          animConfig.ScaleVelocity = Vector3.zero;
        }
        while (!MathUtils.EqualsApproximately(animConfigs[0].Actor.transform.localPosition,
                                              animConfigs[0].ToPosition,
                                              0.005f)) {
          foreach (var animConfig in animConfigs) {
            animConfig.Actor.transform.localPosition =
                Vector3.SmoothDamp(animConfig.Actor.transform.localPosition,
                                   animConfig.ToPosition,
                                   ref animConfig.PositionVelocity,
                                   TransitionSmoothTime);
            animConfig.Actor.transform.localScale =
                Vector3.SmoothDamp(animConfig.Actor.transform.localScale,
                                   animConfig.ToScale,
                                   ref animConfig.ScaleVelocity,
                                   TransitionSmoothTime);
          }
          yield return null;
        }
        foreach (var animConfig in animConfigs) {
          animConfig.Actor.transform.localPosition = animConfig.ToPosition;
          animConfig.Actor.transform.localScale = animConfig.ToScale;
        }
        if (!(objectToHideAfterAnim is null)) {
          objectToHideAfterAnim.SetActive(false);
        }
        PlaySound(SlideToSound);
      } else if (_currentLevel != level) {
        // Jumps to the target level directly. For now there is no transition animation when
        // jumping to a non-neighbor level.
        JumpToLevel(level);
        PlaySound(JumpToSound);
      }
      // Shows everything up once the transition is done.
      _currentLevel = level;
      Nav.SetNavLevel(LevelConfigs.Levels[level].NavLevel,
                      LevelConfigs.Levels[level].ScaleMarkerValueString);
      ScrollRainbowTo(LevelConfigs.Levels[level].NavLevel);
      ShowDescBoxesAtLeftEnd(false);
      ShowDescBoxesAtRightEnd(false);
      ShowDescBoxesAtLevel(_currentLevel, true);
    }

    private void TransitionToLeftOrRightEnd(double number) {
      // For the postive numbers that are out of the LevelConfigs' min and max bounds, shows
      // the indicator only, without any reference object.
      ShowDescBoxesAtLevel(_currentLevel, false);
      ShowRefObjsAtLevel(_currentLevel, false);
      Nav.Visible = false;
      if (number < LevelConfigs.MinVisualizableNumber) {
        // The number exceeds the lower bound.
        ScrollRainbowToLeftEnd();
        ShowDescBoxesAtLeftEnd(true);
        ShowDescBoxesAtRightEnd(false);
      } else {
        // The number exceeds the upper bound.
        ScrollRainbowToRightEnd();
        ShowDescBoxesAtLeftEnd(false);
        ShowDescBoxesAtRightEnd(true);
      }
      Indicator.SetValue(MathUtils.OrderOfMagnitudeUpperBound(number), number);
      _currentLevel = -1;
    }

    private void SetupRefObjs() {
      _refObjs.Clear();
      for (int level = 0; level < LevelConfigs.Levels.Count; level++) {
        PreloadCandidateObjs(level);
        ChooseRefObjsRandomly();
      }
    }

    private void PreloadCandidateObjs(int level) {
      foreach (var refObjConfig in LevelConfigs.LeftAndRightCandidates(level)) {
        if (!_refObjs.ContainsKey(refObjConfig.ObjName)) {
          string containerName = LevelConfigs.GetContainerName(refObjConfig.ObjName);
          var container = RootOfRefObjs.transform.Find(containerName);
          Debug.Assert(!(container is null));
          var obj = container.transform.Find(refObjConfig.ObjName);
          obj.gameObject.SetActive(true);
          container.gameObject.SetActive(false);
          _refObjs.Add(refObjConfig.ObjName, (container.gameObject, obj.gameObject));
        }
      }
    }

    private void ChooseRefObjsRandomly() {
      for (int level = 0; level < LevelConfigs.Levels.Count; level++) {
        var config = LevelConfigs.Levels[level];
        int leftObjIndex = -1;
        if (!(config.LeftObjCandidates is null) && level > 0) {
          // The left/small object must be the same as the right/large object of the previous level.
          leftObjIndex = _levelObjs[level - 1].rightObjIndex;
        }
        Debug.Assert(!(config.RightObjCandidates is null));
        // The right/large object can be randomly chosen from the candidate list.
        int rightObjIndex = config.RightObjCandidates.Length <= 1 ? 0:
            Random.Range(0, config.RightObjCandidates.Length);
        _levelObjs[level] = (leftObjIndex, rightObjIndex);
      }
    }

    private void SetupDescPanels() {
      _descBoxes.Clear();
      for (int level = 0; level < LevelConfigs.Levels.Count; level++) {
        string descPanelName = LevelConfigs.GetDescPanelName(level);
        var panel = RootOfDescPanels.transform.Find(descPanelName);
        if (!(panel is null)) {
          panel.gameObject.SetActive(true);
          var config = LevelConfigs.Levels[level];
          foreach (var refObjConfig in LevelConfigs.LeftAndRightCandidates(level)) {
            string descName = LevelConfigs.GetDescName(refObjConfig.ObjName);
            var descBox = panel.Find(descName);
            if (!(descBox is null)) {
              descBox.gameObject.SetActive(false);
              _descBoxes.Add((level, refObjConfig.ObjName), descBox.gameObject);
            }
          }
        }
      }
      _descLeftEndPanel =
          RootOfDescPanels.transform.Find(LevelConfigs.GetLeftEndDescPanelName()).gameObject;
      _descRightEndPanel =
          RootOfDescPanels.transform.Find(LevelConfigs.GetRightEndDescPanelName()).gameObject;
    }

    private void JumpToLevel(int level) {
      ShowRefObjsAtLevel(_currentLevel, false);
      // Re-chooses reference objects randomly for all levels when jumping to a non-neighbor level.
      // On the other side, when the cuttong board slides to a neighbor level, the reference object
      // must not change since a object of the current level will be staying on the cutting board.
      ChooseRefObjsRandomly();
      ShowRefObjsAtLevel(level, true);
    }

    private void ShowRefObjsAtLevel(int level, bool show) {
      if (level >= 0) {
        var config = LevelConfigs.Levels[level];
        if (_levelObjs[level].leftObjIndex >= 0) {
          ShowRefObj(level, config.LeftObjCandidates[_levelObjs[level].leftObjIndex], true, show);
        }
        Debug.Assert(_levelObjs[level].rightObjIndex >= 0);
        ShowRefObj(level, config.RightObjCandidates[_levelObjs[level].rightObjIndex], false, show);
      }
    }

    private void ShowRefObj(int level, RefObjConfig refObjConfig, bool isLeftObj, bool show) {
      Debug.Assert(_refObjs.ContainsKey(refObjConfig.ObjName));
      var container = _refObjs[refObjConfig.ObjName].Container;
      if (show) {
        container.transform.localPosition = refObjConfig.InitialPosition;
        container.transform.localScale = LevelConfigs.CalcInitialScale(level, isLeftObj);
      }
      container.SetActive(show);
    }

    private void ShowDescBoxesAtLevel(int level, bool show) {
      if (level >= 0) {
        var config = LevelConfigs.Levels[level];
        if (_levelObjs[level].leftObjIndex >= 0) {
          string leftObjName = config.LeftObjCandidates[_levelObjs[level].leftObjIndex].ObjName;
          if (_descBoxes.TryGetValue((level, leftObjName), out var leftDescBox)) {
            LocalizationUtils.SetActiveAndUpdate(leftDescBox, show);
          }
        }
        Debug.Assert(_levelObjs[level].rightObjIndex >= 0);
        string rightObjName = config.RightObjCandidates[_levelObjs[level].rightObjIndex].ObjName;
        if (_descBoxes.TryGetValue((level, rightObjName), out var rightDescBox)) {
          LocalizationUtils.SetActiveAndUpdate(rightDescBox, show);
        }
      }
    }

    private void ShowDescBoxesAtLeftEnd(bool show) {
      LocalizationUtils.SetActiveAndUpdate(_descLeftEndPanel, show);
    }

    private void ShowDescBoxesAtRightEnd(bool show) {
      LocalizationUtils.SetActiveAndUpdate(_descRightEndPanel, show);
    }

    private IReadOnlyList<SlideAnimConfig> PrepareSlideTransition(
        int level, out GameObject objectToHideAfterAnim) {
      var animConfigs = new List<SlideAnimConfig>();
      var currentLevelConfig = LevelConfigs.Levels[_currentLevel];
      var targetLevelConfig = LevelConfigs.Levels[level];
      if (_currentLevel == level + 1) {
        // Slides to left.
        if (_levelObjs[level].leftObjIndex >= 0) {
          var leftConfig = targetLevelConfig.LeftObjCandidates[_levelObjs[level].leftObjIndex];
          var leftScale = LevelConfigs.CalcInitialScale(level, true);
          var leftContainer = _refObjs[leftConfig.ObjName].Container;
          leftContainer.transform.localPosition = leftConfig.VanishingPosition;
          leftContainer.transform.localScale = leftScale / 10.0f;
          leftContainer.SetActive(true);
          animConfigs.Add(new SlideAnimConfig {
            Actor = leftContainer,
            FromPosition = leftContainer.transform.localPosition,
            ToPosition = leftConfig.InitialPosition,
            FromScale = leftContainer.transform.localScale,
            ToScale = leftScale,
          });
        }

        var midConfig =
            currentLevelConfig.LeftObjCandidates[_levelObjs[_currentLevel].leftObjIndex];
        var midScale = LevelConfigs.CalcInitialScale(_currentLevel, true);
        var targetConfig = targetLevelConfig.RightObjCandidates[_levelObjs[level].rightObjIndex];
        var targetScale = LevelConfigs.CalcInitialScale(level, false);
        var midContainer = _refObjs[midConfig.ObjName].Container;
        animConfigs.Add(new SlideAnimConfig {
          Actor = midContainer,
          FromPosition = midConfig.InitialPosition,
          ToPosition = targetConfig.InitialPosition,
          FromScale = midScale,
          ToScale = targetScale,
        });

        var rightConfig =
            currentLevelConfig.RightObjCandidates[_levelObjs[_currentLevel].rightObjIndex];
        var rightScale = LevelConfigs.CalcInitialScale(_currentLevel, false);
        var rightContainer = _refObjs[rightConfig.ObjName].Container;
        animConfigs.Add(new SlideAnimConfig {
          Actor = rightContainer,
          FromPosition = rightConfig.InitialPosition,
          ToPosition = rightConfig.VanishingPosition,
          FromScale = rightScale,
          ToScale = rightScale * 10.0f,
        });
        objectToHideAfterAnim = rightContainer;
      } else if (_currentLevel == level - 1) {
        // Slides to right.
        if (_levelObjs[_currentLevel].leftObjIndex >= 0) {
          var leftConfig = currentLevelConfig.LeftObjCandidates[_levelObjs[_currentLevel].leftObjIndex];
          var leftScale = LevelConfigs.CalcInitialScale(_currentLevel, true);
          var leftContainer = _refObjs[leftConfig.ObjName].Container;
          animConfigs.Add(new SlideAnimConfig {
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

        var midConfig =
            currentLevelConfig.RightObjCandidates[_levelObjs[_currentLevel].rightObjIndex];
        var midScale = LevelConfigs.CalcInitialScale(_currentLevel, false);
        var targetConfig = targetLevelConfig.LeftObjCandidates[_levelObjs[level].leftObjIndex];
        var targetScale = LevelConfigs.CalcInitialScale(level, true);
        var midContainer = _refObjs[midConfig.ObjName].Container;
        animConfigs.Add(new SlideAnimConfig {
          Actor = midContainer,
          FromPosition = midConfig.InitialPosition,
          ToPosition = targetConfig.InitialPosition,
          FromScale = midScale,
          ToScale = targetScale,
        });

        var rightConfig = targetLevelConfig.RightObjCandidates[_levelObjs[level].rightObjIndex];
        var rightScale = LevelConfigs.CalcInitialScale(level, false);
        var rightContainer = _refObjs[rightConfig.ObjName].Container;
        rightContainer.transform.localPosition = rightConfig.VanishingPosition;
        rightContainer.transform.localScale = rightScale * 10.0f;
        rightContainer.SetActive(true);
        animConfigs.Add(new SlideAnimConfig {
          Actor = rightContainer,
          FromPosition = rightContainer.transform.localPosition,
          ToPosition = rightConfig.InitialPosition,
          FromScale = rightContainer.transform.localScale,
          ToScale = rightScale,
        });
      } else {
        throw new System.ArgumentException();
      }
      return animConfigs;
    }

    private void ScrollRainbowTo(int navLevel) {
      Debug.Assert(navLevel >= Nav.MinLevel && navLevel <= Nav.MaxLevel);
      // The rainbow texture image has a left-end and a right-end, which can be used to render a
      // background corresponding to a number that is either out-of-lower-bound or
      // out-of-upper-bound. Hence the number 2 is added to the interval count so that the left-end
      // and the right-end can be excluded when rendering background for normal levels.
      int intervals = Nav.MaxLevel - Nav.MinLevel + 2;
      float texOffsetX = (1.0f - _rainbowTexInitOffsetX) /
          intervals * (navLevel - Nav.MinLevel + 1) + _rainbowTexInitOffsetX;
      if (texOffsetX > 1.0f) {
        texOffsetX -= 1.0f;
      }
      GetComponent<Renderer>().material.SetTextureOffset("_MainTex",
                                                         new UnityEngine.Vector2(texOffsetX, 0));
    }

    private void ScrollRainbowToLeftEnd() {
      float texOffsetX = _rainbowTexInitOffsetX;
      GetComponent<Renderer>().material.SetTextureOffset("_MainTex",
                                                         new UnityEngine.Vector2(texOffsetX, 0));
    }

    private void ScrollRainbowToRightEnd() {
      float texOffsetX = 1.0f;
      GetComponent<Renderer>().material.SetTextureOffset("_MainTex",
                                                         new UnityEngine.Vector2(texOffsetX, 0));
    }

    private void PlaySound(AudioClip audioClip, float volumeScale = 1f) {
      GetComponent<AudioSource>().PlayOneShot(audioClip, volumeScale);
    }
  }
}
