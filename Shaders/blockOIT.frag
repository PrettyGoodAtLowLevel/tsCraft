//same as default.frag, but with some transparency comparison
#version 460 core
#extension GL_ARB_bindless_texture : require

//inputs
in vec2 TexCoords;
in vec3 FragPos;
in vec3 lightColor;
in float skyLight;
in float Ao;
in float faceLight;

//texture id coming from vertex shader
flat in uint TextureID;

//output fragment color
out vec4 FragColor;

//all textures on GPU
layout(std430, binding = 0) readonly buffer TextureBuffer
{
	sampler2D textures[];
};

layout(location = 0) out vec4 outAccum;
layout(location = 1) out float outReveal;

uniform vec3 cameraPos;
uniform vec3 skyColor;

//alpha multiplier for the material
//glass might be 0.15, water maybe 0.5, etc.
uniform float materialAlpha = 1.0;

float ComputeWeight(float alpha, float depth)
{
    float depthFactor = exp(-depth * 0.02); // tune this
    float alphaFactor = alpha * 0.8 + 0.2;

    return alphaFactor * depthFactor * 20.0;
}

void main()
{
    //sample whichever texture this face uses
	vec4 texColor = texture(textures[TextureID], TexCoords);
    if (texColor.a < 0.01) discard;

    float alpha = texColor.a * materialAlpha;
    if (alpha < 0.001) discard;

    vec3 skyLighting = pow(skyLight, 2.0) * skyColor;
    vec3 blockLighting = pow(lightColor, vec3(2.0)) * 2.0;
    vec3 lightFinal = max(blockLighting, skyLighting);

    //face lighting
    lightFinal *= faceLight;

    //ambience
    vec3 ambient = vec3(0.02);
    vec3 litColor = texColor.rgb * (lightFinal + ambient);

    //ao
    litColor *= Ao;

    //depth
    float depth = length(FragPos - cameraPos);

    //optional alpha shaping (no fog influence)
    float alphaEffective = alpha;

    //wboit
    float weight = max(
    min(1.0, max(max(litColor.r, litColor.g), litColor.b) * alphaEffective),
    alphaEffective) * clamp(0.03 / (1e-5 + pow(FragPos.z / 200.0, 4.0)), 1e-2, 3e3);

    outAccum = vec4(litColor * alphaEffective, alphaEffective) * weight;
    outReveal = alphaEffective;
}
