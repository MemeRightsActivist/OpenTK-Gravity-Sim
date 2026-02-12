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


public class Game : GameWindow
{
    public static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    Camera camera;
    Body planetA;
    public static Sphere planetSphere;
    public static float xRat;
    public static float yRat;
    int instanceVBO;
    public int pollTime = 0;
    public int trailTime = 0;
    
    public Matrix4[] instanceMatrices;
    public List<int> allVBOs = new List<int>();

    double accumulator = 0.0;
    double then;
    public static int frame = 0;
    public static Process graphs;
    int planetCam = 0;



    public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) 
    {
        xRat = width;
        yRat = height;
    }
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);
        frame += 1;
        double now = stopwatch.Elapsed.TotalSeconds;
        if (then == 0.0)
        {
            then = now;
        }
        double frameTime = now - then;
        then = now;

        accumulator += frameTime;

        while (accumulator >= Physics.dTime)
        {
            Physics.Gravity((float)Physics.dTime);
            accumulator -= Physics.dTime;
            foreach (Body body in Body.allBodies)
            {
                //body.radius = body.velocity.Length;
            }
            //Console.WriteLine($"Real Time: {stopwatch.Elapsed.TotalSeconds}, Accumulator: {accumulator}");
        }

        
        if ((int)stopwatch.Elapsed.TotalMilliseconds >= pollTime)
        {
            //Console.WriteLine("OK");
            //PipeServer.SendMessage("MyNamedPipe", "Hello from the server!");
            Physics.Repeat();
            foreach (Body body in Body.allBodies)
            {
                //body.path.Add(body.position);
                body.path.Add(body.position[0]);
                body.path.Add(body.position[1]);
                body.path.Add(body.position[2]);

            }
            pollTime += 5;
            
        }

        if ((int) stopwatch.Elapsed.TotalMilliseconds >= trailTime)
        {
            //new Sphere();
        }

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        if (KeyboardState.IsKeyDown(Keys.Comma))
        {
            Physics.dTime -= 1f / 120f;
        }

        if (KeyboardState.IsKeyDown(Keys.Period))
        {
            Physics.dTime += 1f / 120f;
        }

        if (KeyboardState.IsKeyReleased(Keys.W))
        {
            planetA.position += new Vector3(0f, 1f, 0f);
        }
        if (KeyboardState.IsKeyPressed(Keys.Right))
        {
            if (planetCam + 1 > Body.allBodies.Count - 1)
            {
                planetCam = 0;
                Console.WriteLine("Here");
            }
            else
            {
                planetCam += 1;
            }
        }
        if (KeyboardState.IsKeyPressed(Keys.Left))
        {
            if (planetCam - 1 < 0)
            {
                planetCam = Body.allBodies.Count - 1;
            }
            else
            {
                planetCam -= 1;
            }
        }
        for (int i = 0; i < Body.allBodies.Count; i++)
        {
            Graphics.instanceMatrices[i] =
                Matrix4.CreateScale(Body.allBodies[i].radius) *
                Matrix4.CreateTranslation(Body.allBodies[i].position);
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, Graphics.instanceVBO);
        GL.BufferSubData(
            BufferTarget.ArrayBuffer,
            IntPtr.Zero,
            Graphics.instanceMatrices.Length * Marshal.SizeOf<Matrix4>(),
            Graphics.instanceMatrices
        );
        camera.camTarget = Body.allBodies[planetCam].position;


    }
    protected override void OnLoad()
    {
        base.OnLoad();
        MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateOrOpen("MyMappedFile", 1024); // Name and size
        graphs = Process.Start("C:\\Users\\johnl\\source\\repos\\WinFormsApp1\\bin\\Debug\\net8.0-windows\\WinFormsApp1.exe");

        // Build sphere geometry
        planetSphere = new Sphere(2f, 24, 12, Color4.Black);

        planetA = new Body(new Vector3(0), new Vector3(0), 35f, 1000f, Color4.Yellow, "Planet A");
        Body planetB = new Body(new Vector3(200, 0, 0), new Vector3(0, 0, -450), 2f, 20f, Color4.Purple, "Planet B");
        Body planetC = new Body(new Vector3(350), new Vector3(0, 0, -300), 8, 40, Color4.Blue, "Planet C");

        //PipeServer.sMain();
        camera = new Camera();


        then = 0.0;
        Graphics.Start(camera);
  
        camera.distance = Body.allBodies.MaxBy(b => b.radius).radius * 25;
        stopwatch.Start();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        var input = KeyboardState;
        var mb = MouseState;
        camera.Movement(e, input, mb);



        base.OnRenderFrame(e);
        Graphics.RenderUpdate(e, camera);
        SwapBuffers();
    }

    
    public static void MMF()
    {
        string filePath = "C:\\Users\\johnl\\source\\repos\\OpenTKSim\\Data\\SimData.txt"; // Or any other path


        // Create a memory-mapped file from an existing file or create a new one
        MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateOrOpen("MyMappedFile", 1024); // Name and size
        
        // Create a view accessor to write data
        using (MemoryMappedViewAccessor accessor = memoryMappedFile.CreateViewAccessor(0, 1024))
        {
            string message = "5";
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            // Write the bytes to the memory-mapped file
            accessor.WriteArray(0, bytes, 0, bytes.Length);

            Console.WriteLine($"Data written to memory-mapped file. {bytes}");
        
        }

    }

    public static void ReadMMF()
    {
        string filePath = "C:\\Users\\johnl\\source\\repos\\OpenTKSim\\Data\\SimData.txt"; // Or any other path

        //Example of reading the data(can be in another process)
        using (MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateOrOpen("MyMappedFile", 1024)) // Name and size
        {
            using (MemoryMappedViewAccessor accessor = memoryMappedFile.CreateViewAccessor(0, 1024))
            {
                byte[] buffer = new byte[1024];
                accessor.ReadArray(0, buffer, 0, buffer.Length);
                string readMessage = Encoding.UTF8.GetString(buffer).TrimEnd('\0'); // Remove null terminators
                


                Console.WriteLine($"Data read from memory-mapped file: {int.Parse(readMessage) + 5}");
            }
        }
    }


    

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
        xRat = e.Width;
        yRat = e.Height;
    }
        
    protected override void OnUnload()
    {
        //PipeServer.namedPipeServer.Dispose();
        graphs.Kill();
        MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateOrOpen("MyMappedFile", 1024); // Name and size
        memoryMappedFile.Dispose();
        base.OnUnload();
        Graphics.shader.Dispose();
    }
} 
