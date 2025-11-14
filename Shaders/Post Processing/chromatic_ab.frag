#version 330 core

uniform sampler2D sceneTex;
uniform float caStrength;
uniform float saturation;
uniform float vignetteStrength;
uniform vec3 tintColor;
uniform float tintIntensity;

uniform vec2 uResolution; //screen resolution
uniform float aaStrength; //how much smoothing to apply

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

    //apply fxaa
    vec2 texelSize = 1.0 / uResolution;
    vec3 c = color;
    vec3 tl = texture(sceneTex, TexCoords + texelSize * vec2(-1.0, -1.0)).rgb;
    vec3 tr = texture(sceneTex, TexCoords + texelSize * vec2(1.0, -1.0)).rgb;
    vec3 bl = texture(sceneTex, TexCoords + texelSize * vec2(-1.0, 1.0)).rgb;
    vec3 br = texture(sceneTex, TexCoords + texelSize * vec2(1.0, 1.0)).rgb;

    vec3 blur = (tl + tr + bl + br) * 0.25;
    color = mix(color, blur, aaStrength);

    FragColor = vec4(color, 1.0);
}