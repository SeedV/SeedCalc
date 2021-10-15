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
using SeedLang.Common;

namespace SeedCalc {
  // The content for the calculator screen to display. The contnet will also be bound to the
  // visualization panel so that the expression or the last number can be visualized.
  public class DisplayContent {
    // The input expression in plain text format. Incomplete expressions are accepted since the
    // calculator screen can syntax highlight the tokens before user finishes the expression.
    public string Expression { get; }
    // The syntax tokens parsed from the expression.
    public IReadOnlyList<SyntaxToken> SyntaxTokens { get; }
    // A text range that marks a sub-expression that is being calculated. It's set to null when the
    // expression is not being calculated.
    public TextRange BeingCalculatedRange { get; }

    public DisplayContent(string expression,
                          IReadOnlyList<SyntaxToken> syntaxTokens,
                          TextRange beingCalculatedRange) {
      Expression = expression;
      SyntaxTokens = syntaxTokens;
      BeingCalculatedRange = beingCalculatedRange;
    }
  }
}
