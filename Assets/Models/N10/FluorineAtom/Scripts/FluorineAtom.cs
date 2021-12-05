using UnityEngine;

public class FluorineAtom : MonoBehaviour {
  void Update() {
    transform.RotateAround(transform.position,
                           new Vector3(1, 0, 1),
                           90 * Time.deltaTime);
  }
}
