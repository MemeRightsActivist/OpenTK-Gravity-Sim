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
        //vbo = GL.GenBuffer();
        List<float> verts = new List<float>();
        List<uint> inds = new List<uint>();

        float x, y, z, xy;      // vertex position


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

                // texture coordinates
                

                verts.Add(x);
                verts.Add(y);
                verts.Add(z);
            }
        }

        // ----- build indices -----
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
