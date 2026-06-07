#version 330 core

in vec3 FragPos;      // Fragment position in world space
in vec3 SphereCenter; // Center of the sphere
in vec4 Color;        // Instance color
in float Radius;      // Radius of the halo sphere

out vec4 FragColor;

uniform vec3 viewPos;     // Camera position
uniform float layerAlpha; // Alpha for this layer (passed from C#)

void main()
{
	// Use the layer alpha passed from C# code
	FragColor = vec4((Color.rgb / 255.0) * 2, layerAlpha);
}
