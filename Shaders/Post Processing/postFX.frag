#version 330 core

uniform sampler2D godRaysTex;
uniform sampler2D bloomTex;
uniform sampler2D sceneTex;

uniform float caStrength;
uniform float saturation;

uniform float vignetteStrength;
uniform float exposure = 1.0;
uniform float bloomIntensity = 1.0;
uniform float godRaysIntensity = 0.12;

uniform vec3 tintColor;
uniform float tintIntensity;

in vec2 TexCoords;
out vec4 FragColor;

void main()
{

    //chromatic aberration
    vec2 center = vec2(0.5, 0.5);
    vec2 dir = TexCoords - center;
    float dist = length(dir);

    //optional edge fade to make aberration stable
    float edgeFade = smoothstep(0.0, 0.8, 0.8 - dist);
    vec2 offset = dir * caStrength * dist * edgeFade;

    float r = texture(sceneTex, TexCoords + offset).r;
    float g = texture(sceneTex, TexCoords).g;
    float b = texture(sceneTex, TexCoords - offset).b;
    vec3 color = vec3(r, g, b);

    //saturation
    float gray = dot(color, vec3(0.2126, 0.7152, 0.0722));
    color = mix(vec3(gray), color, saturation);

    //vignette
    float vignette = 1.0 - dist * vignetteStrength; 
    vignette = clamp(vignette, 0.0, 1.0);
    color *= vignette;

    //tint
    color = mix(color, tintColor, tintIntensity);

    //add bloom
    vec3 bloom = texture(bloomTex, TexCoords).rgb;
    color += bloom * bloomIntensity;

    //add god rays
    vec3 god = texture(godRaysTex, TexCoords).rgb;
    color += god * godRaysIntensity;

    vec3 mapped = vec3(1.0) - exp(-color * exposure);
    mapped = pow(mapped, vec3(1.0 / 1.1));
    FragColor = vec4(mapped, 1.0);
}