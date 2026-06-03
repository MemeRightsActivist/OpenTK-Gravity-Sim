# GitHub Copilot Instructions

## Learning Philosophy
- **Always correct programming misconceptions, incorrect terminology, or technical errors BEFORE answering the question**
- Prioritize educational explanations that help understanding
- Value accuracy and learning over avoiding corrections
- Explain the "why" behind solutions, not just the "how"

## Project Context
- **OpenTK/OpenGL** graphics and rendering project
- **N-body gravity simulation** with real-time 3D visualization
- Uses **instanced rendering** for performance optimization
- Targeting **.NET 8**

## Technical Stack
- OpenTK (OpenGL bindings for .NET)
- GLSL shaders (vertex and fragment shaders)
- Compute shaders and SSBOs for GPU-based physics
- Real-time camera controls and interactive simulation

## Code Style Preferences
- Use clear, descriptive variable names
- Prefer explicit code over clever shortcuts
- Add comments only when explaining complex graphics/physics concepts
- Keep shader code clean and well-structured

## OpenGL/Graphics Specific
- Follow OpenGL state machine best practices
- Properly manage VAO/VBO/EBO bindings
- Always check shader compilation errors
- Use instancing for rendering multiple similar objects
- Consider depth buffer precision for large-scale scenes
