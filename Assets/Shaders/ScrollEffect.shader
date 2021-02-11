Shader "ScrollEffect"
{
    Properties
    {
        _EffectTex("Effect", Cube) = "white" {}
        _GradientTex("Gradient", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            uniform samplerCUBE _EffectTex;
            uniform sampler2D _GradientTex;

            uniform half4 _EffectColor;
            uniform float _EffectOffset;
            uniform float3 _Scale; // vertex coordinates to [0,1] range

            uniform float3 _LocalAxis;
            uniform int _InvertAxis;

            uniform float _BlendAmount;

            struct VertexInput
            {
                float4 vertex : POSITION;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float3 uv_EffectTex : TEXCOORD0;
                float v_GradientTex : TEXCOORD1;
            };

            float vertexToGradientTexV(float3 vertex)
            {
                float gradTexV = dot(vertex, _LocalAxis);

                if (_InvertAxis == 0) {
                    gradTexV -= _EffectOffset;
                }
                else {
                    gradTexV = 0.5 - gradTexV - _EffectOffset;
                }

                return gradTexV;
            }

            VertexOutput vert(VertexInput v)
            {
                float3 unscaledVertex = v.vertex.xyz * _Scale;

                VertexOutput o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv_EffectTex = unscaledVertex;
                o.v_GradientTex = vertexToGradientTexV(unscaledVertex);

                return o;
            }

            half3 gradColorRamp(half3 gradColor)
            {
                half3 color1 = half3(0.0, 0.0, 0.0);
                half3 color2 = half3(0.5, 0.5, 0.5);
                half3 color3 = half3(1.0, 1.0, 1.0);

                half3 resultColor = lerp(color1, color1, smoothstep(0.0, 0.295, gradColor));
                resultColor = lerp(resultColor, color2, smoothstep(0.295, 0.732, gradColor));
                return lerp(resultColor, color3, smoothstep(0.732, 1.0, gradColor));
            }

            half3 effectColorRamp(half3 effectColor)
            {
                //half3 color1 = half3(1.0, 1.0, 1.0);
                //half3 color2 = half3(0.0, 0.0, 0.0);
                //
                //half3 newColor = lerp(color1, color2, smoothstep(0.0, 0.295, effectColor));
                //return newColor;//lerp(newColor, color2, smoothstep(0.295, 1.0, effectColor));

                return effectColor;
            }

            half4 frag(VertexOutput IN) : SV_Target
            {
                half3 effectTexColor = texCUBE(_EffectTex, IN.uv_EffectTex).rgb;
                half3 effectRampedColor = effectColorRamp(effectTexColor);
                half3 gradTexColor = tex2D(_GradientTex, float2(0.0, IN.v_GradientTex));
                half3 gradRampedColor = gradColorRamp(gradTexColor);

                half3 resultColor = effectRampedColor * gradTexColor;
                resultColor += gradRampedColor;
                resultColor *= _EffectColor.rgb;

                return half4(resultColor, _BlendAmount);
            }

            ENDCG
        }
    }
    Fallback Off
}