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
  // A utility class to format double float numbers.
  public static class NumberFormatter {
    private const int _maxDisplayDigits = 11;

    // Formats a visualizable number in fixed point format.
    //
    // maxDisplayDigits specifies the maximum number of all display digits [0-9] in the formated
    // string, excluding the point character "." if there is any.
    public static string Format(double value, int maxDisplayDigits = _maxDisplayDigits) {
      string leading = "";
      if (value < 0) {
        value = -value;
        leading = "-";
      }
      int integerDigits = value < 1.0 ? 1 : (int)Math.Floor(Math.Log10(value) + 1);
      if (integerDigits > maxDisplayDigits) {
        int scientificFractionalDigits = maxDisplayDigits - 7;
        string format = $"E{scientificFractionalDigits}";
        return leading + value.ToString(format);
      } else {
        int fractionalDigits = maxDisplayDigits - integerDigits;
        string format = fractionalDigits > 0 ? $"F{fractionalDigits}" : $"F0";
        string formatted = value.ToString(format);
        return leading + (fractionalDigits > 0 ? formatted.TrimEnd('0').TrimEnd('.') : formatted);
      }
    }
  }
}
