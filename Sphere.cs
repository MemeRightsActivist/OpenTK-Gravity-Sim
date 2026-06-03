using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

public class Sphere
{
    public static List<Sphere> allSpheres = new List<Sphere>();
    public float[] vertices;
    public uint[] indices;
    public int vbo;
    public float radius;
    
    public Sphere(float radius, int sectorCount, int stackCount, Color4 color)
    {
        this.radius = radius;
        List<float> verts = new List<float>();
        List<uint> inds = new List<uint>();

        float x, y, z, xy;      // vertex position
        float nx, ny, nz;       // vertex normal

        float sectorStep = 2 * MathF.PI / sectorCount;
        float stackStep = MathF.PI / stackCount;
        float sectorAngle, stackAngle;

        // ----- build vertices -----
        for (int i = 0; i <= stackCount; ++i)
        {
            stackAngle = MathF.PI / 2 - i * stackStep; // from pi/2 to -pi/2
            xy = radius * MathF.Cos(stackAngle);       // r * cos(u)
            z = radius * MathF.Sin(stackAngle);        // r * sin(u)

            for (int j = 0; j <= sectorCount; ++j)
            {
                sectorAngle = j * sectorStep;          // from 0 to 2pi

                // vertex position
                x = xy * MathF.Cos(sectorAngle);
                y = xy * MathF.Sin(sectorAngle);

                // normal (normalized position since sphere is at origin)
                // Since we're building with 'radius', we can divide by it
                nx = x / radius;
                ny = y / radius;
                nz = z / radius;
                
                // Alternative: calculate length and normalize
                // float length = MathF.Sqrt(x*x + y*y + z*z);
                // nx = x / length;
                // ny = y / length;
                // nz = z / length;

                // Add position (3 floats)
                verts.Add(x);
                verts.Add(y);
                verts.Add(z);
                
                // Add normal (3 floats)
                verts.Add(nx);
                verts.Add(ny);
                verts.Add(nz);
            }
        }

        // ----- build indices (unchanged) -----
        int k1, k2;
        for (int i = 0; i < stackCount; ++i)
        {
            k1 = i * (sectorCount + 1);     // beginning of current stack
            k2 = k1 + sectorCount + 1;      // beginning of next stack

            for (int j = 0; j < sectorCount; ++j, ++k1, ++k2)
            {
                if (i != 0)
                {
                    inds.Add((uint)k1);
                    inds.Add((uint)k2);
                    inds.Add((uint)(k1 + 1));
                }

                if (i != (stackCount - 1))
                {
                    inds.Add((uint)(k1 + 1));
                    inds.Add((uint)k2);
                    inds.Add((uint)(k2 + 1));
                }
            }
        }

        vertices = verts.ToArray();
        indices = inds.ToArray();
        allSpheres.Add(this);
    }
}
