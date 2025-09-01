//base rendering of the game
#version 330 core
layout(location = 0) in float aXPos; //short mapped to float
layout(location = 1) in float aYPos; //half precision mapped to float
layout(location = 2) in float aZPos; //short mapped to float
layout(location = 3) in vec2 aUV;
layout(location = 4) in int aNormal;

//output variables to frag shader
out vec2 TexCoords;
flat out int NormalID;
out vec3 FragPos;

//positioning and transformations
uniform mat4 camMatrix;
uniform mat4 model;
uniform float uChunkSize = 32.0;

void main()
{
    //convert to normal vec3
    float x = (aXPos / 32767.0) * uChunkSize;
    float z = (aZPos / 32767.0) * uChunkSize;
    float y = aYPos;
    vec3 aPos = vec3(x, y, z);

    //calculate screen position
    vec4 worldPos = model * vec4(aPos, 1.0);  
    gl_Position = camMatrix * worldPos;

    //upload attributes to frag shader
    NormalID = aNormal;
    TexCoords = aUV;
    FragPos = worldPos.xyz; //pass world-space position to fragment shader
}