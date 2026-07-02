#version 330 core

in vec3 WorldDir;
out vec4 FragColor;

//sky colors
uniform vec3 zenithColor;
uniform vec3 midSkyColor;
uniform vec3 horizonColor;

//horizon haze
uniform vec3 horizonHazeColor;
uniform float horizonHazeStrength;

//sun
uniform vec3 sunDirection;
uniform vec3 sunColor;
uniform float sunIntensity;

//sun disc
uniform float sunAngularSize;

//sun glow
uniform float sunGlowIntensity;
uniform float sunGlowFalloff;

//sun scattering
uniform vec3 sunScatterColor;
uniform float sunScatterIntensity;
uniform float sunScatterFalloff;

void main()
{
    //sky gradient
    vec3 dir = normalize(WorldDir);
    vec3 sky;

    //upper sky
    if (dir.y > 0.5)
    {
        float t = (dir.y - 0.5) * 2.0;
        sky = mix(midSkyColor, zenithColor, t);
    }
    //horizon to middle
    else if (dir.y > 0.0)
    {
        float t = dir.y * 2.0;
        sky = mix(horizonColor, midSkyColor, t);
    }
    //below horizon (rarely visible)
    else
    {
        sky = horizonColor * 0.5;
    }

    //horizon haze
    float horizon = pow(1.0 - abs(dir.y), 8.0);
    sky += horizonHazeColor * horizon * horizonHazeStrength;

    //sun scattering
    float sunDot = max(dot(dir, -normalize(sunDirection)), 0.0);
    float scatter = pow(sunDot, sunScatterFalloff);
    sky += sunScatterColor * scatter * sunScatterIntensity;

    //sun glow/halo
    float glow = pow(sunDot, sunGlowFalloff);
    sky += sunColor * glow * sunGlowIntensity;
    
    //sun disc
    float sunDisc = smoothstep(sunAngularSize, sunAngularSize + 0.0005, sunDot);
    sky += sunColor * sunIntensity * sunDisc;

    FragColor = vec4(sky, 1.0);
}