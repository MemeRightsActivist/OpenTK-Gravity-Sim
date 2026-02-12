using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKSim
{
    internal class Trail
    {
        public static List<Trail> trails = new List<Trail>();
        //public static Sphere trailSphere = new Sphere);
        public Vector3 position;


        public Trail(Vector3 position)
        {
            this.position = position;
            trails.Add(this);
        }
    }
}
