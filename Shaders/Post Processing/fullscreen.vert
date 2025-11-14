//post processing texture renderer
#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoords;

out vec2 TexCoords;

//just renders a full quad ontop of the screen with no transformations
void main()
{
    TexCoords = aTexCoords;
    gl_Position = vec4(aPos, 0.0, 1.0);
}