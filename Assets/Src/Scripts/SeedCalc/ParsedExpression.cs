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
using UnityEngine;

namespace SeedCalc {
  // The parsed expression for the calculator screen to display.
  public class ParsedExpression {
    // The input expression in plain text format. Incomplete expressions are accepted since the
    // calculator screen can syntax highlight the tokens before user finishes the expression.
    public string Expression { get; }
    // The syntax tokens parsed from the expression.
    public IReadOnlyList<SyntaxToken> SyntaxTokens { get; }
    // A text range that marks a sub-expression that is highlighted. It's null if nothing is
    // highlighted.
    public TextRange HighlightedRange { get; }
    // If the expression is being calculated.
    public bool BeingCalculated { get; }

    public ParsedExpression(string expression,
                            IReadOnlyList<SyntaxToken> syntaxTokens,
                            TextRange beingCalculatedRange,
                            bool beingCalculated) {
      Expression = expression;
      SyntaxTokens = syntaxTokens;
      HighlightedRange = beingCalculatedRange;
      BeingCalculated = beingCalculated;
    }

    // Extracts the substring corresponding to a parsed token from the expression text.
    public string ExtractTokenText(int tokenIndex) {
      Debug.Assert(tokenIndex >= 0 && tokenIndex < SyntaxTokens.Count);
      return ExtractTokenText(SyntaxTokens[tokenIndex]);
    }

    // Extracts the substring corresponding to a parsed token from the expression text.
    public string ExtractTokenText(SyntaxToken token) {
      return Expression.Substring(token.Range.Start.Column,
                                  token.Range.End.Column - token.Range.Start.Column + 1);
    }

    // Extracts the substring corresponding to the last token from the expression text.
    public string ExtractLastTokenText() {
      return ExtractTokenText(SyntaxTokens.Count - 1);
    }

    // Outputs a token's double value if the token is a number. Returns false if the token is not a
    // number.
    public bool TryParseNumber(int tokenIndex, out double number) {
      Debug.Assert(tokenIndex >= 0 && tokenIndex < SyntaxTokens.Count);
      return TryParseNumber(SyntaxTokens[tokenIndex], out number);
    }

    // Outputs a token's double value if the token is a number. Returns false if the token is not a
    // number.
    public bool TryParseNumber(SyntaxToken token, out double number) {
      number = 0;
      if (token.Type != SyntaxType.Number) {
        return false;
      }
      number = double.Parse(ExtractTokenText(token));
      return true;
    }

    // Returns if the last token is a number.
    public bool LastTokenIsNumber() {
      if (SyntaxTokens.Count <= 0) {
        return false;
      }
      return SyntaxTokens[SyntaxTokens.Count - 1].Type == SyntaxType.Number;
    }

    // Tries to get the number value of the last token. Returns false if the last token is not a
    // number.
    public bool TryParseLastTokenToNumber(out double number) {
      number = 0;
      if (!LastTokenIsNumber() || !TryParseNumber(SyntaxTokens.Count - 1, out number)) {
        return false;
      } else {
        return true;
      }
    }
  }
}
