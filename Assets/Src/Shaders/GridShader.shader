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

// A shader that renders primary and secondary grids on a textured surface.
//
// The properties of the primary and secondary grids can be configured via Unity's inspector window.
//
// The properties can also be set by C# scripts during runtime. For example, the following code
// shows a zoom-in animation effect to scale the primary grid from 10x10 to 1x1:
//
//  public void ZoomIn() {
//    StartCoroutine(ZoomInAnim());
//  }
//
//  private IEnumerator ZoomInAnim() {
//    for (int scale = 10; scale > 0; scale--) {
//      GetComponent<Renderer>().material.SetFloat("_GridDimension", scale);
//      yield return new WaitForSeconds(0.1f);
//    }
//  }
Shader "Custom/GridShader" {
  Properties {
    // Standard texture and lighting properties.

    // The target textured surface will be rendered in the transparent queue, where the alpha
    // channels of _MainTex and _Color define the transparency of the surface.
    _MainTex("Texture", 2D) = "white" {}
    _Color("Color Tint", Color) = (0, 0, 0, 1)
    _Specular("Specular", Color) = (1, 1, 1, 1)
    _Gloss("Gloss", Range(8.0, 256.0)) = 20.0

    // The customizable properties of the primary grid.

    // The alpha channel of _GridColor defines how much the grid color is blended into the main
    // texture.
    _GridColor("Grid Color", Color) = (1, 1, 1, 1)
    // _GridDimension indicates the number of rows and columns of the primary grid. The primary grid
    // will be turned off if it is set to zero.
    _GridDimension("Grid Dimension", Range(0.0, 30.0)) = 10.0
    // The line width of the primary grid.
    _GridLineWidth("Grid Line Width", Range(1.0, 50.0)) = 2.0

    // The customizable properties of the secondary grid.

    // The alpha channel of _SecondaryGridColor defines how much the secondary grid color is blended
    // into the main texture.
    _SecondaryGridColor("Secondary Grid Color", Color) = (1, 1, 1, 1)
    // _SecondaryGridDimension indicates the number of rows and columns of the secondary grid. The
    // secondary grid will be turned off if it is set to zero.
    _SecondaryGridDimension("Secondary Grid Dimension", Range(0.0, 10.0)) = 0.0
    // The line width of the secondary grid.
    _SecondaryGridLineWidth("Secondary Grid Line Width", Range(1.0, 50.0)) = 1.0
  }

  SubShader {
    Tags {
      "Queue" = "Transparent"
      "IgnoreProjector" = "True"
      "RenderType" = "Transparent"
    }
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    LOD 100

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "Lighting.cginc"

      struct a2v {
        float4 pos : POSITION;
        float3 normal : NORMAL;
        float2 uv : TEXCOORD0;
      };

      struct v2f {
        float4 pos : SV_POSITION;
        float3 worldNormal : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
        float2 uv : TEXCOORD2;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;
      fixed4 _Color;
      fixed4 _Specular;
      float _Gloss;
      fixed4 _GridColor;
      float _GridLineWidth;
      float _GridDimension;
      fixed4 _SecondaryGridColor;
      float _SecondaryGridLineWidth;
      float _SecondaryGridDimension;

      v2f vert(a2v v) {
        v2f o;
        o.pos = UnityObjectToClipPos(v.pos);
        o.worldNormal = UnityObjectToWorldNormal(v.normal);
        o.worldPos = mul(unity_ObjectToWorld, v.pos).xyz;
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
      }

      // Calculates the grid color at a specified uv position. The alpha channel of the returned
      // color is a grayscale rendering mask, indicating how much the grid color will be blended to
      // the main texture.
      fixed4 GetGridColor(
          fixed4 gridColor,
          float gridDimension,
          float gridLineWidth,
          float2 uv) {
        const float Pi = 3.14159;
        const float GridLineDensity = 3000.0;
        // Scales a periodic curve (y = cos(x)) to the expected period and amplitude, then clips an
        // arc out of each period of the curve. The clipped arc‘s width is not strictly proportional
        // to the gridLineWidth argument but the result is good enough.
        float density = GridLineDensity / gridDimension;
        float2 curve = abs(cos(gridDimension * Pi * (uv - .5)));
        float2 clip = saturate((curve - 1) * density / gridLineWidth + 1);
        float blend = saturate(clip.x + clip.y);
        return fixed4(gridColor * blend);
      }

      // Returns the max number among the three components of a vector.
      float Max3(fixed3 v) {
        return max(max(v.x, v.y), v.z);
      }

      fixed4 frag(v2f i) : SV_Target {
        // Texture and lighting calculation for a specular surface.
        fixed3 worldNormal = normalize(i.worldNormal);
        fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
        fixed4 texColor = tex2D(_MainTex, i.uv);
        fixed3 albedo = texColor.rgb * _Color.rgb;
        fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
        fixed3 diffuse = _LightColor0.rgb * albedo * saturate(dot(worldNormal, worldLightDir));
        fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
        fixed3 halfDir = normalize(worldLightDir + viewDir);
        fixed3 specular =
            _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(worldNormal, halfDir)), _Gloss);
        fixed3 main = ambient + diffuse + specular;

        // Prepares the grid color. The color's alpha channel is a grayscale mask to indicate how
        // much the grid color will be blended to the main color.
        fixed4 grid = GetGridColor(_GridColor, _GridDimension, _GridLineWidth, i.uv);
        fixed4 secondary =
            GetGridColor(_SecondaryGridColor, _GridDimension * _SecondaryGridDimension,
                _SecondaryGridLineWidth, i.uv);

        // Blends the main color, the primary grid color, and the secondary grid color. The alpha
        // channels (masks) of the two grids control the priorities of the three colors:
        //
        // - The primary grid color overrides the other two colors.
        // - The secondary grid color overrids the main color.
        fixed3 blend =
            saturate(grid.rgb +
                secondary.rgb * (1 - grid.a) +
                main * (1 - grid.a) * (1 - secondary.a));

        return fixed4(blend, texColor.a * _Color.a);
      }
      ENDCG
    }
  }
  FallBack "Specular"
}
