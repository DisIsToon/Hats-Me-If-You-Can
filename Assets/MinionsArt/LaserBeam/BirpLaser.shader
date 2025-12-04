// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/LaserBeam"
{
    Properties
    {

        // noise
        [Header(Noise Distortion)]
        _NoiseTex("Noise Tex", 2D) = "white" {}
        _NoiseScale("NoiseScale", Range(0, 1)) = 1.0
        _SpeedNoise("Move Speed Noise", Range(-50, 50)) = 1.0

        // layer 1 main
        [Header(Primary Shape)]
        _ShapeTexPrim("Shape Texture Primary", 2D) = "white" {}
        _MainTexScale("Shape Texture scale", Range(0, 20)) = 1.0
        _SpeedPrim("Move Speed Primary", Range(-200, 200)) = 1.0
        _NoiseDistortStrengthPrim("Noise Distort Strength Prim", Range(0, 3)) = 1.0
        _StrengthBeam("Lerp Beam in Shape", Range(0, 1)) = 1.0
        _NoiseStrengthShape("Lerp Noise in Prim Shape", Range(0, 1)) = 1.0

        [Header(Primary Cutoff and Intensity)]
        _CutoffPrim("Cutoff Primary", Range(0, 1)) = 1.0
        _Smoothness("Smoothness", Range(0, 1)) = 1.0
        _Intensity("Intensity", Range(0, 10)) = 1.0

        [Header(Primary Beam)]
        _PrimaryBeamWidth ("Width Beam", Range(0, 1)) = 0.1
        _SmoothnessBeam("Smoothess Beam", Range(0, 1)) = 0.1
        // layer 1 color
        [Header(Primary Color)]
        [HDR]_PrimCol1("Color 1 Prim", Color) = (0.5, 0.5, 0.5, 0.5)
        [HDR]_PrimCol2("Color 2 Prim", Color) = (0.5, 0.5, 0.5, 0.5)
        _Stretch("Col Stretch Primary", Range(-1, 1)) = 1.0
        _Offset("Col Offset Primary", Range(-3, 3)) = 1.0

        [Header(Secondary Layer Shape)]
        _ShapeTexSec("Secondary Shape Texture", 2D) = "white" {}
        _MainTexScale2("Shapetex scale2", Range(0, 20)) = 1.0
        _SpeedSecondary("Move Speed", Range(-20, 20)) = 1.0
        _NoiseDistortStrengthSec("Noise Distort Strength Sec", Range(0, 3)) = 1.0
        [Header(Secondary Layer Beam)]
        _SecondaryBeamWidth ("Beam Width Sec", Range(0, 1)) = 0.1
        [Header(Secondary Layer Cutoff)]
        _CutoffSec("Cutoff Secondary", Range(0, 1)) = 1.0
        _Smoothness2("Smoothness Secondary ", Range(0, 1)) = 1.0

        [Header(Secondary Layer Colors)]
        _Stretch2("SecondaryCol Stretch ", Range(-1, 1)) = 1.0
        _Offset2("Col Offset Secondary", Range(-3, 3)) = 1.0
        [HDR]_SecCol1("Secondary Color 1 ", Color) = (0.5, 0.5, 0.5, 0.5)
        [HDR]_SecCol2("Secondary Color 2 ", Color) = (0.5, 0.5, 0.5, 0.5)

        [Header(Extra Options)]
        [Toggle(ROTATE)] _ROTATE("Rotate UV", Float) = 0
        [Toggle(RIM)] _RIM("Use Rim for 3D Objects", Float) = 0
        [Header(Rim Options)]
        _RimpowerMain("Rimpower Beams", Range(-3, 30)) = 1.0
        _RimpowerFalloff("Rimpower Falloff", Range(-3, 30)) = 1.0
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane"
        }
        Blend One OneMinusSrcAlpha
        Cull back Lighting Off ZWrite Off

        SubShader
        {
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 4.0
                #pragma multi_compile_fog
                #pragma shader_feature RIM
                #pragma shader_feature ROTATE

                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                    float3 normal : NORMAL;
                    UNITY_FOG_COORDS(1)
                    float3 viewDir : TEXCOORD2;

                };

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.normal = v.normal;
                    o.viewDir = ObjSpaceViewDir(v.vertex);
                    o.texcoord = v.texcoord;
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    return o;
                }

                // textures
                sampler2D _NoiseTex, _ShapeTexPrim, _ShapeTexSec;

                float _NoiseDistortStrengthPrim;
                float _NoiseDistortStrengthSec;

                // colors
                float4 _PrimCol1, _PrimCol2;
                float4 _SecCol1, _SecCol2;

                float _RimpowerFalloff;

                // speed settings
                float _SpeedPrim, _SpeedNoise, _SpeedSecondary;

                float _StrengthBeam;
                float _NoiseStrengthShape;
                float _NoiseScale;
                float _Stretch, _Offset;
                float _Stretch2, _Offset2;
                float _CutoffPrim;
                float _Smoothness;
                float _MainTexScale, _MainTexScale2;
                float _PrimaryBeamWidth, _SmoothnessBeam;
                float _SecondaryBeamWidth;

                float _CutoffSec, _Smoothness2;
                float _Intensity;
                float _RimpowerMain;

                fixed4 frag(v2f i) : SV_Target
                {
                    // rotate the uv's, linerenderers  will have a different orientation than other meshes
                    #if ROTATE
                        i.texcoord = float2(i.texcoord.y, i.texcoord.x);
                    #endif

                    // create beam uv
                    float beamUV = 1 - abs(i.texcoord.y - 0.5);

                    // for 3d shapes, use the fresnel/rim
                    #if RIM
                        float baseFresnel = saturate(dot(normalize(i.normal), normalize(i.viewDir)));
                        float rim = 1 - pow(baseFresnel, _RimpowerMain);
                    #endif

                    // speed
                    float speedPrim = fmod((_Time.x * _SpeedPrim), _SpeedPrim);
                    float speedNoise = fmod((_Time.x * _SpeedNoise), _SpeedNoise);

                    // main scrolling noise
                    fixed noise = tex2D(_NoiseTex, float2(i.texcoord.x + speedNoise, i.texcoord.y) * _NoiseScale).r;

                    // primary layer
                    // primary shape
                    fixed shape = tex2D(_ShapeTexPrim, float2(((i.texcoord.x + speedPrim + (noise * _NoiseDistortStrengthPrim)) * _MainTexScale), i.texcoord.y)).r;

                    // beam shape
                    float beamShape = smoothstep(_PrimaryBeamWidth, _PrimaryBeamWidth + _SmoothnessBeam, beamUV);
                    #if RIM// use fresnel rim instead of uvs for 3d shapes
                        beamShape = smoothstep(_PrimaryBeamWidth, _PrimaryBeamWidth - _SmoothnessBeam, rim);
                    #endif

                    // lerp between noise and primary shape
                    float noisyBeam = lerp(beamShape, beamShape * shape, _StrengthBeam);
                    float noisyShape = lerp(shape, noise * shape, _NoiseStrengthShape);
                    float noisyAndShape = saturate(noisyBeam + noisyShape);
                    float cutoffshapePrim = smoothstep(_CutoffPrim, _CutoffPrim + _Smoothness, noisyAndShape);
                    // stretch and offset the gradient to control the colors more
                    float stretchedGradientPrim = saturate((noisyAndShape * _Stretch) + _Offset);
                    float4 primColored = lerp(_PrimCol1, _PrimCol2, stretchedGradientPrim) * cutoffshapePrim;

                    // secondary layer
                    float speedSec = fmod((_Time.x * _SpeedSecondary), _SpeedSecondary);
                    // secondary shape
                    fixed shape2 = tex2D(_ShapeTexSec, float2(((i.texcoord.x + speedSec + (noise * _NoiseDistortStrengthSec)) * _MainTexScale2), i.texcoord.y)).r;

                    // secondary beam shape
                    float beamShape2 = smoothstep(_SecondaryBeamWidth, _SecondaryBeamWidth + _SmoothnessBeam, beamUV);
                    #if RIM
                        beamShape2 = smoothstep(_SecondaryBeamWidth, _SecondaryBeamWidth - _SmoothnessBeam, rim);
                    #endif

                    // secondary shape only within the beam shape, with its own cutoff
                    float shapeWithinBeam = shape2 * beamShape2;
                    float cutoffShapeSec = smoothstep(_CutoffSec, _CutoffSec + _Smoothness2, shapeWithinBeam);
                    float strechtedGradientSec = saturate((shapeWithinBeam * _Stretch2) + _Offset2);

                    // lerp between 2 colors and keep the alpha from the shapelayer;
                    float4 secColored = lerp(_SecCol1, _SecCol2, strechtedGradientSec) * cutoffShapeSec;

                    // Combine
                    // layer minus second layer
                    float layerMinus = saturate(cutoffshapePrim - cutoffShapeSec);
                    float4 layeredResult = lerp(secColored, primColored, layerMinus);

                    // falloff edges
                    float smoothEdgesFalloff = smoothstep(0.01, 0.1, beamUV);
                    #if RIM
                        smoothEdgesFalloff = saturate(pow(baseFresnel, _RimpowerFalloff) * 10);
                    #endif

                    return (layeredResult * smoothEdgesFalloff);
                }
                ENDCG
            }
        }
    }
}
