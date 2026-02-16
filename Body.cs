using OpenTK.Mathematics;
using OpenTKSim;
using ScottPlot.DataViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public class Body
{
    public Vector3 position;
    public Vector3 velocity;
    public float radius;
    public float mass;
    public float orbitalEnergy;
    public Color4 color;
    public string name;
    public Sphere shape;
    public List<Vector3> path = new List<Vector3>();
    public static float[] allPaths;
    public static int pathIndex = 0;
    public static int pathCount = 0;
    public static uint[] trailIndices;
    public static int trailIndiceCount = 0;
    public static uint trailIndiceIndex = 0;
    public static bool trails = true;

    public static List<Body> allBodies = new List<Body>();

    public Body(Vector3 position, Vector3 velocity, float radius, float mass, Color4 color, string name)
    {
        this.position = position;
        this.velocity = velocity * 0.1f;
        this.radius = radius;
        this.mass = mass * 500f;
        this.color = color;
        this.name = name;

        shape = new Sphere(radius, 24, 12, color);
        allBodies.Add(this);
        allPaths = new float[allBodies.Count * 3 * 100000];
        trailIndices = new uint[allBodies.Count * 200000];

        
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
        this.path.Add(position);
    }
}

