using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Linq;
using System.Diagnostics;
using System.IO;
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
    public static bool paused = false;
    public Matrix4[] instanceMatrices;
    public List<int> allVBOs = new List<int>();

    public static Random random;
    double accumulator = 0.0;
    double then;
    public static int frame = 0;
    public static Process graphs;
    public static int planetCam = 0;
    public static bool showGrid = true;
    public static bool showTrails = true;

    private double _timeAccumulator = 0;
    private int _frameCount = 0;
    private double _currentFps = 0;


    public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title, Location = (1000, 0) }) 
    {
        xRat = width;
        yRat = height;
        Console.WriteLine(GL.GetString(StringName.Version));
    }
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        if (!paused)
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
                
            }



            if ((int)stopwatch.Elapsed.TotalMilliseconds >= pollTime)
            {
                
                //PipeServer.SendMessage("MyNamedPipe", "Hello from the server!");
                Physics.Repeat();

                pollTime += 5;

            }

            if ((int)stopwatch.Elapsed.TotalMilliseconds >= trailTime)
            {
                trailTime += 50;
                if (showTrails)
                {
                    Trail.TrailUpdate();
                }
            }
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
            if (planetCam + 1 > Body.count - 1)
            {
                planetCam = 0;
                
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
                planetCam = Body.count - 1;
            }
            else
            {
                planetCam -= 1;
            }
        }
        if (KeyboardState.IsKeyPressed(Keys.Space))
        {
            paused = !paused;
        }
        if (KeyboardState.IsKeyPressed(Keys.G))
        {
            showGrid = !showGrid;
            if (showGrid)
            {
                Grid.opacity = 0.01f;
            }
            else
            {
                Grid.opacity = 0.0f;
            }
        }
        if (KeyboardState.IsKeyPressed(Keys.T))
        {
            showTrails = !showTrails;
        }
        if (KeyboardState.IsKeyPressed(Keys.C))
        {
            Trail.ClearTrails();
        }
        for (int i = 0; i < Body.count; i++)
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
        random = new Random();
        MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateOrOpen("MyMappedFile", 1024); // Name and size
        graphs = Process.Start("C:\\Users\\johnl\\source\\repos\\WinFormsApp1\\bin\\Debug\\net8.0-windows\\WinFormsApp1.exe");

        // Build sphere geometry
        planetSphere = new Sphere(2f, 48, 24, Color4.Black);

        //SystemCreation.StarPlanet();
        //SystemCreation.PMAstroids();
        //SystemCreation.BinaryStars();
        //SystemCreation.CircularBStars();
        //SystemCreation.ClaudeSolarSystem();

        SystemCreation.systemMethods[0]();

        //Body planetB = new Body(new Vector3(200, 0, 0), new Vector3(0, 0, -450), 2f, 20f, Color4.Purple, "Planet B");
        //Body planetC = new Body(new Vector3(350), new Vector3(0, 0, -300), 8, 40, Color4.Blue, "Planet C");
        //Body planetD = new Body(new Vector3(0, 0, 500), new Vector3(160, 0, 0), 3, 15, Color4.Green, "Planet D");



        //PipeServer.sMain();
        camera = new Camera();

        //Body.shape = new Sphere(Body.radius, 24, 12, color);


        Trail.TrailStart(Body.count, Trail.maxTrailLength);

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

        _timeAccumulator += e.Time;
        _frameCount++;

        if (_timeAccumulator >= 1.0)
        {
            _currentFps = _frameCount / _timeAccumulator;

            // Update the display (Title, UI, or Console)
            Title = $"OpenTK Engine | FPS: {_currentFps:0}";

            // Reset for the next second
            _frameCount = 0;
            _timeAccumulator = 0.0;
        }


        if (!paused)
        {
            base.OnRenderFrame(e);
            Graphics.RenderUpdate(e, camera);
            SwapBuffers();
        }
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
        graphs.Kill();
        MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateOrOpen("MyMappedFile", 1024); // Name and size
        memoryMappedFile.Dispose();
        base.OnUnload();
        Graphics.shader.Dispose();
        Graphics.gridShader.Dispose();
        Graphics.trailShader.Dispose();
    }
} 
