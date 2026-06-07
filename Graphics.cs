using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Linq;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using OpenTKSim;

namespace OpenTKSim
{

    public class Graphics
    {
        public static int VertexBufferObject;
        public static int VertexArrayObject;
        public static int ElementBufferObject;
        public static int instanceColorVBO;
        public static Matrix4 projection;
        public static Shader shader;
        public static Shader gridShader;
        public static Shader trailShader;
        public static Shader lightShader;
        public static Shader haloShader;
        public static Texture texture;
        public static Grid grid;
        public static int instanceVBO;
        public static int gridVAO;
        public static int gridVBO;
        public static int trailVAO;
        public static int trailColorVBO;
        public static int trailVBO;
        public static int trailEBO;
        public static int lightVAO;
        public static Matrix4[] instanceMatrices;
        public static List<int> allVBOs = new List<int>();
        public static Sphere planetSphere;
        public static TextRenderer textRenderer;  // Add text rendering capability



        public static void Start(Camera camera)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            String s = "Shaders/";
            shader = new Shader(s + "shader.vert", s + "shader.frag");
            gridShader = new Shader(s + "grid.vert", s + "grid.frag");
            trailShader = new Shader(s + "trail.vert", s + "trail.frag");
            haloShader = new Shader(s + "halo.vert", s + "halo.frag");
            //lightShader = new Shader(s + "light.vert", s + "light.frag");
            grid = new Grid();
            planetSphere = Game.planetSphere;

            // Initialize text renderer for on-screen text
            textRenderer = new TextRenderer((int)Game.xRat, (int)Game.yRat);

            // NOTE: do not pre-fill trail indices here. Indices are generated while
            // appending vertices (in Game.OnUpdateFrame) because the vertex layout
            // is time-major / interleaved (planet0@t0, planet1@t0, ..., planet0@t1, ...)
            // and index values must reference the correct vertex indices as they are
            // added. The EBO is allocated below; we will update actual index bytes
            // with BufferSubData when indices change.






            Vector4[] instanceColors = new Vector4[Body.count];
            for (int i = 0; i < Body.count; i++)
            {
                instanceColors[i] = new Vector4(Body.allBodies[i].color.R, Body.allBodies[i].color.G, Body.allBodies[i].color.B, Body.allBodies[i].color.A);
            }

            instanceColorVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceColorVBO);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          instanceColors.Length * Vector4.SizeInBytes,
                          instanceColors,
                          BufferUsageHint.DynamicDraw);

            int[] isEmissiveFlags = new int[Body.count];
            for (int i = 0; i < Body.count; i++)
            {
                // Assuming you have a property to identify light sources
                isEmissiveFlags[i] = Body.allBodies[i].isLightSource ? 1 : 0;
            }

            int emissiveVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, emissiveVBO);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          isEmissiveFlags.Length * sizeof(int),
                          isEmissiveFlags,
                          BufferUsageHint.StaticDraw);

            // Build transforms for each sphere instance
            instanceMatrices = new Matrix4[Body.count];
            for (int i = 0; i < Body.count; i++)
            {
                instanceMatrices[i] =
                    Matrix4.CreateScale(Body.allBodies[i].radius) *
                    Matrix4.CreateTranslation(Body.allBodies[i].position);
            }





            // === CREATE AND BIND VAO ===
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            // Setup instance color attribute (location 6)
            GL.EnableVertexAttribArray(6);
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceColorVBO);
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes, 0);
            GL.VertexAttribDivisor(6, 1);

            // Setup emissive flag attribute (location 7) - MOVED HERE
            GL.EnableVertexAttribArray(7);
            GL.BindBuffer(BufferTarget.ArrayBuffer, emissiveVBO);
            GL.VertexAttribIPointer(7, 1, VertexAttribIntegerType.Int, 0, IntPtr.Zero);
            GL.VertexAttribDivisor(7, 1);

            // === VBO for sphere vertices ===
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer,
                            planetSphere.vertices.Length * sizeof(float),
                            planetSphere.vertices,
                            BufferUsageHint.StaticDraw);





            // === EBO for indices (bound while VAO is active!) ===
            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                            planetSphere.indices.Length * sizeof(uint),
                            planetSphere.indices,
                            BufferUsageHint.StaticDraw);

            int stride = 6 * sizeof(float);

            // --- Vertex attributes ---
            // Position (layout = 0)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            // Normal (location = 1)
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));



            // === Instance buffer ===
            instanceVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
            GL.BufferData(BufferTarget.ArrayBuffer,
                            instanceMatrices.Length * Marshal.SizeOf<Matrix4>(),
                            instanceMatrices,
                            BufferUsageHint.DynamicDraw);

            int vec4Size = sizeof(float) * 4;
            int mat4Size = vec4Size * 4;

            // A mat4 needs 4 consecutive attribute locations
            for (int i = 0; i < 4; i++)
            {
                int attribLocation = 2 + i; // locations 2,3,4,5
                GL.EnableVertexAttribArray(attribLocation);
                GL.VertexAttribPointer(attribLocation, 4, VertexAttribPointerType.Float,
                                        false, mat4Size, i * vec4Size);
                GL.VertexAttribDivisor(attribLocation, 1); // advance per instance
            }

            // Unbind array buffer (VAO keeps state, EBO binding stays with VAO)
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            Grid.GridA();



            // Grid VAO
            gridVAO = GL.GenVertexArray();
            GL.BindVertexArray(gridVAO);


            // Grid VBO
            gridVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, gridVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, grid.gridVertices.Length * sizeof(float), grid.gridVertices, BufferUsageHint.StaticDraw);

            // Grid EBO (element buffer) for line indices
            int gridEBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, gridEBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, grid.gridIndices.Length * sizeof(uint), grid.gridIndices, BufferUsageHint.StaticDraw);

            // Position attribute (same as sphere's attrib 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Unbind
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // Position attribute (same as sphere's attrib 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Unbind
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // Light VAO
            lightVAO = GL.GenVertexArray();
            GL.BindVertexArray(lightVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

        }

        public static void GetPlanetPositions(Vector4[] posRad, int count)
        {

        }

        public static void RenderUpdate(FrameEventArgs e, Camera camera)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 model = Matrix4.Identity;
            Matrix4 view = camera.view;
            projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45.0f),
                Game.xRat / Game.yRat,
                1.0f,
                100000.0f);





            // Get light-emitting bodies for both glow and lighting
            List<Body> lightBodies = Body.allBodies.Where(b => b.isLightSource).ToList();

            // === Sphere drawing ===
            shader.Use();

            // Pass light positions
            for (int i = 0; i < lightBodies.Count; i++)
            {
                int location = GL.GetUniformLocation(shader.Handle, $"lightPositions[{i}]");
                Vector3 pos = lightBodies[i].position;
                GL.Uniform3(location, pos);

                location = GL.GetUniformLocation(shader.Handle, $"lightColors[{i}]");
                Vector3 color = new Vector3(lightBodies[i].color.R,
                                             lightBodies[i].color.G,
                                             lightBodies[i].color.B);
                GL.Uniform3(location, color);
            }

            // Pass camera position
            int viewPosLoc = GL.GetUniformLocation(shader.Handle, "viewPos");
            GL.Uniform3(viewPosLoc, camera.camPos);

            int numLightsLoc = GL.GetUniformLocation(shader.Handle, "numLights");
            GL.Uniform1(numLightsLoc, lightBodies.Count);
            int loc;

            loc = GL.GetUniformLocation(shader.Handle, "model");
            GL.UniformMatrix4(loc, false, ref model);
            loc = GL.GetUniformLocation(shader.Handle, "view");
            GL.UniformMatrix4(loc, false, ref view);
            loc = GL.GetUniformLocation(shader.Handle, "projection");
            GL.UniformMatrix4(loc, false, ref projection);



            // Bind VAO and draw all sphere instances
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElementsInstanced(PrimitiveType.Triangles,
                                        planetSphere.indices.Length,
                                        DrawElementsType.UnsignedInt,
                                        IntPtr.Zero,
                                        Body.count);

            // === Halo Rendering (glow effect for light sources) ===
            if (lightBodies.Count > 0)
            {
                // Enable additive blending so glow ADDS brightness (doesn't dim the original)
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

                // Disable depth testing so halo always renders (prevents z-fighting)
                //GL.Disable(EnableCap.DepthTest);
                GL.DepthMask(false);

                haloShader.Use();

                // Set uniforms (only once, shared by all layers)
                loc = GL.GetUniformLocation(haloShader.Handle, "view");
                GL.UniformMatrix4(loc, false, ref view);
                loc = GL.GetUniformLocation(haloShader.Handle, "projection");
                GL.UniformMatrix4(loc, false, ref projection);
                loc = GL.GetUniformLocation(haloShader.Handle, "viewPos");
                GL.Uniform3(loc, camera.camPos);

                // Draw 10 layers of halos, from largest (furthest) to smallest (closest)
                int numLayers = 100;
                float minScale = 1.1f;  // Start just outside the original sphere
                float maxScale = 10.0f;  // End at 4x the size
                float maxAlpha = 0.99f;  // Maximum alpha for innermost layer
                float minAlpha = 0.5f; // Minimum alpha for outermost layer

                // Draw from LARGEST to SMALLEST (back to front for proper blending)
                for (int layer = numLayers - 1; layer >= 0; layer--)
                {
                    float t = (float)layer / (numLayers - 1); // 0.0 to 1.0
                    float scale = minScale + (maxScale - minScale) * (1.0f - t); // Decreases with layer
                    float alpha = minAlpha + (maxAlpha - minAlpha) * t; // Increases with layer

                    // Pass alpha to shader as a uniform
                    loc = GL.GetUniformLocation(haloShader.Handle, "layerAlpha");
                    GL.Uniform1(loc, alpha);

                    // Create matrices for this layer
                    Matrix4[] haloMatrices = new Matrix4[lightBodies.Count];
                    Vector4[] haloColors = new Vector4[lightBodies.Count];

                    for (int i = 0; i < lightBodies.Count; i++)
                    {
                        haloMatrices[i] =
                            Matrix4.CreateScale(lightBodies[i].radius * scale) *
                            Matrix4.CreateTranslation(lightBodies[i].position);

                        haloColors[i] = new Vector4(
                            lightBodies[i].color.R,
                            lightBodies[i].color.G,
                            lightBodies[i].color.B,
                            1.0f);
                    }

                    // Upload instance data for this layer
                    GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);
                    GL.BufferSubData(BufferTarget.ArrayBuffer,
                                     IntPtr.Zero,
                                     haloMatrices.Length * Marshal.SizeOf<Matrix4>(),
                                     haloMatrices);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, instanceColorVBO);
                    GL.BufferSubData(BufferTarget.ArrayBuffer,
                                     IntPtr.Zero,
                                     haloColors.Length * Vector4.SizeInBytes,
                                     haloColors);

                    // Draw this layer
                    GL.BindVertexArray(VertexArrayObject);
                    GL.DrawElementsInstanced(PrimitiveType.Triangles,
                                             planetSphere.indices.Length,
                                             DrawElementsType.UnsignedInt,
                                             IntPtr.Zero,
                                             lightBodies.Count);
                }

                // Restore state
                GL.Enable(EnableCap.DepthTest);
                GL.DepthMask(true);
                GL.Disable(EnableCap.Blend);
            }

            if (Game.showGrid)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                // === Grid drawing ===
                gridShader.Use();
                loc = GL.GetUniformLocation(gridShader.Handle, "model");
                GL.UniformMatrix4(loc, false, ref model);
                loc = GL.GetUniformLocation(gridShader.Handle, "view");
                GL.UniformMatrix4(loc, false, ref view);
                loc = GL.GetUniformLocation(gridShader.Handle, "projection");
                GL.UniformMatrix4(loc, false, ref projection);

                loc = GL.GetUniformLocation(gridShader.Handle, "opacity");
                GL.Uniform1(loc, Grid.opacity);


                GL.BindVertexArray(gridVAO);
                // Draw grid as lines using the element buffer
                GL.DrawElements(PrimitiveType.Lines, grid.gridIndices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
            // === Light drawing? ===


            // === Trail drawing ===
            if (Game.showTrails)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                trailShader.Use();
                loc = GL.GetUniformLocation(trailShader.Handle, "model");
                GL.UniformMatrix4(loc, false, ref model);
                loc = GL.GetUniformLocation(trailShader.Handle, "view");
                GL.UniformMatrix4(loc, false, ref view);
                loc = GL.GetUniformLocation(trailShader.Handle, "projection");
                GL.UniformMatrix4(loc, false, ref projection);

                loc = GL.GetUniformLocation(trailShader.Handle, "onOff");
                GL.Uniform1(loc, Trail.opacity);

                int maxLoc = GL.GetUniformLocation(trailShader.Handle, "maxTrailLength");
                GL.Uniform1(maxLoc, Trail.maxTrailLength);

                int piLoc = GL.GetUniformLocation(trailShader.Handle, "planetIndex");

                // Bind SSBOs
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, Trail.trailsSSBO);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, Trail.bodyMetaSSBO);

                // Need an empty VAO for shader-based vertex pulling
                if (trailVAO == 0)
                {
                    trailVAO = GL.GenVertexArray();
                }
                GL.BindVertexArray(trailVAO);

                for (int i = 0; i < Body.count; i++)
                {
                    GL.Uniform1(piLoc, i);
                    int count = Trail.bodyMeta[i].count;
                    if (count > 1)
                        GL.DrawArrays(PrimitiveType.LineStrip, 0, count);
                }

                GL.BindVertexArray(0);
            }

            // === Text Rendering (always rendered last, on top of everything) ===
            // Example: Display FPS counter
            textRenderer.DrawText($"FPS: {Game.frame}", 10, 10, 1.5f);

            // Example: Display camera position
            textRenderer.DrawText($"Camera: {camera.camPos.X:F1}, {camera.camPos.Y:F1}, {camera.camPos.Z:F1}", 10, 40, 1.0f);

            // Example: Display body count
            textRenderer.DrawText($"Bodies: {Body.count}", 10, 60, 1.0f);
        }
    }
}
