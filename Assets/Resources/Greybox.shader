// Greybox shader for the Phase 0 slice.
//
// It lives in Resources and is loaded by name so it is guaranteed to be in
// the player. Shader.Find("Standard") is not: built-in shaders that no scene
// material references get stripped, and Bootstrap builds every material at
// runtime, so nothing in the scene keeps them alive. That stripping is
// invisible in the editor and only shows up on device.
//
// Lighting is a single hardcoded direction. The slice does not need real
// lighting, it needs the crack to read as the darkest value on screen.
Shader "MindTheCrack/Greybox"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = normalize(float3(0.35, 1.0, -0.25));
                float ndotl = saturate(dot(normalize(i.worldNormal), lightDir));
                float shade = ndotl * 0.6 + 0.45;
                return fixed4(_Color.rgb * shade, _Color.a);
            }
            ENDCG
        }
    }
}
