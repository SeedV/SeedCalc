// Copyright 2021-2022 The SeedV Lab.
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
  // A utility class to format double float numbers.
  public static class NumberFormatter {
    private const int _maxDisplayDigits = 11;

    // Format a double value to a display string. If abs(value) is within the lower and upper bounds
    // of LevelConfigs, formats the value to a string with at most 11 digits. Otherwise, outputs the
    // value's scientific notation.
    public static string Format(double value) {
      string leading = "";
      if (value < 0) {
        value = -value;
        leading = "-";
      }
      int integerDigits = value < 1.0 ? 1 : (int)Math.Floor(Math.Log10(value) + 1);
      if (ToBeFormattedInScientificNotation(value)) {
        int scientificFractionalDigits = _maxDisplayDigits - 7;
        string format = $"E{scientificFractionalDigits}";
        return leading + value.ToString(format);
      } else {
        int fractionalDigits = _maxDisplayDigits - integerDigits;
        string format = fractionalDigits > 0 ? $"F{fractionalDigits}" : $"F0";
        string formatted = value.ToString(format);
        return leading + (fractionalDigits > 0 ? formatted.TrimEnd('0').TrimEnd('.') : formatted);
      }
    }

    // Determines if the double value will be formatted in scientific notation, considering the
    // lower and upper bounds of LevelConfigs.
    public static bool ToBeFormattedInScientificNotation(double value) {
      if (value < 0) {
        value = -value;
      }
      return value > 0 && (value < LevelConfigs.MinVisualizableNumber ||
                           value > LevelConfigs.MaxVisualizableNumber);
    }
  }
}
