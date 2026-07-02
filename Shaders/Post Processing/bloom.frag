//finds all bright values in our resolved scene framebuffer
#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D sceneTex;
uniform float threshold = 1.0;
uniform float knee = 1.0;

void main()
{
    vec3 color = texture(sceneTex, TexCoords).rgb;
    float brightness = max(max(color.r, color.g), color.b);

    //soft threshold
    float soft = smoothstep(threshold - knee, threshold + knee, brightness);
    vec3 bloom = color * soft;

    FragColor = vec4(bloom, 1.0);
}