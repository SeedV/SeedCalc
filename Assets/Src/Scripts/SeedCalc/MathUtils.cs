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
  }
}
