Shader "Custom/MotionBlur"
{
    Properties
    {
        _MainTex ("", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1.0
        _BlurSamples ("Blur Samples", Int) = 16
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _BlurSize;
            int _BlurSamples;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 color = fixed4(0, 0, 0, 0);
                float2 blurDirection = float2(0, -1);
                
                for (int j = 0; j < _BlurSamples; j++)
                {
                    float offset = (float(j) / float(_BlurSamples - 1) - 0.5) * _BlurSize;
                    float2 uv = i.uv + blurDirection * offset;
                    color += tex2D(_MainTex, uv);
                }
                
                return color / float(_BlurSamples);
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;   
            sampler2D _CurrTex;   
            float     _BlurAmount;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 prev = tex2D(_MainTex, i.uv);
                fixed4 curr = tex2D(_CurrTex, i.uv);
                return lerp(curr, prev, _BlurAmount);
            }
            ENDCG
        }
    }
    
    Fallback Off
}
