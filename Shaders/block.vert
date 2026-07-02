#version 330 core

//inputs
layout(location = 0) in float aXPos;
layout(location = 1) in float aYPos;
layout(location = 2) in float aZPos;
layout(location = 3) in vec2 aUV;
layout(location = 4) in int aLighting;
layout(location = 5) in int aAO;
layout(location = 6) in int aAnimationType;
layout(location = 7) in uint aTexID;

//outputs
out vec2 TexCoords;
out vec3 FragPos;
out vec3 lightColor;
out float skyLight;
out float Ao;
out float faceLight;
flat out uint TextureID;
out vec4 FragPosLightSpace;
out vec3 Normal;

//uniforms
uniform mat4 lightSpaceMatrix;
uniform vec2 uChunkWorldPos;
uniform mat4 camMatrix;
uniform mat4 model;

uniform float uChunkSize = 32.0;
uniform float uChunkHeight = 384.0;
uniform float uTime = 0.0;

//ambient occlusion lookup
const float AO_TABLE[4] = float[](1.0, 0.85, 0.7, 0.55);

//normal based lighting values, btm, top, frnt, bck, rght, lft
const float FACE_LIGHT[6] = float[6](0.60, 1.00, 0.78, 0.78, 0.72, 0.72);

//expanded normals, btm, top, frnt, bck, rght, lft
const vec3 FACE_NORMALS[6] = vec3[6](vec3( 0,-1, 0), vec3( 0, 1, 0), vec3( 0, 0, 1), vec3( 0, 0,-1), vec3( 1, 0, 0), vec3(-1, 0, 0));

//hasing function
float hash(vec2 p)
{
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

//leaves sway
vec3 ApplyLeaves(vec3 pos, vec3 worldPos, vec2 blockXZ)
{
    float rnd = hash(blockXZ);

    float phase = rnd * 6.28318;
    float speed = mix(1.0, 1.8, rnd);
    float amp   = mix(0.03, 0.06, rnd);

    float windX = sin(uTime * speed + phase + blockXZ.x * 0.15 + blockXZ.y * 0.12);
    float windZ = cos(uTime * speed * 0.85 + phase + blockXZ.x * 0.10 + blockXZ.y * 0.14);

    pos.x += windX * amp;
    pos.z += windZ * amp;

    vec3 viewDir = normalize(-worldPos);
    pos += viewDir * 0.0015;

    return pos;
}

//main
void main()
{
    float x = (aXPos / 65535.0) * uChunkSize;
    float y = (aYPos / 65535.0) * uChunkHeight;
    float z = (aZPos / 65535.0) * uChunkSize;
    vec3 pos = vec3(x, y, z);

    vec4 worldBase = model * vec4(pos, 1.0);
    vec3 worldPos = worldBase.xyz;

    //block coordinate (stable per chunk/world)
    vec2 blockXZ = floor(vec2(x, z) + vec2(uChunkWorldPos));

    //animation dispatch
    if (aAnimationType == 1) pos = ApplyLeaves(pos, worldPos, blockXZ);

    //final transform
    vec4 finalWorld = model * vec4(pos, 1.0);
    gl_Position = camMatrix * finalWorld;

    //decode ao and normal
    int packedAO = aAO;
    int aoVal = packedAO & 3;
    int normal = (packedAO >> 2) & 7;

    //lighting decode
    int L = int(aLighting);
    float lightR = ((L >> 0) & 0xF) / 15.0;
    float lightG = ((L >> 4) & 0xF) / 15.0;
    float lightB = ((L >> 8) & 0xF) / 15.0;
    float lightS = ((L >> 12) & 0xF) / 15.0;
    float normalLight = FACE_LIGHT[normal];

    //outputs
    TexCoords = aUV;
    FragPos = finalWorld.xyz;
    lightColor = vec3(lightR, lightG, lightB);
    skyLight = lightS;
    Ao = AO_TABLE[clamp(aoVal, 0, 3)];
    faceLight = normalLight;
    TextureID = aTexID;
    FragPosLightSpace = lightSpaceMatrix * finalWorld;
    Normal = FACE_NORMALS[clamp(normal, 0, 5)];
}