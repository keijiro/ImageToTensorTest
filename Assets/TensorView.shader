Shader "Hidden/ImageToTensorTest/TensorView"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    StructuredBuffer<float> _Tensor;
    uint _TensorWidth;

    void Vertex(float4 position : POSITION,
                float2 texCoord : TEXCOORD0,
                float4 color : COLOR,
                out float4 outPosition : SV_Position,
                out float2 outTexCoord : TEXCOORD0,
                out float4 outColor : COLOR)
    {
        outPosition = UnityObjectToClipPos(position);
        outTexCoord = texCoord;
        outColor = color;
    }

    float4 Fragment(float4 position : SV_Position,
                    float2 texCoord : TEXCOORD0,
                    float4 color : COLOR) : SV_Target
    {
        uint2 texp = texCoord * _TensorWidth;
        uint offs = (texp.y * _TensorWidth + texp.x) * 3;
        float r = _Tensor[offs + 0];
        float g = _Tensor[offs + 1];
        float b = _Tensor[offs + 2];
        return float4(r, g, b, 1) * color;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
