#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in mat4 instanceModel;
layout (location = 6) in vec4 aInstanceColor;
layout (location = 7) in int aIsEmissive;  // ADD: per-instance emissive flag

uniform mat4 view;
uniform mat4 projection;

out vec4 vColor;
out vec3 FragPos;
out vec3 Normal;
flat out int isEmissive;  // ADD: pass to fragment shader

void main(void)
{
    vec4 worldPos = instanceModel * vec4(aPos, 1.0);
    FragPos = vec3(worldPos);
    
    // Transform normal (use transpose of inverse for non-uniform scaling)
    Normal = mat3(transpose(inverse(instanceModel))) * aNormal;
    
    gl_Position = projection * view * worldPos;
    vColor = aInstanceColor;
    isEmissive = aIsEmissive;  // Pass through
}