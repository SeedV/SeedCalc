using System;
using UnityEngine;

namespace SeedCalc {
  public static class MathUtils {
    // The default epsilon is the same as the value that Vector3's ==, != operators use.
    public const float DefaultEpsilon = 0.00001f;

    // In Unity, both Vector3's ==, != operators and Mathf's Approximately method can compare float
    // numbers approximately. But it's not possible to control the epsilon value that they use to
    // compare numbers. For example, Vector3's ==, != operators use 1E-5 as the epsilon value, while
    // Mathf.Approximately uses Mathf.Epsilon which is the smallest float value.
    //
    // The following methods provide flexibility to set the epsilon value when comparing two float
    // numbers.
    public static bool EqualsApproximately(float a, float b, float epsilon = DefaultEpsilon) {
      return Mathf.Abs(a - b) < epsilon;
    }

    public static bool EqualsApproximately(Vector3 a, Vector3 b, float epsilon = DefaultEpsilon) {
      float diffX = a.x - b.x;
      float diffY = a.y - b.y;
      float diffZ = a.z - b.z;
      float squareDistance = diffX * diffX + diffY * diffY + diffZ * diffZ;
      return squareDistance < epsilon * epsilon;
    }

    // Returns the upper bound of a positive double value's order of magnitude. For example, returns
    // 10 for 3, 100 for 30, 0.1 for 0.03, 0.01 for 0.003. etc.
    //
    // A boundary number will be counted as the upper bound of its lower order of magnitude. For
    // example, this method returns 10 for 10, 0.1 for 0.1, etc.
    public static double OrderOfMagnitudeUpperBound(double value) {
      Debug.Assert(value > 0);
      double log = Math.Log10(value);
      int power = log % 1 == 0 ? (int)log : (int)Math.Floor(log + 1);
      return Math.Pow(10, power);
    }
  }
}
