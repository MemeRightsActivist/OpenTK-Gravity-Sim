using OpenTK.Graphics.OpenGL4;
using ScottPlot.MultiplotLayouts;
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
        public float[] gridVertices;
        public int gridSize = 500;
        float gridSpacing = 100f;
        // Number of vertices per side (2 * gridSize because loop runs from -gridSize..gridSize-1)
        public int side;
        // Indices to draw grid lines as line segments (pairs of vertex indices)
        public uint[] gridIndices;

        public Grid()
        {
            gridVertices = new float[gridSize * gridSize * 12];
            int scroll = 0;
            for (int i = -gridSize; i < gridSize; i++)
            {
                for (int j = -gridSize; j < gridSize; j++)
                {
                    gridVertices[scroll] = (float)i * gridSpacing;
                    gridVertices[scroll + 1] = 0f;
                    gridVertices[scroll + 2] = (float)j * gridSpacing;

                    
                    scroll += 3;
                }
            }

            // Build index buffer for drawing grid as line segments (wireframe)
            // side = number of vertices per side
            side = 2 * gridSize;

            // For each row there are (side - 1) horizontal segments -> side * (side - 1)
            // Same for vertical segments. Total segments = 2 * side * (side - 1)
            // Each segment is two indices, so total indices = 4 * side * (side - 1)
            int indicesCount = 4 * side * (side - 1);
            gridIndices = new uint[indicesCount];
            int idx = 0;

            // Horizontal segments (left -> right)
            for (int r = 0; r < side; r++)
            {
                for (int c = 0; c < side - 1; c++)
                {
                    uint a = (uint)(r * side + c);
                    uint b = (uint)(r * side + c + 1);
                    gridIndices[idx++] = a;
                    gridIndices[idx++] = b;
                }
            }

            // Vertical segments (top -> bottom)
            for (int c = 0; c < side; c++)
            {
                for (int r = 0; r < side - 1; r++)
                {
                    uint a = (uint)(r * side + c);
                    uint b = (uint)((r + 1) * side + c);
                    gridIndices[idx++] = a;
                    gridIndices[idx++] = b;
                }
            }
        }

        

        public static void GridA()
        {
            
        }
    }
}
