using System.Collections;
using UnityEngine;

namespace SeedCalc {
  public class CuttingBoard : MonoBehaviour {
    private const int _numLevel = 20;
    private static readonly Color _onColor = new Color(.8f, .8f, .8f);
    private static readonly Color _offColor = new Color(.14f, .27f, .26f);

    public Texture RainbowTexture;
    public GameObject _lightingMask;

    public void TurnOn(int level) {
      _lightingMask.SetActive(true);
      GetComponent<Renderer>().material.mainTexture = RainbowTexture;
      GetComponent<Renderer>().material.color = _onColor;
      ScrollRainbowTo(level);
    }

    public void TurnOff() {
      _lightingMask.SetActive(false);
      GetComponent<Renderer>().material.color = _offColor;
      GetComponent<Renderer>().material.mainTexture = null;
    }

    void Start() {
      StartCoroutine(AnimateAllLevels(.2f));
    }

    private IEnumerator AnimateAllLevels(float intervalInSeconds) {
      TurnOn(0);
      for (int i = 0; i < _numLevel; i++) {
        ScrollRainbowTo(i);
        yield return new WaitForSeconds(intervalInSeconds);
      }
      TurnOff();
    }

    private void ScrollRainbowTo(int level) {
      Debug.Assert(level >= 0 && level < _numLevel);
      float texOffsetX = 0.05f * (level + 1);
      GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(texOffsetX, 0));
    }
  }
}
