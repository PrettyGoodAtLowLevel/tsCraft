#version 450 core
out vec4 FragColor;

uniform sampler2D accumColorTex;
uniform sampler2D accumAlphaTex;
in vec2 TexCoords;

void main()
{
    vec3 color = texture(accumColorTex, TexCoords).rgb;
    float alpha = texture(accumAlphaTex, TexCoords).r;

    FragColor = vec4(color / max(alpha, 0.0001), alpha); // final blended result
}