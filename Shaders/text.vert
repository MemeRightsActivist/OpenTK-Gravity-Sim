#version 330 core

// Input: vertex position and texture coordinates
layout (location = 0) in vec2 aPosition;  // Screen position (x, y)
layout (location = 1) in vec2 aTexCoord;  // Texture coordinate (u, v)

// Output: pass texture coordinates to fragment shader
out vec2 TexCoords;

// Uniform: projection matrix (converts screen coordinates to clip space)
uniform mat4 projection;

void main()
{
	// Transform screen coordinates to clip space (-1 to 1)
	gl_Position = projection * vec4(aPosition, 0.0, 1.0);

	// Pass texture coordinates through
	TexCoords = aTexCoord;
}
