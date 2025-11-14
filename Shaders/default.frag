#version 330 core

//inputs
in vec2 TexCoords;
flat in int NormalID;
in float AO;
in vec3 FragPos;

//output fragment color
out vec4 FragColor;

//global rendering var
uniform sampler2D tex0;
uniform vec3 cameraPos;

//sky color
uniform vec3 skyColor =  vec3(0.5, 0.3, 0.4);

//lighting parameters
uniform vec3 lightDir = vec3(0.2, 1.0, 0.3); //direction to the light
uniform float ambientStrength = 0.3;

//fog parameters
uniform float fogStart = 150.0;
uniform float fogEnd = 300.0;
uniform bool useAO = false;

//hardcoded face normals
const vec3 faceNormals[6] = vec3[]
(
    vec3(0.0, -1.0, 0.0), //bottom (0)
    vec3(0.0, 1.0, 0.0),  //top (1)
    vec3(0.0, 0.0, 1.0),  //front (2)
    vec3(0.0, 0.0, -1.0), //back (3)
    vec3(1.0, 0.0, 0.0),  //right (4)
    vec3(-1.0, 0.0, 0.0)  //left (5)
);

void main()
{
    //sample texture
    vec4 texColor = texture(tex0, TexCoords);
    
    if(texColor.a < 0.01) discard; //skip fully transparent
    
    //get normal from array
    vec3 normal = faceNormals[NormalID];
    
    //diffuse lighting (Lambertian)
    vec3 lightDirNorm = normalize(lightDir);
    float diff = max(dot(normal, lightDirNorm), 0.0);
    
    //combine ambient and diffuse
    vec3 ambient = ambientStrength * skyColor;
    vec3 diffuse = diff * skyColor;
    vec3 lighting = ambient + diffuse;
    
    //apply AO
    float aoFactor = clamp(AO, 0.0, 1.0);   
    if (!useAO) aoFactor = 0.0;
    
    vec3 litColor = texColor.rgb * lighting * (1.0 - aoFactor);
    
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