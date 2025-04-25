Shader "Custom/Snow Interactive VR Compatible" { // VR CHANGE: Nombre actualizado
    Properties{
        // Propiedades originales sin cambios...
        [Header(Main)]
        _Noise("Snow Noise", 2D) = "gray" {}
        _NoiseScale("Noise Scale", Range(0,2)) = 0.1
        _NoiseWeight("Noise Weight", Range(0,2)) = 0.1
        [HDR]_ShadowColor("Shadow Color", Color) = (0.5,0.5,0.5,1)

        [Space]
        [Header(Tesselation)]
        _MaxTessDistance("Max Tessellation Distance", Range(10,100)) = 50
        _Tess("Tessellation", Range(1,500)) = 20

        [Space]
        [Header(Snow)]
        [HDR]_Color("Snow Color", Color) = (0.5,0.5,0.5,1)
        [HDR]_PathColorIn("Snow Path Color In", Color) = (0.5,0.5,0.7,1)
        [HDR]_PathColorOut("Snow Path Color Out", Color) = (0.5,0.5,0.7,1)
        _PathBlending("Snow Path Blending", Range(0,3)) = 0.3
        _MainTex("Snow Texture", 2D) = "white" {}
        _SnowHeight("Snow Height", Range(0,2)) = 0.3
        _SnowDepth("Snow Path Depth", Range(-2,2)) = 0.3
        _SnowTextureOpacity("Snow Texture Opacity", Range(0,1)) = 0.3
        _SnowTextureScale("Snow Texture Scale", Range(0,2)) = 0.3
        _Normal("Snow Normal Map", 2D) = "bump" {}
        _SnowNormalStrength("Snow Normal Strength", Range(0,1)) = 0.3

        [Space]
        [Header(Sparkles)]
        _SparkleScale("Sparkle Scale", Range(0,10)) = 10
        _SparkCutoff("Sparkle Cutoff", Range(0,2)) = 0.8
        _SparkleNoise("Sparkle Noise", 2D) = "gray" {}

        [Space]
        [Header(Rim)]
        _RimPower("Rim Power", Range(0,20)) = 20
        [HDR]_RimColor("Rim Color Snow", Color) = (0.5,0.5,0.5,1)

        // Propiedad Alpha Cutoff (usada por URP internamente a veces)
         [HideInInspector] _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
         // Propiedad Cull (si quieres controlarlo desde material)
         // [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Culling", Float) = 2 // Back
    }

    HLSLINCLUDE
    #pragma target 4.5 // Requerido para teselación

    // Includes de URP (Core incluye Lighting y otros básicos)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" // Para GetMainLight, etc.
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl" // Para TransformWorldToShadowCoord etc.

    // Incluir nuestro código de teselación
    #include "SnowTessellation.hlsl"

    // --- Texturas y Samplers Globales ---
    // (Usar TEXTURE2D/SAMPLER para compatibilidad moderna)
    TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
    TEXTURE2D(_SparkleNoise);   SAMPLER(sampler_SparkleNoise);
    TEXTURE2D(_Normal);         SAMPLER(sampler_Normal);
    // _Noise y _GlobalEffectRT ya están declarados/usados en SnowTessellation.hlsl
    // pero necesitamos los samplers si los usamos aquí también
    SAMPLER(sampler_Noise);
    SAMPLER(sampler_GlobalEffectRT);

    // --- CBuffer para propiedades de Material ---
    CBUFFER_START(UnityPerMaterial)
        // Variables asociadas a Properties
        float4 _Color; float4 _RimColor; float _RimPower;
        float4 _PathColorIn; float4 _PathColorOut; float _PathBlending;
        float _SparkleScale; float _SparkCutoff;
        float _SnowTextureOpacity; float _SnowTextureScale;
        float4 _ShadowColor; float _SnowNormalStrength;
        // Teselación (ya declaradas en HLSL, pero aquí para CBuffer)
        float _NoiseScale; float _NoiseWeight; float _SnowHeight; float _SnowDepth;
        float _MaxTessDistance; float _Tess;
    CBUFFER_END

    // --- Vertex Shader de Entrada (pasa datos al Hull Shader) ---
    ControlPoint TessellationVertexProgram(Attributes2 v)
    {
        ControlPoint p = (ControlPoint)0;
        // VR CHANGE: Configurar ID de Instancia
        UNITY_SETUP_INSTANCE_ID(v);
        // VR CHANGE: Transferir ID al punto de control
        UNITY_TRANSFER_INSTANCE_ID(v, p);

        // Pasar datos originales
        p.vertex = v.vertex;
        p.uv = v.uv;
        p.normal = v.normal;
        p.tangent = v.tangent;
        return p;
    }
    ENDHLSL // Fin de HLSLINCLUDE

    SubShader {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector"="True" }
        LOD 300

        // ======================= Pass Principal (Forward Lit) =======================
        Pass {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend Off ZWrite On ZTest LEqual Cull Back // Estado de renderizado Opaque

            HLSLPROGRAM
            // Shaders del pipeline de Teselación
            #pragma vertex TessellationVertexProgram
            #pragma hull hull
            #pragma domain domain
            // Fragment Shader
            #pragma fragment frag

            // --- Keywords y Variantes ---
            // VR CHANGE: Habilitar Instancing
            #pragma multi_compile_instancing
            // Keywords de URP para iluminación y sombras
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION // Si se usa SSAO
            #pragma multi_compile_fog // Niebla

            // --- Fragment Shader ---
            half4 frag(Varyings2 IN) : SV_Target {
                // VR CHANGE: Configurar Instancia y Stereo Eye Index
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN); // Necesario para lógica per-ojo (si la hubiera)

                // --- Lógica del Fragment Shader (similar al original) ---
                // UV para el efecto RT
                float2 effectUV = (IN.worldPos.xz - _Position.xz) / (_OrthographicCamSize * 2.0) + 0.5;
                float4 effect = SAMPLE_TEXTURE2D(_GlobalEffectRT, sampler_GlobalEffectRT, effectUV);
                // Máscara de borde
                float edgeMaskX = smoothstep(0.0, 0.1, effectUV.x) * smoothstep(1.0, 0.9, effectUV.x);
                float edgeMaskY = smoothstep(0.0, 0.1, effectUV.y) * smoothstep(1.0, 0.9, effectUV.y);
                effect *= edgeMaskX * edgeMaskY;

                // Color base (Albedo)
                float3 snowTexture = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.worldPos.xz * _SnowTextureScale).rgb;
                float3 snowTex = lerp(_Color.rgb, snowTexture * _Color.rgb, _SnowTextureOpacity);
                float3 path = lerp(_PathColorOut.rgb, _PathColorIn.rgb, saturate(effect.g * _PathBlending)); // * PathBlending estaba mal antes
                float3 albedo = lerp(snowTex, path, saturate(effect.g));

                // Normal (usando TBN de Varyings)
                float3 normalMap = UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, IN.worldPos.xz * _NoiseScale), _SnowNormalStrength); // Usar UnpackNormalScale
                float3 normalWS = normalize(IN.normal); // Usar normal de Varyings (ya en world)
                float3 tangentWS = normalize(IN.tangent);
                float3 bitangentWS = normalize(IN.bitangent);
                float3x3 TBN = float3x3(tangentWS, bitangentWS, normalWS);
                normalWS = TransformTangentToWorld(normalMap, TBN); // Aplicar normal map

                // Iluminación y Sombras
                float4 shadowCoord = TransformWorldToShadowCoord(IN.worldPos);
                Light mainLight = GetMainLight(shadowCoord);
                float shadow = mainLight.shadowAttenuation;

                // Luces Adicionales (loop simple)
                float3 additionalLightsColor = float3(0,0,0);
                #ifdef _ADDITIONAL_LIGHTS
                    int pixelLightCount = GetAdditionalLightsCount();
                    for (int i = 0; i < pixelLightCount; ++i) {
                        Light light = GetAdditionalLight(i, IN.worldPos, normalWS); // Pasar normalWS
                        additionalLightsColor += light.color * (light.distanceAttenuation * light.shadowAttenuation);
                    }
                #endif

                // Color final iluminado (simplificado)
                // Iluminación = (Albedo * (LuzPrincipal * Sombra + LuzAmbiente) + LucesAdicionales)
                // Nota: Esto es una aproximación. URP Lit usa un modelo PBR más complejo.
                // Si necesitas PBR completo, tendrías que preparar InputData/SurfaceData y llamar a UniversalFragmentPBR.
                float3 finalColor = albedo * (mainLight.color * shadow + SampleSH(normalWS)) + additionalLightsColor * albedo; // Multiplicar luces adicionales por albedo

                // Sparkles
                float sparklesStatic = SAMPLE_TEXTURE2D(_SparkleNoise, sampler_SparkleNoise, IN.worldPos.xz * _SparkleScale).r;
                float cutoffSparkles = step(_SparkCutoff, sparklesStatic);
                finalColor += lerp(cutoffSparkles * 4, 0, saturate(effect.g * 2)) * albedo; // Añadir como brillo sobre albedo

                // Rim Light
                half rim = 1.0 - saturate(dot(normalWS, normalize(IN.viewDir))); // Usar normalWS
                rim = pow(rim, _RimPower);
                rim = lerp(rim, 0, saturate(effect.g));
                finalColor += _RimColor.rgb * rim * _RimColor.a; // Multiplicar por alpha del color del rim

                // Colored Shadows (mezcla simple sobre el resultado)
                finalColor = lerp(finalColor * _ShadowColor.rgb, finalColor, shadow);

                // Niebla
                finalColor = MixFog(finalColor, IN.fogFactor);

                return half4(finalColor, 1.0); // Devolver color final (alpha 1.0 para opaco)
            }
            ENDHLSL
        } // Fin Pass ForwardLit

        // ======================= Pass Shadow Caster =======================
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual ColorMask 0 Cull Back

            HLSLPROGRAM
            // VR CHANGE: Usar pipeline de teselación para sombras
            #pragma vertex TessellationVertexProgram
            #pragma hull hull
            #pragma domain domain
            // Fragment mínimo
            #pragma fragment ShadowPassFragment

            // --- Keywords y Variantes ---
            // VR CHANGE: Habilitar Instancing
            #pragma multi_compile_instancing
            // Keyword para tipo de luz (usado en VertexProcessingLogic dentro de SnowTessellation.hlsl)
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // --- Fragment Shader Mínimo ---
            float4 ShadowPassFragment(Varyings2 IN) : SV_TARGET {
                UNITY_SETUP_INSTANCE_ID(IN); // Configurar instancia
                // No necesita hacer nada más, la profundidad se escribe automáticamente
                return 0;
            }
            ENDHLSL
        } // Fin Pass ShadowCaster

        // ======================= Pass Depth Only =======================
        Pass {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On ZTest LEqual ColorMask 0 Cull Back

            HLSLPROGRAM
            // VR CHANGE: Usar pipeline de teselación para Depth
            #pragma vertex TessellationVertexProgram
            #pragma hull hull
            #pragma domain domain
            // Fragment mínimo
            #pragma fragment DepthOnlyFragment

            // --- Keywords y Variantes ---
            // VR CHANGE: Habilitar Instancing
            #pragma multi_compile_instancing

            // --- Fragment Shader Mínimo ---
            float4 DepthOnlyFragment(Varyings2 IN) : SV_TARGET {
                UNITY_SETUP_INSTANCE_ID(IN); // Configurar instancia
                // Podría tener 'clip()' aquí si hubiera alpha test
                return 0;
            }
            ENDHLSL
        } // Fin Pass DepthOnly

    } // Fin SubShader
    Fallback "Universal Render Pipeline/Lit" // Fallback si falla
} // Fin Shader