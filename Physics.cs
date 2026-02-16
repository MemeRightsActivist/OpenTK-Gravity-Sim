using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

public class Physics
{
    public static float gConstant = 1f;
    static int dimensions = 3;
    public static float dTime = 1f / 120f;
    public int num = 0;


    public Physics()
    {

    }
    

    public static void Gravity(float step)
    {
        foreach (Body a in Body.allBodies)
        {
            foreach (Body b in Body.allBodies)
            {
                if (a != b)
                {
                    
                    float force = (((a.mass * b.mass) / Vector3.DistanceSquared(a.position, b.position)) * gConstant);
                    
                    a.velocity += ((Vector3.Normalize(b.position - a.position) * force) / a.mass) * step;
                    
                    
                }
            }
            
        }
        foreach (Body body in Body.allBodies)
        {
            body.position += body.velocity * step;

        }
        //Body.allBodies[2].position = new Vector3(0);
        
    }

    public static void Repeat()
    {
        foreach(Body body in Body.allBodies)
        {
            body.OrbitalEnergy();
        }
        Telemtry();
    }

    public static void Telemtry()
    {
        MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateOrOpen("MyMappedFile", 1024); // Name and size

        // Create a view accessor to write data
        using (MemoryMappedViewAccessor accessor = memoryMappedFile.CreateViewAccessor(0, 1024))
        {
            //string sFrame = $"{Game.frame}";
            //string time = $"{Game.stopwatch.Elapsed.TotalSeconds}";
            //string vel = $"{Body.allBodies[0].velocity.Length}";
            //string vel = $"{Body.allBodies[0].velocity.Length}";



            byte[] frameB = BitConverter.GetBytes(Game.frame);
            byte[] timeB = BitConverter.GetBytes(Game.stopwatch.Elapsed.TotalSeconds);
            byte[] velA = BitConverter.GetBytes(Body.allBodies[0].velocity.Length);
            byte[] velB = BitConverter.GetBytes(Body.allBodies[1].velocity.Length);
            byte[] energyA = BitConverter.GetBytes(Body.allBodies[0].orbitalEnergy);



            // Write the bytes to the memory-mapped file
            accessor.WriteArray(0, frameB, 0, frameB.Length);
            accessor.WriteArray(sizeof(int), timeB, 0, timeB.Length);
            accessor.WriteArray(sizeof(double) + sizeof(int), velA, 0, velA.Length);
            accessor.WriteArray(sizeof(double) + sizeof(int) + sizeof(float), velB, 0, velB.Length);
            accessor.WriteArray(sizeof(double) + sizeof(int) + sizeof(float) + sizeof(float), energyA, 0, energyA.Length);



            

        }
    }

}
