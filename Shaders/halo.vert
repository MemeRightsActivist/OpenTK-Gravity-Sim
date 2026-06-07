#version 330 core

// Vertex position (sphere geometry centered at origin)
layout(location = 0) in vec3 aPos;

// Per-instance data
layout(location = 2) in mat4 instanceMatrix;  // Model matrix for this instance
layout(location = 6) in vec4 instanceColor;   // Color for this instance

// Output to fragment shader
out vec3 FragPos;      // Fragment position in world space
out vec3 SphereCenter; // Center of the sphere in world space
out vec4 Color;        // Instance color
out float Radius;      // Radius of the halo sphere

uniform mat4 view;
uniform mat4 projection;

void main()
{
	// Transform vertex position to world space
	vec4 worldPos = instanceMatrix * vec4(aPos, 1.0);
	FragPos = worldPos.xyz;

	// Extract sphere center from the model matrix (last column, xyz components)
	SphereCenter = vec3(instanceMatrix[3][0], instanceMatrix[3][1], instanceMatrix[3][2]);

	// Calculate radius from the scale in the model matrix
	// The scale is in the diagonal of the 3x3 upper-left submatrix
	Radius = length(vec3(instanceMatrix[0][0], instanceMatrix[1][1], instanceMatrix[2][2])) / 1.732; // divide by sqrt(3) for avg

	// Pass color to fragment shader
	Color = instanceColor;

	// Transform to clip space
	gl_Position = projection * view * worldPos;
}
