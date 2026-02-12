using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKSim
{
    public class Grid
    {
        int gridVAO;
        int gridVBO;
        int gridEBO;


        int gridLines;
        public float[] gridLineVertices;
        public static int[] secondVerts;
        float[] square;
        int squareN;
        float gridSpacing;
        float gridLength;

        public Grid()
        {
            // Build Grid
            gridSpacing = 30f;
            gridLines = 1000;
            gridLength = 10000f;
            gridLineVertices = new float[gridLines * 12];
            for (int i = 0; i < gridLineVertices.Length / 2; i += 6)
            {
                gridLineVertices[i] = gridLength;
                gridLineVertices[i + 1] = 0.0f;
                gridLineVertices[i + 2] = ((i / 6) * gridSpacing) - ((gridLines * gridSpacing) / 2);
                gridLineVertices[i + 3] = -gridLength;
                gridLineVertices[i + 4] = 0.0f;
                gridLineVertices[i + 5] = ((i / 6) * gridSpacing) - ((gridLines * gridSpacing) / 2);
            }

            for (int i = gridLineVertices.Length / 2; i < gridLineVertices.Length; i += 6)
            {
                gridLineVertices[i] = (((i - (gridLineVertices.Length / 2)) / 6) * gridSpacing) - ((gridLines * gridSpacing) / 2);
                gridLineVertices[i + 1] = 0.0f;
                gridLineVertices[i + 2] = gridLength;
                gridLineVertices[i + 3] = (((i - (gridLineVertices.Length / 2)) / 6) * gridSpacing) - ((gridLines * gridSpacing) / 2);
                gridLineVertices[i + 4] = 0.0f;
                gridLineVertices[i + 5] = -gridLength;
            }

            squareN = 128;
            square = new float[squareN * squareN * 3];
            for (int i = 0; i < squareN; i++)
            {
                for (int j = 0; j < squareN; j++)
                {
                    // Calculate starting index for this (i,j) point
                    int index = (i * squareN + j) * 3;

                    square[index + 0] = (float)i;     // x or whatever coordinate
                    square[index + 1] = (float)j;     // y or z
                    square[index + 2] = 0.0f;         // z or whatever
                                                      //Console.WriteLine($"{(float)i} {(float)j} {0.0f} Yeah");
                }
            }
        }

        public static void GridA()
        {
            secondVerts = new int[1000 * 1000 * 2];
            int scroll = 0;
            for (int i = 0; i < 1000; i++)
            {
                for (int j = 0; j < 1000; j++)
                {
                    secondVerts[scroll] = i;
                    secondVerts[scroll + 1] = j;
                    //Console.WriteLine($"{(float)i} {(float)j} {0.0f} Yeah");
                    scroll += 2;
                }
            }

            Console.WriteLine(secondVerts.Length);
            int width = 1000;
            var pairs = secondVerts.Chunk(2).Select(p => $"({p[0]},{p[1]})");
            foreach (var row in pairs.Chunk(width))
                Console.WriteLine(string.Join(" ", row));
        }
    }
}
