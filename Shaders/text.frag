#version 330 core

// Input: texture coordinates from vertex shader
in vec2 TexCoords;

// Output: final pixel color
out vec4 FragColor;

// Uniform: the font texture (bitmap containing all characters)
uniform sampler2D fontTexture;

void main()
{
	// Sample the font texture at the current texture coordinate
	// The texture is single-channel (red), use it as alpha
	float alpha = texture(fontTexture, TexCoords).r;

	// Text color (white for visibility)
	vec3 textColor = vec3(1.0, 1.0, 1.0);

	// Use the sampled alpha channel for transparency
	// This allows the character shape to show through
	FragColor = vec4(textColor, alpha);
}
