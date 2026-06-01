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

        private static (float speed, Vector3 velocity) COrbit(Body central, float radius)
        {
            Vector3 velocity;
            float speed = MathF.Sqrt((Physics.gConstant * central.mass) / radius) / Body.velocityOffset;
            if (central.velocity.Length <= 0.00001f)
            {
                velocity = new Vector3(0, 0, speed);
            }
            else
            {
                velocity = central.velocity.Normalized() * speed;
            }


            return (speed, velocity);
        }



        ///PMAstroids
        public static List<Action> systemMethods = new List<Action>
        {
            PMAstroids,
            PMAstroidsR,
            StarPlanet,
            BinaryStars,
            CircularBStars,
            ClaudeSolarSystem
        };

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
            int amount = 220 / 2;
            Body star = new Body(new Vector3(0),
                                new Vector3(0),
                                35f,
                                60000f,
                                Color4.Yellow,
                                "Planet A", true);
            Body planet = new Body(new Vector3(5000, 0, 0),
                                    COrbit(star, 5000).velocity,
                                    10,
                                    500,
                                    Color4.Blue,
                                    "Planet B");
            Body moon = new Body(planet.position + new Vector3(planet.radius * 15, 0, 0),
                                    COrbit(planet, planet.radius * 15).velocity,
                                    4,
                                    6.17f,
                                    Color4.White,
                                    "Moon");


            for (int i = -amount; i < amount; i++)
            {
                new Body(new Vector3(3500 + (i * 14), i, 0), // POSITION
                            new Vector3(0, i, COrbit(star, 3500 + (i  * 14)).speed + (i * 3)), // VELOCITY
                            2, // RADIUS
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
                                    COrbit(star, 500).velocity, // VELOCITY
                                    10, // RADIUS
                                    1, // MASS
                                    Color4.Blue, // COLOR
                                    "Planet"); // NAME
        }

        public static void BinaryStars()
        {
            Body starA = new Body(new Vector3(600, 0, 0), // POSITION
                                new Vector3(0, 0, 200), // VELOCITY
                                35f, // RADIUS
                                6000f, // MASS
                                Color4.Yellow, // COLOR
                                "Star A"); // NAME
            Body starB = new Body(-starA.position,
                                  -starA.velocity / Body.velocityOffset,
                                  starA.radius,
                                  starA.mass / Body.massOffset,
                                  Color4.OrangeRed,
                                  "Star B");
        }

        public static void CircularBStars()
        {
            Body starA = new Body(new Vector3(600, 0, 0), // POSITION
                                new Vector3(0, 0, 200), // VELOCITY
                                35f, // RADIUS
                                6000f, // MASS
                                Color4.Yellow, // COLOR
                                "Star A"); // NAME
            Body starB = new Body(-starA.position,
                                  -starA.velocity / Body.velocityOffset,
                                  starA.radius,
                                  starA.mass / Body.massOffset,
                                  Color4.OrangeRed,
                                  "Star B");

            starA.velocity = COrbit(starB, (starB.position - starA.position).Length).velocity;
            starB.velocity = COrbit(starA, (starA.position - starB.position).Length).velocity;
        }

        public static void ClaudeSolarSystem()
        {
            // Central star - massive and stationary
            Body sun = new Body(new Vector3(0), 
                               new Vector3(0), 
                               40f, 
                               80000f, 
                               Color4.Yellow, 
                               "Sun");

            // Inner rocky planet - fast orbit, no moons
            float mercuryDist = 800f;
            Body mercury = new Body(new Vector3(mercuryDist, 0, 0), 
                                   COrbit(sun, mercuryDist).velocity, 
                                   6f, 
                                   5f, 
                                   Color4.Gray, 
                                   "Mercury");

            // Second planet with slight inclination - demonstrates tilted orbital plane
            float venusDist = 1400f;
            float venusInclination = MathHelper.DegreesToRadians(15f); // 15° tilt
            Vector3 venusPos = new Vector3(venusDist * MathF.Cos(venusInclination), 
                                          venusDist * MathF.Sin(venusInclination), 
                                          0);
            Vector3 venusVel = COrbit(sun, venusDist).velocity;
            // Rotate velocity vector to match inclined orbit
            venusVel = new Vector3(0, 
                                  venusVel.Z * MathF.Sin(venusInclination), 
                                  venusVel.Z * MathF.Cos(venusInclination));
            Body venus = new Body(venusPos, 
                                 venusVel, 
                                 9f, 
                                 15f, 
                                 Color4.Orange, 
                                 "Venus");

            // Earth-like planet with one moon
            float earthDist = 2200f;
            Body earth = new Body(new Vector3(earthDist, 0, 0), 
                                 COrbit(sun, earthDist).velocity, 
                                 10f, 
                                 20f, 
                                 Color4.Blue, 
                                 "Earth");

            // Earth's moon - demonstrating moon orbital mechanics
            float moonDist = earth.radius * 18f;
            Body moon = new Body(earth.position + new Vector3(0, moonDist, 0), 
                                earth.velocity + new Vector3(COrbit(earth, moonDist).speed, 0, 0), 
                                3f, 
                                0.5f, 
                                Color4.LightGray, 
                                "Moon");

            // Mars with two small moons (like Phobos and Deimos)
            float marsDist = 3200f;
            Body mars = new Body(new Vector3(marsDist, 0, 0), 
                                COrbit(sun, marsDist).velocity, 
                                7f, 
                                8f, 
                                Color4.Red, 
                                "Mars");

            // Phobos - close, fast moon
            float phoboseDist = mars.radius * 12f;
            Body phobos = new Body(mars.position + new Vector3(phoboseDist, 0, 0), 
                                  mars.velocity + new Vector3(0, 0, COrbit(mars, phoboseDist).speed), 
                                  2f, 
                                  0.1f, 
                                  Color4.DarkGray, 
                                  "Phobos");

            // Deimos - farther, slower moon
            float deimosDist = mars.radius * 25f;
            Body deimos = new Body(mars.position + new Vector3(-deimosDist, 0, 0), 
                                  mars.velocity + new Vector3(0, 0, -COrbit(mars, deimosDist).speed), 
                                  1.5f, 
                                  0.05f, 
                                  Color4.SlateGray, 
                                  "Deimos");

            // Gas giant with multiple moons at different inclinations
            float jupiterDist = 5000f;
            Body jupiter = new Body(new Vector3(jupiterDist, 0, 0), 
                                   COrbit(sun, jupiterDist).velocity, 
                                   25f, 
                                   300f, 
                                   Color4.SandyBrown, 
                                   "Jupiter");

            // Io - innermost moon, flat orbit
            float ioDist = jupiter.radius * 15f;
            Body io = new Body(jupiter.position + new Vector3(0, 0, ioDist), 
                              jupiter.velocity + new Vector3(COrbit(jupiter, ioDist).speed, 0, 0), 
                              4f, 
                              2f, 
                              Color4.Yellow, 
                              "Io");

            // Europa - inclined orbit to show 3D motion
            float europaDist = jupiter.radius * 22f;
            float europaIncl = MathHelper.DegreesToRadians(25f);
            Vector3 europaPos = jupiter.position + new Vector3(0, 
                                                              europaDist * MathF.Sin(europaIncl), 
                                                              europaDist * MathF.Cos(europaIncl));
            Vector3 europaVel = jupiter.velocity + new Vector3(COrbit(jupiter, europaDist).speed, 
                                                              0, 
                                                              0);
            Body europa = new Body(europaPos, 
                                  europaVel, 
                                  3.5f, 
                                  1.2f, 
                                  Color4.White, 
                                  "Europa");

            // Ganymede - largest moon, different orbital plane
            float ganymedeDist = jupiter.radius * 30f;
            Body ganymede = new Body(jupiter.position + new Vector3(ganymedeDist, 0, 0), 
                                    jupiter.velocity + new Vector3(0, COrbit(jupiter, ganymedeDist).speed, 0), 
                                    5f, 
                                    3f, 
                                    Color4.Beige, 
                                    "Ganymede");

            // Outer planet - demonstrates slower outer orbits
            float saturnDist = 7500f;
            Body saturn = new Body(new Vector3(saturnDist, 0, 0), 
                                  COrbit(sun, saturnDist).velocity, 
                                  22f, 
                                  200f, 
                                  Color4.Wheat, 
                                  "Saturn");

            // Titan - large moon with retrograde orbit (opposite direction)
            float titanDist = saturn.radius * 28f;
            Body titan = new Body(saturn.position + new Vector3(0, titanDist, 0), 
                                 saturn.velocity + new Vector3(-COrbit(saturn, titanDist).speed, 0, 0), 
                                 5.5f, 
                                 3.5f, 
                                 Color4.OrangeRed, 
                                 "Titan");
        }
    }
}
