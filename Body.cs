using OpenTK.Mathematics;
using OpenTKSim;
using ScottPlot.DataViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



public class Body
{
    public Vector3 position;
    public Vector3 velocity;
    public static float velocityOffset = 0.1f;
    public float radius;
    public float mass;
    public static float massOffset = 500f;
    public float orbitalEnergy;
    public Color4 color;
    public bool emitsLight;
    public string name;
    public BodyStruct metaData;
    public static int count = 0;
    public Sphere shape;
    public static float[] allPaths;
    
    

    public static List<Body> allBodies = new List<Body>();

    public Body(Vector3 position, Vector3 velocity, float radius, float mass, Color4 color, string name, bool emitsLight = false)
    {
        this.position = position;
        this.velocity = velocity * velocityOffset;
        this.radius = radius;
        this.mass = mass * massOffset;
        this.color = color;
        this.name = name;
        this.metaData = new BodyStruct(0, 0, color);
        this.emitsLight = emitsLight;
        allBodies.Add(this);


        
        count++;
        
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BodyStruct
    {
        public int head;
        public int count;
        public Vector2 pad;
        public Vector4 color;


        public BodyStruct(int head, int count, Color4 color)
        {
            this.head = head;
            this.count = count;
            this.color = (Vector4)color;
            this.pad = Vector2.Zero;
        }
    }

    

    public void OrbitalEnergy()
    {
        float energy = 0;
        foreach (Body body in allBodies)
        {
            if (this != body)
                {
                    energy += (Physics.gConstant * this.mass * body.mass) / Vector3.Distance(this.position, body.position);
                }
        }
        orbitalEnergy = (0.5f * this.mass * (float)MathHelper.Pow(velocity.Length, 2)) - energy;
        
    }

    public void TrailAdd()
    {
        
    }
}

