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

namespace SeedCalc {
  // A utility class to define the range and the characteristics of the visualizable number set.
  public static class VisualizableNumbers {
    // The left end (inclusive) of the range.
    public const double _minValue = 1E-10;
    // The right end (inclusive) of the range.
    public const double _maxValue = 1E+10;
    // The maximum number of all display digits [0-9] in the formated string, excluding the point
    // character "." if there is nay.
    public const int _maxDisplayDigits = 11;

    // Tries to format a visualizable value in fixed point format.
    public static bool TryFormatVisualizableValue(double value, out string result) {
      if (value < _minValue || value > _maxValue) {
        result = null;
        return false;
      }

      int integerDigits = value < 1.0 ? 1 : (int)Math.Floor(Math.Log10(value) + 1);
      int fractionalDigits = _maxDisplayDigits - integerDigits;
      string format = fractionalDigits > 0 ? $"F{fractionalDigits}" : $"F0";
      string s = value.ToString(format);
      result = fractionalDigits > 0 ? s.TrimEnd('0').TrimEnd('.') : s;
      return true;
    }
  }
}
