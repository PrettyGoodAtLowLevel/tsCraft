#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aColor;

out vec3 color;

//positioning and transformations
uniform mat4 camMatrix;
uniform mat4 model;

void main()
{
    //calculate screen position
    vec4 worldPos = model * vec4(aPos, 1.0);  
    gl_Position = camMatrix * worldPos;

    color = aColor;
}
