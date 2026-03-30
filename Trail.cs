using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKSim
{
    internal class Trail
    {
        // SSBO 0: trail positions — laid out as [body0_point0, body0_point1, ..., body1_point0, ...]
        public static Vector4[] trailArray;
        public static int trailsSSBO;

        // SSBO 1: per-body metadata (head, count, color)
        public static Body.BodyStruct[] bodyMeta;
        public static int bodyMetaSSBO;
        public static float opacity = 1f;
        public static int maxTrailLength = 10000;

        public static void TrailStart(int planets, int maxPoints)
        {
            // Position SSBO — one Vector4 per trail point per planet
            trailArray = new Vector4[planets * maxPoints];
            trailsSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, trailsSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, trailArray.Length * 16, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, trailsSSBO);

            // Body metadata SSBO
            bodyMeta = new Body.BodyStruct[planets];
            for (int i = 0; i < planets; i++)
                bodyMeta[i] = Body.allBodies[i].metaData;

            bodyMetaSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, bodyMetaSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, planets * 32, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, bodyMetaSSBO);
        }

        public static void TrailUpdate()
        {
            for (int i = 0; i < Body.count; i++)
            {
                ref Body.BodyStruct meta = ref Body.allBodies[i].metaData;

                // Write position + timestamp into this body's ring-buffer slot
                int index = i * maxTrailLength + meta.head;
                trailArray[index] = new Vector4(Body.allBodies[i].position, (float)Game.stopwatch.Elapsed.TotalSeconds);

                // Advance head (circular)
                meta.head = (meta.head + 1) % maxTrailLength;

                // Grow count until full
                if (meta.count < maxTrailLength)
                    meta.count++;

                bodyMeta[i] = meta;
            }

            // Upload positions
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, trailsSSBO);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, trailArray.Length * 16, trailArray);

            // Upload metadata
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, bodyMetaSSBO);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, bodyMeta.Length * 32, bodyMeta);
        }
    }
}