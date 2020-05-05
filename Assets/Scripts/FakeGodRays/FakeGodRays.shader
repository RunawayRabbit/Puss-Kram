
Shader "Unlit/FakeGodRays"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Speed("Speed", Float) = 5
		_Strength("Strength", Float) = 0.3
	}
    SubShader
    {
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
		ZWrite Off
		Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Speed;
			float _Strength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float t1 = fmod(1 + (sin(_Time.x)*sin(_Time.x + _Speed)),2)*0.5f;
				float t2 = 1 + (sin(_Time.x * _Speed) *0.5);

				float yt1 = (i.uv.x*t1);
				float yt2 = (i.uv.x*t2);

				fixed4 anim1 = tex2D(_MainTex, float2(i.uv.x, yt1));
				fixed4 anim2 = tex2D(_MainTex, float2(i.uv.x, yt2));
				fixed4 stat = tex2D(_MainTex, i.uv);
				fixed4 col = anim1 * 0.4 + anim2 * 0.4 + stat*0.2;
				
				col.a *= i.uv.y * _Strength;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
