Shader "Hidden/MotionBlur"
{
    Properties
    {
        _MainTex ("", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            fixed4 frag(v2f_img i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
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
}