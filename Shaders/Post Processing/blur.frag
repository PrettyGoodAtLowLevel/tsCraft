//blurs all bright pixels found from bright pass framebuffer shader
#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D sceneTex;
uniform bool horizontal;
uniform vec2 Resolution;

void main()
{
    vec2 texel = 1.0 / Resolution;
    float weight[5] = float[](0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);
    vec3 result = texture(sceneTex, TexCoords).rgb * weight[0];

    for (int i = 1; i < 5; i++)
    {
        vec2 offset = horizontal ? vec2(texel.x * i, 0.0) : vec2(0.0, texel.y * i);

        result += texture(sceneTex, TexCoords + offset).rgb * weight[i];
        result += texture(sceneTex, TexCoords - offset).rgb * weight[i];
    }

    FragColor = vec4(result, 1.0);
}
