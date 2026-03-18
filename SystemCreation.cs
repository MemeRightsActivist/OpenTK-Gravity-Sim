using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKSim
{
    internal class SystemCreation
    {

        static private Random random = Game.random;

        private static float COrbit(Body central, float radius)
        {

            float velocity = MathF.Sqrt((Physics.gConstant * central.mass) / radius);
            return velocity / Body.velocityOffset;
        }
        public static void PMAstroidsR()
        {
            Body planetA = new Body(new Vector3(0),
                                new Vector3(0),
                                35f,
                                60000f,
                                Color4.Yellow,
                                "Planet A");
            Body planetB = new Body(new Vector3(5000, 0, 0),
                                    new Vector3(0, 0, 900),
                                    10,
                                    500,
                                    Color4.Blue,
                                    "Planet B");
            Body planetC = new Body(planetB.position + new Vector3(planetB.radius * 20, 0, 0),
                                    planetB.velocity + new Vector3(0, 0, 1200),
                                    4,
                                    6.17f,
                                    Color4.White,
                                    "Moon");


            for (int i = -50; i < 50; i++)
            {
                new Body(new Vector3(3500 + (i * 14), i, 0), // POSITION
                            new Vector3(0, i, 500 + (i * 5)), // VELOCITY
                            random.Next(1, 6), // RADIUS
                            (float)(random.NextDouble() * (0.4f - 0.01f) + 0.01f), // MASS
                            Color4.Gray, // COLOR
                            $"Asteroid {i}"); // NAME
            }
        }

        public static void PMAstroids()
        {
            Body planetA = new Body(new Vector3(0),
                                new Vector3(0),
                                35f,
                                60000f,
                                Color4.Yellow,
                                "Planet A");
            Body planetB = new Body(new Vector3(5000, 0, 0),
                                    new Vector3(0, 0, 900),
                                    10,
                                    500,
                                    Color4.Blue,
                                    "Planet B");
            Body planetC = new Body(planetB.position + new Vector3(planetB.radius * 20, 0, 0),
                                    planetB.velocity + new Vector3(0, 0, 1200),
                                    4,
                                    6.17f,
                                    Color4.White,
                                    "Moon");


            for (int i = -50; i < 50; i++)
            {
                new Body(new Vector3(3500 + (i * 14), i, 0), // POSITION
                            new Vector3(0, i, 500 + (i * 5)), // VELOCITY
                            5, // RADIUS
                            0.01f, // MASS
                            Color4.Gray, // COLOR
                            $"Asteroid {i}"); // NAME
            }
        }

        public static void StarPlanet()
        {
            Body star = new Body(new Vector3(0), // POSITION
                                new Vector3(0), // VELOCITY
                                35f, // RADIUS
                                60000f, // MASS
                                Color4.Yellow, // COLOR
                                "Star"); // NAME
            Body planet = new Body(new Vector3(500, 0, 0), // POSITION
                                    new Vector3(0, COrbit(star, 500) * MathF.Sin(45), COrbit(star, 500) * MathF.Cos(45)), // VELOCITY
                                    10, // RADIUS
                                    1, // MASS
                                    Color4.Blue, // COLOR
                                    "Planet"); // NAME
        }
    }
}
