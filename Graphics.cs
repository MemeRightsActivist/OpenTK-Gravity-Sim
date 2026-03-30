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
        public static Matrix4 projection;
        public static Shader shader;
        public static Shader gridShader;
        public static Shader trailShader;
        public static Texture texture;
        public static Grid grid;
        public static int instanceVBO;
        public static int gridVAO;
        public static int gridVBO;
        public static int trailVAO;
        public static int trailColorVBO;
        public static int trailVBO;
        public static int trailEBO;
        public static Matrix4[] instanceMatrices;
        public static List<int> allVBOs = new List<int>();
        public static Sphere planetSphere;



        public static void Start(Camera camera)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            shader = new Shader("shader.vert", "shader.frag");
            gridShader = new Shader("grid.vert", "grid.frag");
            trailShader = new Shader("trail.vert", "trail.frag");
            grid = new Grid();
            planetSphere = Game.planetSphere;

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

            int instanceColorVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceColorVBO);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          instanceColors.Length * Vector4.SizeInBytes,
                          instanceColors,
                          BufferUsageHint.DynamicDraw);

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

            GL.EnableVertexAttribArray(6); // choose a free attribute location
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceColorVBO);
            GL.VertexAttribPointer(6, 3, VertexAttribPointerType.Float, false, Vector4.SizeInBytes, 0);
            GL.VertexAttribDivisor(6, 1); // <-- make it instanced

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

            int stride = 3 * sizeof(float);

            // --- Vertex attributes ---
            // Position (layout = 0)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);



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



            //Trail SSBO 0



            // Trail VAO

            //trailVAO = GL.GenVertexArray();
            //GL.BindVertexArray(trailVAO);

            //trailColorVBO = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ArrayBuffer, trailColorVBO);
            //GL.BufferData(BufferTarget.ArrayBuffer, Body.trailColors.Length * sizeof(float), Body.trailColors, BufferUsageHint.DynamicDraw);

            //GL.EnableVertexAttribArray(2); // choose a free attribute location
            //GL.BindBuffer(BufferTarget.ArrayBuffer, trailColorVBO);
            //GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 4*sizeof(float), 0);
            ////GL.VertexAttribDivisor(2, 1); // <-- make it instanced

            //trailVBO = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ArrayBuffer, trailVBO);
            //// Allocate GPU storage for trail vertex positions (streamed). Use DynamicDraw
            //// because we will update portions frequently via BufferSubData.
            //GL.BufferData(BufferTarget.ArrayBuffer, Body.allPaths.Length * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            //// === EBO for indices (bound while VAO is active!) ===
            //trailEBO = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, trailEBO);
            //// Allocate GPU storage for indices. We'll update with BufferSubData when indices are appended.
            //GL.BufferData(BufferTarget.ElementArrayBuffer, Body.trailIndices.Length * sizeof(uint), IntPtr.Zero, BufferUsageHint.DynamicDraw);



            // Position attribute (same as sphere's attrib 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Unbind
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

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
                0.001f,
                1000000.0f);







            // === Sphere drawing ===
            shader.Use();
            int loc;

            loc = GL.GetUniformLocation(shader.Handle, "model");
            GL.UniformMatrix4(loc, false, ref model);
            loc = GL.GetUniformLocation(shader.Handle, "view");
            GL.UniformMatrix4(loc, false, ref view);
            loc = GL.GetUniformLocation(shader.Handle, "projection");
            GL.UniformMatrix4(loc, false, ref projection);



            // Bind VAO and draw all instances
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElementsInstanced(PrimitiveType.Triangles,
                                        planetSphere.indices.Length,
                                        DrawElementsType.UnsignedInt,
                                        IntPtr.Zero,
                                        Body.count);

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

            // === Trail drawing ===
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
    }
}
