// Sprite'ı tek renk (siluet) çizer: hasar parlaması (FR-23) ve ulti parıltısı için. Renk = SpriteRenderer.color; flipX desteklenir.
Shader "LeafBrawl/SpriteFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "CanUseSpriteAtlas" = "True" "PreviewType" = "Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnitySprites.cginc"

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed a = SampleSpriteTexture(IN.texcoord).a;
                return fixed4(IN.color.rgb, a * IN.color.a);
            }
            ENDCG
        }
    }
}
