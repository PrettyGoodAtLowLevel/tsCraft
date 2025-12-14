#version 330 core

//inputs
in vec2 TexCoords;
in vec3 FragPos;
in vec3 lightColor;
in float skyLight;
flat in int NormalID;

//output fragment color
out vec4 FragColor;

//global rendering var
uniform sampler2D tex0;
uniform vec3 cameraPos;

//sky color
uniform vec3 skyColor;

//fog parameters
uniform float fogStart = 150.0;
uniform float fogEnd = 300.0;

//shade the colors slightly based on face
const float faceLight[6] = float[]
( 
    0.4, 1.0,
    0.7, 0.7,
    0.6, 0.6
);

void main()
{
    //sample texture
    vec4 texColor = texture(tex0, TexCoords);
    
    if(texColor.a < 0.01) discard; //skip fully transparent

    //block and sky light
    vec3 skyLighting = pow(skyLight, 2.0) * skyColor;
    vec3 blockLighting = pow(lightColor, vec3(2.0));
    vec3 lightFinal = max(blockLighting, skyLighting);   
    
    //combine texture, lighting, and block light
    vec3 ambience = vec3(0.01);
    vec3 litColor = texColor.rgb  * (lightFinal + ambience); // ADDITIVE
    litColor *= faceLight[NormalID];
    
    //compute distance to camera
    float dist = length(FragPos - cameraPos);
    float fogFactor = (dist - fogStart) / (fogEnd - fogStart);
    fogFactor = clamp(fogFactor, 0.0, 1.0);
    
    //exponential-like ramp (Minecraft style)
    fogFactor = 1.0 - exp(-pow(fogFactor, 1.5) * 4.0);
    //blend with fog color
    vec3 finalColor = mix(litColor, skyColor, fogFactor);
    FragColor = vec4(finalColor, texColor.a);
}