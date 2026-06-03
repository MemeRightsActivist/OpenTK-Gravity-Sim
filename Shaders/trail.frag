#version 430

in vec4 vColor;
in float vAge;

uniform float onOff;


out vec4 FragColor;

void main()
{
    FragColor = vec4(vColor.rgb, vColor.a * vAge * onOff);
}