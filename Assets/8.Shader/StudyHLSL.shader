//쉐이더라고 적혀있는 부분은 "인식" 용도입니다.
//스크립터블 오브젝트에서 폴더 지정하는 거랑 똑같다!
//머티리얼에서 쉐이더를 지정할 때 볼 수 있습니다.
Shader "Custom/StudyHLSL"
{
	//대상의 속성이 되는 멤버 변수!
    Properties
    {
		//          변수명     (표시명       자료형) = 기본값
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
		//                                            float4
		_ShadowColor("나는그림자속에있다", Color) = (0.5, 0.5, 0.5, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

		_Power("Power", Range(0, 1)) = 1.0
    }

    SubShader
    {
		//Opaque : 불투명
		//Tranparent : 투명   물, 유리, 등, 머리카락
		//Additive : 모든것이 그려진 뒤에 색깔을 더하는 이펙트
		//AlphaCutout : 투명+불투명 확실하게 나뉘어짐
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

		//깊이를 쓸 것인가? => 가려짐 여부
		//
		ZWrite On
		ZTest Less

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return color;
            }
            ENDHLSL
        }
    }
}
