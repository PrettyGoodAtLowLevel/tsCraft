#version 330 core

layout(location = 0) in vec3 aPosition;

uniform mat4 projection;
uniform mat4 view;

out vec3 WorldDir;

void main()
{
    //remove translation
    mat4 rotView = mat4(mat3(view));
    vec4 pos = projection * rotView * vec4(aPosition, 1.0);

    gl_Position = pos.xyww;

    WorldDir = normalize(aPosition);
}
