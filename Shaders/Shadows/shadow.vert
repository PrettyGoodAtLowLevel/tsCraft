#version 460 core
layout(location = 0) in float aXPos;
layout(location = 1) in float aYPos;
layout(location = 2) in float aZPos;
layout(location = 6) in int aAnimationType;

uniform mat4 model;
uniform mat4 lightSpaceMatrix;

uniform float uChunkSize = 32.0;
uniform float uChunkHeight = 384.0;
uniform float uTime = 0.0;

uniform vec2 uChunkWorldPos;

float hash(vec2 p)
{
    return fract(sin(dot(p, vec2(127.1,311.7))) * 43758.5453123);
}

vec3 ApplyLeaves(vec3 pos, vec3 worldPos, vec2 blockXZ)
{
    float rnd = hash(blockXZ);

    float phase = rnd * 6.28318;
    float speed = mix(1.0, 1.8, rnd);
    float amp = mix(0.03, 0.06, rnd);

    float windX = sin(uTime * speed + phase);
    float windZ = cos(uTime * speed * 0.85 + phase);

    pos.x += windX * amp;
    pos.z += windZ * amp;

    return pos;
}

void main()
{
    float x = (aXPos / 65535.0) * uChunkSize;
    float y = (aYPos / 65535.0) * uChunkHeight;
    float z = (aZPos / 65535.0) * uChunkSize;

    vec3 pos = vec3(x,y,z);
    vec4 worldBase = model * vec4(pos,1.0);

    vec2 blockXZ = floor(vec2(x,z)+uChunkWorldPos);

    if(aAnimationType == 1) pos = ApplyLeaves(pos, worldBase.xyz, blockXZ);
    vec4 worldPos = model * vec4(pos,1.0);

    gl_Position = lightSpaceMatrix * worldPos;
}