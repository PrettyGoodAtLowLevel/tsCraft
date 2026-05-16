#version 460 core
#extension GL_ARB_bindless_texture : require

in vec2 TexCoords;
out vec4 FragColor;

layout(bindless_sampler) uniform sampler2D tex0;

void main()
{
    vec4 texColor = texture(tex0, TexCoords);
    if (texColor.a <= 0.0) discard;

    FragColor = texColor;
}