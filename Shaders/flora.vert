//shader that does minecraft like sway on leaves and plants
#version 330 core
//regular v shader stuff
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aUV;
layout(location = 2) in int aNormal;    
layout(location = 3) in int aAO; 

out vec2 TexCoords;
flat out int NormalID;
flat out int AO;
out vec3 FragPos;

uniform mat4 camMatrix;
uniform mat4 model;

//sway controls
uniform float time;        
uniform float swayStrength; //e.g. 0.08
uniform float swaySpeed;    //e.g. 1.5
uniform float maxPlantHeight; //e.g. 1.0 for grass, 2.0 for flowers

const float epsilon = 0.01;  //vertical offset to avoid z-fighting
const float shrinkXZ = 0.9995; //scale quad in X/Z to prevent side z-fighting

void main()
{
    //contract X/Z to avoid overlapping neighbors
    vec3 localPos = aPos * vec3(shrinkXZ, 1.0, shrinkXZ);

    //base world position
    vec4 worldPos = model * vec4(localPos, 1.0);

    //small vertical lift
    worldPos.y += epsilon;

    //side-to-side sway
    float sway = sin(worldPos.x * 0.25 + time * swaySpeed) * swayStrength;

    //normalized height factor so bottom moves minimally
    float heightFactor = clamp((aPos.y + epsilon) / maxPlantHeight, 0.0, 1.0);

    //apply sway
    worldPos.x += sway * heightFactor;

    //outputs
    FragPos = worldPos.xyz;
    gl_Position = camMatrix * worldPos;
    NormalID = aNormal;
    AO = aAO;
    TexCoords = aUV;
}
