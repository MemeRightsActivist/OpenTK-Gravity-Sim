#version 330 core
in vec4 vColor;
in vec3 FragPos;
in vec3 Normal;
flat in int isEmissive;  // ADD: flat means no interpolation between vertices

out vec4 FragColor;

// Light properties
uniform vec3 lightPositions[10];
uniform vec3 lightColors[10];
uniform int numLights;
uniform vec3 viewPos;

void main()
{
    // If this object emits light, just render it at full brightness
    if (isEmissive == 1)
    {
        FragColor = vColor * 10.0;
        return;  // Skip all lighting calculations
    }
    
    // Normal lighting for non-emissive objects
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);
    
    vec3 ambient = 0.002 * vColor.rgb;
    vec3 result = ambient;
    
    for(int i = 0; i < numLights; i++)
    {
        vec3 lightDir = normalize(lightPositions[i] - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = diff * lightColors[i] * vColor.rgb * 30;
        
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
        vec3 specular = 0.5 * spec * lightColors[i];
        
        float distance = length(lightPositions[i] - FragPos);
        float attenuation = 1.0 / (1.0 + 0.00001 * distance + 0.000001 * distance * distance);
        
        result += (diffuse + specular) * attenuation;
    }
    
    FragColor = vec4(result, vColor.a);
}