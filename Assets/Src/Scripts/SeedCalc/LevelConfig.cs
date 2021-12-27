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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SeedCalc {
  // Class to define a reference object in a level.
  public class RefObjConfig {
    // The name of the reference object.
    //
    // Every reference object must have an empty parent object as its container. The container
    // object is used to position and scale the reference object. The name of the container must
    // be named as `<ObjName>_Container`.
    //
    // The reference object itself owns the animations that can be triggered by controllers. It
    // must also have a collider component so that it can receive users' click and touch events.
    //
    // A reference object may appear in more than one scale levels. In different scale levels, it
    // may have different description boxes so that its description text can vary among levels. In
    // the UI overlay, the scale level object and its child description box object are named as
    // `Level_<LevelIndex>/<ObjName>_Desc`.
    public string ObjName;
    // The initial position of the object.
    public Vector3 InitialPosition = new Vector3(0, 0, 0);
    // The vanishing point when the object is sliding out of the board. The slide-out animation
    // only happens when transitioning to a neighbor level. Thus, the left object always slides to
    // its left side and the right object always slides to its right side.
    public Vector3 VanishingPosition = new Vector3(0, 0, 0);
  }

  // Class to define the reference objects, the animations and the scale of a single level.
  public class LevelConfig {
    // The recommended center point to position the left/smaller object.
    public const float LeftCenterX = -3.514f;
    public const float LeftCenterY = -3f;

    // The recommended center point to position the right/larger object.
    public const float RightCenterX = 0;
    public const float RightCenterY = 0;

    // The recommended to-left vanishing point.
    public const float VanishingLeftX = -8;
    public const float VanishingLeftY = -2.8f;

    // The recommended to-right vanishing point.
    public const float VanishingRightX = 40;
    public const float VanishingRightY = 0.43f;

    // A typical level shows two reference objects - the left/smaller object and the right/larger
    // object. Each object can be randomly chosen from a list of candidates, defined with the
    // following two arrays.
    //
    // The left end level (the 1e-10 level) may show only one reference object. In this case,
    // LeftObjCandidates is set to null.
    public RefObjConfig[] LeftObjCandidates;
    public RefObjConfig[] RightObjCandidates;

    // The cutting board has a 4 rows by 6 columns grid, composed of 24 large square cells. Every
    // large cell has a 10 x 10 grid inside. ScalePerLargeUnit defines the side length (in meters)
    // of every large cell.
    public double ScalePerLargeUnit;

    // The formmatted value string of the scale marker length. Theoretically this value can be got
    // from (ScalePerLargeUnit / 5.0), but we require a pre-formatted string here to provide
    // flexibility to render the marker in very different formats for different levels.
    public string ScaleMarkerValueString;

    // The corresponding level of the nav bar. The nav bar uses a different level system so that
    // more than one config levels can map to the same nav bar level.
    public int NavLevel;

    // The min number (inclusive) and the max number (exclusive except for the very last level) of
    // the visualizable range for the current level.
    public double MinVisualizableNumber;
    public double MaxVisualizableNumber;
  }

  // The global definitions of all the visualization levels.
  public static class LevelConfigs {
    // Constructs the container name of a reference object.
    public static string GetContainerName(string objName) {
      return $"{objName}_Container";
    }

    // Constructs the parent object name of a level's description boxes.
    public static string GetDescPanelName(int level) {
      return $"Level_{level}";
    }

    // Constructs the description box name of a reference object.
    public static string GetDescName(string objName) {
      return $"{objName}_Desc";
    }

    // Determines if a double value is visualizable based on the level configs.
    public static bool IsVisualizable(double value) {
      return LevelConfigs.MapNumberToLevel(value) >= 0;
    }

    // Maps a visualizable number to a predefined level.
    public static int MapNumberToLevel(double number) {
      if (number <= 0) {
        return -1;
      }
      for (int level = 0; level < Levels.Count; level++) {
        var config = Levels[level];
        // Uses a [Min, Max) range to determinate if the number falls in a particular level, except
        // for the first level where [Min, Max] is used.
        //
        // If a (Min, Max] range is needed, the following conditions can be used instead:
        //
        //     (number >= config.MinVisualizableNumber && number < config.MaxVisualizableNumber) ||
        //     (level == Levels.Count - 1 && number == config.MaxVisualizableNumber)
        if ((number > config.MinVisualizableNumber && number <= config.MaxVisualizableNumber) ||
            (level == 0 && number == config.MinVisualizableNumber)) {
          return level;
        }
      }
      return -1;
    }

    // Calculates the initial scale of the reference object.
    public static Vector3 CalcInitialScale(int level, bool isLeftObj) {
      Debug.Assert(level >= 0 && level < Levels.Count);
      // The left/smaller object's initial scale is calculated by considering both the grid scale of
      // its left neighbor level and the grid scale of its own level. The right/larger object's
      // initial scale is always 1.
      float scale = isLeftObj ?
          (float)(Levels[level - 1].ScalePerLargeUnit / Levels[level].ScalePerLargeUnit) : 1;
      return new Vector3(scale, scale, scale);
    }

    // Returns an enumerable object that contains the reference object configs of both the left
    // candidates and the right candidates.
    public static IEnumerable<RefObjConfig> LeftAndRightCandidates(int level) {
      Debug.Assert(level >= 0 && level < Levels.Count &&
          !(Levels[level].RightObjCandidates is null));
      return Levels[level].LeftObjCandidates is null ? Levels[level].RightObjCandidates :
          Levels[level].LeftObjCandidates.Concat(Levels[level].RightObjCandidates);
    }

    public static readonly List<LevelConfig> Levels = new List<LevelConfig> {
      // 1E-10 only.
      new LevelConfig() {
        LeftObjCandidates = null,
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "FluorineAtom",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -2f),
            // No vanishing point for the left end level.
          },
        },
        ScalePerLargeUnit = 7.5e-11,
        NavLevel = -11,
        ScaleMarkerValueString = "1.5E-11",
        MinVisualizableNumber = 1e-10,
        MaxVisualizableNumber = 3e-10,
      },

      // 1E-10 and 1E-9.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "FluorineAtom",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, -2.65f, -10f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, -2.65f, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "DNA",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(80, LevelConfig.VanishingRightY, 20f),
          },
        },
        ScalePerLargeUnit = 6e-10,
        NavLevel = -10,
        ScaleMarkerValueString = "1.2E-10",
        MinVisualizableNumber = 3e-10,
        MaxVisualizableNumber = 1e-9, // Max scale of this level is 2.4e-9,
      },

      // 1E-9 and 1E-8.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "DNA",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, -2.7f, -1.2f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, -2.7f, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Catalase",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 2.5e-9,
        NavLevel = -9,
        ScaleMarkerValueString = "5E-10",
        MinVisualizableNumber = 1e-9,
        MaxVisualizableNumber = 1e-8,
      },

      // 1E-8 and 1E-7.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Catalase",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "FluVirus",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 2.5e-8,
        NavLevel = -8,
        ScaleMarkerValueString = "5E-9",
        MinVisualizableNumber = 1e-8,
        MaxVisualizableNumber = 1e-7,
      },

      // 1E-7 and 1E-6.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "FluVirus",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "EColi",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 5e-7,
        NavLevel = -7,
        ScaleMarkerValueString = "1E-7",
        MinVisualizableNumber = 1e-7,
        MaxVisualizableNumber = 1e-6, // Max scale of this level is 2e-6,
      },

      // 1E-6 and 1E-5.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "EColi",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "RedBloodCell",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 5e-6,
        NavLevel = -6,
        ScaleMarkerValueString = "1E-6",
        MinVisualizableNumber = 1e-6,
        MaxVisualizableNumber = 1e-5, // Max scale of this level is 2e-5,
      },

      // 1E-5 and 1E-4.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "RedBloodCell",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "EggCell",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 5e-5,
        NavLevel = -5,
        ScaleMarkerValueString = "1E-5",
        MinVisualizableNumber = 1e-5,
        MaxVisualizableNumber = 1e-4, // Max scale of this level is 2e-4,
      },

      // 1E-4 and 1E-3.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "EggCell",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "CabbageSeed",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 5e-4,
        NavLevel = -4,
        ScaleMarkerValueString = "1E-4",
        MinVisualizableNumber = 1e-4,
        MaxVisualizableNumber = 1e-3, // Max scale of this level is 2e-3,
      },

      // 1E-3 and 1E-2.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "CabbageSeed",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Bee",
            InitialPosition = new Vector3(.5f, LevelConfig.RightCenterY, -5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = .0025,
        NavLevel = -3,
        ScaleMarkerValueString = "0.0005",
        MinVisualizableNumber = 1e-3,
        MaxVisualizableNumber = .01,
      },

      // 1E-2 and 1E-1.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Bee",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Jellyfish",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
          new RefObjConfig {
            ObjName = "Cat",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
          new RefObjConfig {
            ObjName = "Rabbit",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = .075,
        NavLevel = -2,
        ScaleMarkerValueString = "0.025",
        MinVisualizableNumber = .01,
        MaxVisualizableNumber = .1, // Max scale of this level is .3,
      },

      // 1E-1 and 1E0.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Jellyfish",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
          new RefObjConfig {
            ObjName = "Cat",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, -2.5f, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, -2.5f, -.5f),
          },
          new RefObjConfig {
            ObjName = "Rabbit",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, -2.5f, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, -2.5f, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Penguin",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -1f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
          new RefObjConfig {
            ObjName = "Deer",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -1f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
          new RefObjConfig {
            ObjName = "Velociraptor",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = .25,
        NavLevel = -1,
        ScaleMarkerValueString = "0.05",
        MinVisualizableNumber = .1,
        MaxVisualizableNumber = 1,
      },

      // 1E0 and 1E+1.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Penguin",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, -3.2f, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, -3.2f, -.5f),
          },
          new RefObjConfig {
            ObjName = "Deer",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, -3.2f, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX,  -3.2f, -.5f),
          },
          new RefObjConfig {
            ObjName = "Velociraptor",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, -3.2f, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, -3.2f, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Orca",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
          new RefObjConfig {
            ObjName = "Giraffe",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
          new RefObjConfig {
            ObjName = "Tyrannosaurus",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 2.5,
        NavLevel = 0,
        ScaleMarkerValueString = "0.5",
        MinVisualizableNumber = 1,
        MaxVisualizableNumber = 10,
      },

      // 1E+1 and 1E+2.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Orca",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
          new RefObjConfig {
            ObjName = "Giraffe",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, -3.2f, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, -3.2f, -.5f),
          },
          new RefObjConfig {
            ObjName = "Tyrannosaurus",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, -3.2f, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, -3.2f, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Redwood",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -1f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 25,
        NavLevel = 1,
        ScaleMarkerValueString = "5",
        MinVisualizableNumber = 10,
        MaxVisualizableNumber = 100,
      },

      // 1E+2 and 1E+3.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Redwood",
            InitialPosition = new Vector3(-4f, -3.35f, -7f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Mountain",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -2f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 500,
        NavLevel = 2,
        ScaleMarkerValueString = "100",
        MinVisualizableNumber = 100,
        MaxVisualizableNumber = 1000 // Max scale of this level is 2000,
      },

      // 1E+3 and 1E+4.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Mountain",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, -2.85f, -25f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, -3.7f, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Everest",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, 0f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 100f),
          },
        },
        ScalePerLargeUnit = 2500,
        NavLevel = 3,
        ScaleMarkerValueString = "500",
        MinVisualizableNumber = 1000,
        MaxVisualizableNumber = 10000,
      },

      // 1E+4 and 1E+5.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Everest",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, -3.35f, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, -3.35f, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Manicouagan",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, -.5f),
          },
        },
        ScalePerLargeUnit = 50000,
        NavLevel = 4,
        ScaleMarkerValueString = "10000",
        MinVisualizableNumber = 10000,
        MaxVisualizableNumber = 1e+5, // Max scale of this level is 2E+5,
      },

      // 1E+5 and 1E+6.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Manicouagan",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Ceres",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 2.5E+5,
        NavLevel = 5,
        ScaleMarkerValueString = "50000",
        MinVisualizableNumber = 1E+5,
        MaxVisualizableNumber = 1E+6,
      },

      // 1E+6 and 1E+7.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Ceres",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Earth",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 3.75E+6,
        NavLevel = 6,
        ScaleMarkerValueString = "7.5E+5",
        MinVisualizableNumber = 1E+6,
        MaxVisualizableNumber = 1E+7, // Max scale of this level is 1.5E+7,
      },

      // 1E+7 and 1E+8.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Earth",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.5f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Jupiter",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 3.75E+7,
        NavLevel = 7,
        ScaleMarkerValueString = "7.5E+6",
        MinVisualizableNumber = 1E+7,
        MaxVisualizableNumber = 1E+8, // Max scale of this level is 1.5E+8,
      },

      // 1E+8 and 1E+9.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Jupiter",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -10f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Sun",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -1f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 3.75E+8,
        NavLevel = 8,
        ScaleMarkerValueString = "7.5E+7",
        MinVisualizableNumber = 1E+8,
        MaxVisualizableNumber = 1E+9, // Max scale of this level is 1.5E+9,
      },

      // 1E+9 and 1E+10.
      new LevelConfig() {
        LeftObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Sun",
            InitialPosition = new Vector3(LevelConfig.LeftCenterX, LevelConfig.LeftCenterY, -.2f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingLeftX, LevelConfig.VanishingLeftY, -.5f),
          },
        },
        RightObjCandidates = new RefObjConfig[] {
          new RefObjConfig {
            ObjName = "Blackhole",
            InitialPosition = new Vector3(LevelConfig.RightCenterX, LevelConfig.RightCenterY, -3f),
            VanishingPosition =
                new Vector3(LevelConfig.VanishingRightX, LevelConfig.VanishingRightY, 40f),
          },
        },
        ScalePerLargeUnit = 1.5E+10,
        NavLevel = 9,
        ScaleMarkerValueString = "3E+9",
        MinVisualizableNumber = 1E+9,
        MaxVisualizableNumber = 6E+10 // Max scale of this level is 6E+10,
      },
    };
  }
}
