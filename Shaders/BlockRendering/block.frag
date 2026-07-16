#version 460 core
#extension GL_ARB_bindless_texture : require

//inputs
in vec2 TexCoords;
in vec3 FragPos;
in vec3 lightColor;
in float skyLight;
in float Ao;
in float faceLight;
in vec4 FragPosLightSpace;
in vec3 Normal;

//texture id coming from vertex shader
flat in uint TextureID;

//output fragment color
out vec4 FragColor;

//all textures on GPU
layout(std430, binding = 0) readonly buffer TextureBuffer
{
	sampler2D textures[];
};

//uniforms
uniform vec3 cameraPos;
uniform vec3 skyColor;
uniform vec3 sunDirection;

void main()
{
    vec4 texColor = texture(textures[TextureID], TexCoords);

    if(texColor.a < 0.01) discard;

    //lighting
    vec3 skyLighting = pow(skyLight,2.0) * skyColor;
    vec3 blockLighting = pow(lightColor, vec3(2.0)) * 2.0;
    vec3 lightFinal = max(blockLighting, skyLighting);

    //face lighting
    lightFinal *= faceLight;

    //ambient
    vec3 ambient = vec3(0.02);
    vec3 litColor = texColor.rgb * (lightFinal + ambient);

    //AO
    litColor *= Ao;
    FragColor = vec4(litColor, texColor.a);
}