#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D sceneTex;
uniform sampler2D depthTex;

uniform vec2 lightScreenPos;
uniform float density;
uniform float decay;
uniform float exposure;
uniform float weight;
uniform float sunVisibility;
uniform int samples;
uniform vec3 rayColor;

void main()
{
    vec2 delta = (TexCoords - lightScreenPos);
    delta *= density / float(samples);

    vec2 coord = TexCoords;
    float illuminationDecay = 1.0;
    vec3 color = vec3(0.0);

    for (int i = 0; i < samples; i++)
    {
        coord -= delta;

        //clamp god rays to screen to not get weird stretching
        if (coord.x < 0.0 || coord.x > 1.0 || coord.y < 0.0 || coord.y > 1.0)
            break;

        float depth = texture(depthTex, coord).r;

        //only let rays through where the depth is very far / sky-like
        float visibility = smoothstep(0.995, 1.0, depth);

        //use luminance so the rays do not inherit huge scene colors
        vec3 scene = texture(sceneTex, coord).rgb;
        float lum = dot(scene, vec3(0.2126, 0.7152, 0.0722));

        //small threshold so dark terrain doesn't flood the effect
        float light = mix(0.2, 1.0, lum);

        color += rayColor * light * visibility * illuminationDecay * weight;
        illuminationDecay *= decay;
    }

    FragColor = vec4(color * exposure * sunVisibility, 1.0);
}