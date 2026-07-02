#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D opaqueTex;
uniform sampler2D oitAccumTex;
uniform sampler2D oitRevealTex;

void main()
{
    vec3 opaque = texture(opaqueTex, TexCoords).rgb;
    vec4 accum = texture(oitAccumTex, TexCoords);
    float reveal = texture(oitRevealTex, TexCoords).r;

    vec3 transColor = vec3(0.0);
    if (accum.a > 1e-5) transColor = accum.rgb / accum.a;

    //reveal = 1 means no transparent contribution
    //reveal = 0 means transparent layers fully dominate
    vec3 finalColor = mix(opaque, transColor, 1.0 - reveal);

    FragColor = vec4(finalColor, 1.0);
}
