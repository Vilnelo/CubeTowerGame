Shader "Custom/SemiOvalMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center ("Center", Vector) = (0.5, 0.3, 0, 0)
        _SizeX ("Oval Width", Range(0.001, 1.0)) = 0.6
        _SizeY ("Oval Height", Range(0.001, 1.0)) = 0.3
        _RectWidth ("Rectangle Width", Range(0.1, 3.0)) = 1.0
        _RectHeight ("Rectangle Height Up", Range(0.1, 2.0)) = 1.0
        _ShowMask ("Show Mask (Debug)", Range(0, 1)) = 0 // ← ДОБАВЛЕНО для отладки
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }
        
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
            
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
            float4 _MainTex_ST;
            float4 _Center;
            float _SizeX;
            float _SizeY;
            float _RectWidth;
            float _RectHeight;
            float _ShowMask; // ← ДОБАВЛЕНО

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float2 uv = i.uv - _Center.xy;
                
                // Создаем эллипс
                float ellipse = (uv.x * uv.x) / (_SizeX * _SizeX) + 
                               (uv.y * uv.y) / (_SizeY * _SizeY);
                
                // Полуовал сверху (только верхняя часть эллипса)
                float semiOval = step(ellipse, 1.0) * step(uv.y, 0.0);
                
                // Прямоугольник снизу с настраиваемой шириной
                float rectangleHeight = step(0.0, uv.y);
                float rectangleWidth = 1.0 - step(_RectWidth * 0.5, abs(uv.x));
                float rectangle = rectangleHeight * rectangleWidth;
                
                // Дополнительный прямоугольник вверх с настраиваемой высотой
                float upperRectHeight = 1.0 - step(_RectHeight * 0.5, abs(uv.y + 0.5));
                float upperRectWidth = 1.0 - step(_RectWidth * 0.5, abs(uv.x));
                float upperRect = step(uv.y, 0.0) * upperRectHeight * upperRectWidth;
                
                // Объединяем ВСЕ фигуры
                float combinedShape = max(max(semiOval, rectangle), upperRect);
                
                // ВАЖНО: отсекаем пиксели вне маски для Stencil Buffer
                clip(combinedShape - 0.5);
                
                // ← ИЗМЕНЕНО: возвращаем прозрачный цвет, но с возможностью показать маску для отладки
                return fixed4(1, 1, 1, _ShowMask);
            }
            ENDCG
        }
    }
}