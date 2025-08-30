//base rendering of the game
#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aUV;
layout(location = 2) in int aNormal;
layout(location = 3) in int aAO;   

//output variables to frag shader
out vec2 TexCoords;
flat out int NormalID;
flat out int AO;
out vec3 FragPos;

//positioning and transformations
uniform mat4 camMatrix;
uniform mat4 model;

void main()
{
    //calculate world position
    vec4 worldPos = model * vec4(aPos, 1.0);  
    gl_Position = camMatrix * worldPos;

    //upload attributes to frag shader
    NormalID = aNormal;
    TexCoords = aUV;
    AO = aAO;
    FragPos = worldPos.xyz; //pass world-space position to fragment shader
}