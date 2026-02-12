using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;



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
    public List<float> path = new List<float>();

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
}

