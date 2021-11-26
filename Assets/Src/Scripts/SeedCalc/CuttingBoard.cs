using System.Collections;
using UnityEngine;

public class CuttingBoard : MonoBehaviour {
  void Start(){
    StartCoroutine(Scroll());
  }

  private IEnumerator Scroll() {
    while (true) {
      for (int i = 0; i < 20; i++) {
        ScrollRainbowTo(i);
        yield return new WaitForSeconds(1f);
      }
    }
  }

  private void ScrollRainbowTo(int level) {
    Debug.Assert(level >= 0 && level < 20);
    float texOffsetX = 0.05f * (level + 1);
    GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(texOffsetX, 0));
  }
}
