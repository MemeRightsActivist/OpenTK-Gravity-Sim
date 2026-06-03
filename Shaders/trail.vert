#version 430

layout(std430, binding = 0) buffer TrailPositions {
    vec4 positions[];  // xyz = position, w = timestamp
};

layout(std430, binding = 1) buffer BodyMeta {
    struct {
        int head;
        int count;
        vec2 pad;
        vec4 color;
    } bodies[];
};

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform int maxTrailLength;
uniform int planetIndex;


out vec4 vColor;
out float vAge;

void main()
{
    int pointIndex = gl_VertexID;

    // Read from circular buffer: oldest first
    int start = (bodies[planetIndex].head - bodies[planetIndex].count + maxTrailLength) % maxTrailLength;
    int actual = (start + pointIndex) % maxTrailLength;
    int globalIdx = planetIndex * maxTrailLength + actual;

    vec4 pos = positions[globalIdx];
    gl_Position = projection * view * model * vec4(pos.xyz, 1.0);

    // Fade: older points are more transparent
    float age = float(pointIndex) / float(max(bodies[planetIndex].count - 1, 1));
    vAge = age;
    vColor = bodies[planetIndex].color;
}