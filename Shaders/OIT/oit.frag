#version 450 core
layout(location = 0) out vec4 AccumColor;
layout(location = 1) out float AccumAlpha;

in vec2 TexCoords; //pass this from voxel shader

uniform sampler2D blockTex;

void main()
{
    vec4 texColor = texture(blockTex, TexCoords);
    float alpha = texColor.a;

    vec3 color = texColor.rgb * alpha;

    AccumColor.rgb += color;
    AccumColor.a   += alpha;
    AccumAlpha = AccumColor.a;
}