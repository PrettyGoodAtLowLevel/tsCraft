//adds very small effects ontop of the screen for a final nice rendering result
//chromatic aberration creates realistic light refractions on the edge of the screen
//we also can change the saturation, which is how much the colors pop, high saturation means very colorful, and vice versa
//vignette makes the screen slightly darker on the edges for a realistic veiwing expierence
//the tint literally just tints the screen, used for underwater effects, or damaging or what not
#version 330 core
uniform sampler2D sceneTex;        //framebuffer texture
uniform float caStrength;          //chromatic aberration intensity
uniform float saturation;          //1.0 = normal, >1 = more saturated
uniform float vignetteStrength;    //0 = no vignette, 1 = full dark edges
uniform vec3 tintColor;            //RGB tint, e.g., red flash on damage
uniform float tintIntensity;       //0 = no tint, 1 = full tint

in vec2 TexCoords;
out vec4 FragColor;

void main()
{
    //compute normalized distance from screen center
    vec2 center = vec2(0.5, 0.5);
    vec2 dir = TexCoords - center;
    float dist = length(dir); //0 at center, ~0.707 at corners

    //chromatic aberration offsets
    vec2 offset = dir * caStrength * dist;

    float r = texture(sceneTex, TexCoords + offset).r;
    float g = texture(sceneTex, TexCoords).g;
    float b = texture(sceneTex, TexCoords - offset).b;

    vec3 color = vec3(r, g, b);

    //saturation
    float gray = dot(color, vec3(0.2126, 0.7152, 0.0722));
    color = mix(vec3(gray), color, saturation);

    //vignette
    float vignette = 1.0 - dist * vignetteStrength; 
    vignette = clamp(vignette, 0.0, 1.0);
    color *= vignette;

    //tint (multiply or lerp)
    color = mix(color, tintColor, tintIntensity);

    FragColor = vec4(color, 1.0);
}