//base rendering of game
#version 330 core

//inputs
in vec2 TexCoords;
flat in int NormalID;
flat in int AO;
in vec3 FragPos;

//output fragment color
out vec4 FragColor;

//global rendering var
uniform sampler2D tex0;
uniform vec3 cameraPos;

//fog parameters
uniform vec3 fogColor = vec3(0.6, 0.7, 0.8); //sky/fog color
uniform float fogStart = 150.0;
uniform float fogEnd = 300.0;

//hardcoded face shading
const float faceLight[6] = float[](0.3, 1.0, 0.7, 0.7, 0.5, 0.5);

void main()
{
    //sample texture
    vec4 texColor = texture(tex0, TexCoords);
    
    if(texColor.a < 0.01) discard; //skip fully transparent
    
    //face lighting
    float aoFactor = clamp(float(AO) / 255.0, 0.0, 1.0);   
    vec3 litColor = texColor.rgb * faceLight[NormalID] * (1.0 - aoFactor);

    //compute distance to camera
    float dist = length(FragPos - cameraPos);
    float fogFactor = (dist - fogStart) / (fogEnd - fogStart);
    fogFactor = clamp(fogFactor, 0.0, 1.0);
    
    //exponential-like ramp (Minecraft style)
    fogFactor = 1.0 - exp(-pow(fogFactor, 1.5) * 4.0);

    //blend with fog color
    vec3 finalColor = mix(litColor, fogColor, fogFactor);

    FragColor = vec4(finalColor, texColor.a);
}