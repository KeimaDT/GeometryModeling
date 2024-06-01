Shader "Examples/UniqueHueShift"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 positionOS : Position;
            };

            struct v2f
            {
                float4 positionCS : SV_Position;
                float4 hueShiftColor : COLOR;
            };

            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.positionCS = UnityObjectToClipPos(v.positionOS);

                float3 hsvColor = float3(0.0f, 1.0f, 1.0f);
                hsvColor.r = (sin(_Time.y * 1.2) + 1.0f) * 0.5f; 
                float3 rgbColor = hsv2rgb(hsvColor);

                o.hueShiftColor = float4(rgbColor, 1.0f);

                return o;
            }

            float4 _BaseColor; 

            float4 frag (v2f i) : SV_Target
            {
                return float4(i.hueShiftColor.rgb * _BaseColor.rgb, 1.0f); 
            }
            ENDHLSL
        }
    }
}
