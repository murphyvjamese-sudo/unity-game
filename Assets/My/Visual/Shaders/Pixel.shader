Shader "Unlit/Pixel"
{  //How to use: There is a ton of boilerplate crap required for handwritten shaders, (trust me, I tried to simplify it) but the only 
    //places in the codebase you should have to modify are in the Properties{} block, and
    //the body of the fixed4 frag function
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _IsInverted("IsInverted", Float) = 0.0
        _IsFrozen("IsFrozen", Float) = 0.0
        _Transparency("Transparency", Float) = 1.0

        _IsText("IsText", Float) = 0.0
        _Stroke("Stroke", Color) = (1, 1, 1) //png created with white stroke for text
        _Fill("Fill", Color) = (0, 0, 0) //png created with black fill for text
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha // Set the blend mode explicitly
        ZWrite Off

        Pass
        {
            Name "TikiTides"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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
            float _IsInverted;
            float _IsFrozen;
            float _Transparency;

            float _IsText;
            float4 _Stroke;
            float4 _Fill;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                
                if(_IsText == 1 && color.r > .5)
                { //if raw text image data is white (stroke), change color to specified
                    color.rgb = float3(_Stroke.r, _Stroke.g, _Stroke.b);
                }
                else if(_IsText == 1 && color.r < .5)
                { //if raw text image data is black (fill), change color to specified
                    color.rgb = float3(_Fill.r, _Fill.g, _Fill.b);
                }
                if(_IsFrozen && color.a != 0)
                {  //turns all opaque pixels white (might make this more advanced in future, since objects that are pallette swaps would be indistinguishable from each other if this shader applied.)
                    //one idea for visual distinction is to only apply this shader to the top half of the image?
                    color.rgb = float3(1, 1, 1);  //opaque white
                }
                if(_IsInverted)
                {  //same as high contrast - colors become opposites of each other.
                    color.rgb = float3(1 - color.r, 1 - color.g, 1 - color.b);
                }
                color.a *= _Transparency;  //modify transparency via scripts by setting this _Transparency prop instead of SpriteRenderer.Color

                return color;
            }
            ENDCG
        }
    }
}
