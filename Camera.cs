using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Camera
{
    public Vector3 camPos;
    public Vector3 camTarget = Vector3.Zero;

    public float yaw = -90f;
    public float pitch = 10f;
    public int scrollSpeed = 150;
    

    public float distance = 100f;

    public Matrix4 view;

    // ----- Constructor -----
    public Camera()
    {
        UpdatePosition();
        UpdateView();
    }

    // ----- Convert yaw/pitch into camPos -----
    private void UpdatePosition()
    {
        float radYaw = MathHelper.DegreesToRadians(yaw);
        float radPitch = MathHelper.DegreesToRadians(pitch);

        camPos.X = camTarget.X + distance * MathF.Cos(radPitch) * MathF.Cos(radYaw);
        camPos.Y = camTarget.Y + distance * MathF.Sin(radPitch);
        camPos.Z = camTarget.Z + distance * MathF.Cos(radPitch) * MathF.Sin(radYaw);
    }

    // ----- Recompute the view matrix -----
    public void UpdateView()
    {
        view = Matrix4.LookAt(camPos, camTarget, Vector3.UnitY);
    }

    // ----- Handle input each frame -----
    public void Movement(FrameEventArgs e, KeyboardState key, MouseState mb)
    {
        // Orbiting (Right mouse button)
        if (mb.IsButtonDown(MouseButton.Right))
        {
            yaw -= (mb.PreviousX - mb.X) * 0.3f;
            pitch -= (mb.PreviousY - mb.Y) * 0.08f;

            pitch = Math.Clamp(pitch, -89f, 89f);
        }

        // Zoom
        if (mb.ScrollDelta.Y != 0)
        {
            distance -= mb.ScrollDelta.Y * scrollSpeed;
            distance = Math.Clamp(distance, Body.allBodies[Game.planetCam].radius * 5f, 100000f);
            scrollSpeed = (int)(distance * 0.05);

        }

        UpdatePosition();
        UpdateView();
        
    }
}
