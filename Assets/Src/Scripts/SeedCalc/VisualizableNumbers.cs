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

using System;
using UnityEngine;

namespace SeedCalc {
  // A utility class to define the range and the characteristics of the visualizable number set.
  public static class VisualizableNumber {
    // The maximum number of all display digits [0-9] in the formated string, excluding the point
    // character "." if there is any.
    private const int _maxDisplayDigits = 13;

    public static bool IsVisualizable(double value) {
      return LevelConfigs.MapNumberToLevel(value) >= 0;
    }

    // Formats a visualizable number in fixed point format.
    public static string Format(double value) {
      Debug.Assert(VisualizableNumber.IsVisualizable(value));
      int integerDigits = value < 1.0 ? 1 : (int)Math.Floor(Math.Log10(value) + 1);
      int fractionalDigits = _maxDisplayDigits - integerDigits;
      string format = fractionalDigits > 0 ? $"F{fractionalDigits}" : $"F0";
      string s = value.ToString(format);
      return fractionalDigits > 0 ? s.TrimEnd('0').TrimEnd('.') : s;
    }
  }
}
