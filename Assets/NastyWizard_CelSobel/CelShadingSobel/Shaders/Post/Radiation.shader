Shader "RadShads/RadiationShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_RadLevel("Radiation Level", Float) = 0.1
		_ResX("Resolution X", Float) = 1024
		_ResY("Resolution Y", Float) = 1024
	}
    SubShader
    {
		Tags { "RenderType" = "Opaque" }
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "PostFunctions.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _RadLevel;
			float _ResX;
			float _ResY;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float rand(fixed4 co) {
				return frac(sin(dot(dot(co.x, fixed4(12.9898, 78.233, 35.254, 98.435)), _Time/400000000)) * 43758.5453);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				//float2 offsets[9];
				//GetOffsets3x3(_ResX, _ResY, offsets);

				//fixed3 textures[9];
				//for (int j = 0; j < 9; j++)
				//{
				//	textures[j] = tex2D(_MainTex, i.uv + offsets[j]).rgb;
				//}

				fixed4 frag = tex2D(_MainTex, i.uv);

				frag = frag + rand(frag)*_RadLevel;

				return frag;
			}
            ENDCG
        }
    }
}
