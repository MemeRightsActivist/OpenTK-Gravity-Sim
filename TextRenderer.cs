using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace OpenTKSim
{
    /// <summary>
    /// Renders text on screen using OpenGL. Uses a simple bitmap font approach
    /// where each character is rendered as a textured quad.
    /// 
    /// Key Concepts:
    /// - Screen coordinates: (0,0) is top-left, (width, height) is bottom-right
    /// - Each character is a quad (two triangles) with texture coordinates
    /// - Uses orthographic projection for 2D screen-space rendering
    /// - Renders in a separate pass after 3D scene (no depth testing)
    /// 
    /// Performance Notes:
    /// - Font texture is created once at startup (expensive operation)
    /// - DrawText() builds vertex data in C# (cheap) and uploads with BufferSubData (fast)
    /// - Single draw call per text string (efficient)
    /// - Perfect for frequently updated text like FPS counters
    /// </summary>
    public class TextRenderer
    {
        // OpenGL object handles
        private int vao;           // Vertex Array Object - stores vertex attribute configuration
        private int vbo;           // Vertex Buffer Object - stores vertex data
        private Shader textShader; // Shader program for rendering text
        private int fontTexture;   // Texture containing the font bitmap

        // Font configuration
        private int charWidth = 8;   // Width of each character in pixels
        private int charHeight = 8;  // Height of each character in pixels
        private int charsPerRow = 16; // Number of characters per row in font texture

        /// <summary>
        /// Initialize the text renderer. Call this once during startup.
        /// </summary>
        /// <param name="windowWidth">Width of the window in pixels</param>
        /// <param name="windowHeight">Height of the window in pixels</param>
        public TextRenderer(int windowWidth, int windowHeight)
        {
            // Create and compile the text rendering shader
            textShader = new Shader("Shaders/text.vert", "Shaders/text.frag");

            // Set up orthographic projection matrix for 2D screen coordinates
            // Maps (0,0) to top-left and (width, height) to bottom-right
            var projection = Matrix4.CreateOrthographicOffCenter(
                0.0f, windowWidth,      // Left and right edges
                windowHeight, 0.0f,     // Bottom and top edges (flipped for top-left origin)
                -1.0f, 1.0f);           // Near and far planes (don't matter for 2D)

            textShader.Use();
            int projLoc = GL.GetUniformLocation(textShader.Handle, "projection");
            GL.UniformMatrix4(projLoc, false, ref projection);

            // Create a simple readable bitmap font texture
            GenerateFontTexture();

            // Set up vertex array and buffer for dynamic text rendering
            // We'll update the VBO with new quad data for each DrawText call
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            // Reserve space for rendering text (6 vertices per character for 2 triangles)
            // Allocate enough for 200 characters at once (reasonable for most UI text)
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                sizeof(float) * 6 * 4 * 200, // 6 vertices * 4 floats per vertex * 200 chars
                IntPtr.Zero,
                BufferUsageHint.DynamicDraw);

            // Configure vertex attributes:
            // Each vertex has 4 floats: [x, y, texCoordX, texCoordY]

            // Position attribute (x, y)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(
                0,                                              // Attribute location
                2,                                              // 2 components (x, y)
                VertexAttribPointerType.Float,
                false,                                          // No normalization
                4 * sizeof(float),                              // Stride: 4 floats per vertex
                0);                                             // Offset: starts at beginning

            // Texture coordinate attribute (u, v)
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(
                1,                                              // Attribute location
                2,                                              // 2 components (u, v)
                VertexAttribPointerType.Float,
                false,                                          // No normalization
                4 * sizeof(float),                              // Stride: 4 floats per vertex
                2 * sizeof(float));                             // Offset: skip first 2 floats (x, y)

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Draw text at the specified screen coordinates.
        /// PERFORMANCE: Optimized for frequent updates (FPS counters, coordinates, etc.)
        /// - Vertex data built on CPU (fast)
        /// - Single BufferSubData upload (minimal GPU overhead)
        /// - Single draw call per string
        /// </summary>
        /// <param name="text">The string to render</param>
        /// <param name="x">X position in pixels (left edge of text)</param>
        /// <param name="y">Y position in pixels (top edge of text)</param>
        /// <param name="scale">Size multiplier (1.0 = normal size)</param>
        public void DrawText(string text, float x, float y, float scale = 1.0f)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // Save OpenGL state we'll modify
            bool depthTestEnabled = GL.IsEnabled(EnableCap.DepthTest);
            bool blendEnabled = GL.IsEnabled(EnableCap.Blend);

            // Configure OpenGL state for 2D text rendering
            GL.Disable(EnableCap.DepthTest); // Ignore depth buffer (always draw on top)
            GL.Enable(EnableCap.Blend);      // Enable transparency
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            textShader.Use();
            GL.BindVertexArray(vao);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, fontTexture);

            // Build vertex data for all characters in the string
            // Using List<float> is fine for text rendering - not a performance bottleneck
            List<float> vertices = new List<float>(text.Length * 24); // Pre-allocate
            float currentX = x;

            foreach (char c in text)
            {
                // Calculate which character this is in the ASCII table
                int charIndex = (int)c;

                // Calculate texture coordinates for this character in the font atlas
                // The font texture is a grid of characters (16x16 grid for 256 chars)
                int col = charIndex % charsPerRow;
                int row = charIndex / charsPerRow;

                // Texture coordinates (0.0 to 1.0 range)
                float texX = (float)col / charsPerRow;
                float texY = (float)row / charsPerRow;
                float texW = 1.0f / charsPerRow;
                float texH = 1.0f / charsPerRow;

                // Screen coordinates for this character's quad
                float charW = charWidth * scale;
                float charH = charHeight * scale;

                // Build two triangles (6 vertices) to form a quad for this character
                // Triangle 1: top-left, bottom-left, bottom-right
                // Triangle 2: top-left, bottom-right, top-right

                // Vertex format: [x, y, texU, texV]

                // Top-left
                vertices.Add(currentX);
                vertices.Add(y);
                vertices.Add(texX);
                vertices.Add(texY);

                // Bottom-left
                vertices.Add(currentX);
                vertices.Add(y + charH);
                vertices.Add(texX);
                vertices.Add(texY + texH);

                // Bottom-right
                vertices.Add(currentX + charW);
                vertices.Add(y + charH);
                vertices.Add(texX + texW);
                vertices.Add(texY + texH);

                // Top-left (again for second triangle)
                vertices.Add(currentX);
                vertices.Add(y);
                vertices.Add(texX);
                vertices.Add(texY);

                // Bottom-right (again)
                vertices.Add(currentX + charW);
                vertices.Add(y + charH);
                vertices.Add(texX + texW);
                vertices.Add(texY + texH);

                // Top-right
                vertices.Add(currentX + charW);
                vertices.Add(y);
                vertices.Add(texX + texW);
                vertices.Add(texY);

                // Move to next character position
                currentX += charW;
            }

            // Upload vertex data to GPU
            // BufferSubData is perfect for dynamic data - it reuses the existing buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferSubData(
                BufferTarget.ArrayBuffer,
                IntPtr.Zero,
                vertices.Count * sizeof(float),
                vertices.ToArray());

            // Draw all characters in a single call
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Count / 4); // 4 floats per vertex

            // Restore OpenGL state
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            if (!blendEnabled)
                GL.Disable(EnableCap.Blend);
            if (depthTestEnabled)
                GL.Enable(EnableCap.DepthTest);
        }

        /// <summary>
        /// Generate a simple readable bitmap font texture using a basic 8x8 pixel font.
        /// This creates ASCII characters 0-127 with simple pixel patterns.
        /// 
        /// EFFICIENCY: This is called ONCE at startup, so complexity doesn't matter.
        /// The resulting texture is cached on the GPU for the entire program lifetime.
        /// </summary>
        private void GenerateFontTexture()
        {
            int texWidth = charWidth * charsPerRow;
            int texHeight = charHeight * charsPerRow;
            byte[] pixels = new byte[texWidth * texHeight]; // Single channel (grayscale)

            // Initialize all to transparent
            Array.Fill<byte>(pixels, 0);

            // Define simple 8x8 pixel patterns for readable characters
            // Each character is defined as 8 bytes (one per row)
            // Bit = 1 means pixel is visible, 0 means transparent
            byte[][] fontData = new byte[128][];

            // Initialize all to empty
            for (int i = 0; i < 128; i++)
                fontData[i] = new byte[8];

            // Define readable characters (numbers and letters)
            // '0' = 48
            fontData[48] = new byte[] { 0x3C, 0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x00 };
            // '1' = 49
            fontData[49] = new byte[] { 0x18, 0x38, 0x18, 0x18, 0x18, 0x18, 0x7E, 0x00 };
            // '2' = 50
            fontData[50] = new byte[] { 0x3C, 0x66, 0x06, 0x0C, 0x18, 0x30, 0x7E, 0x00 };
            // '3' = 51
            fontData[51] = new byte[] { 0x3C, 0x66, 0x06, 0x1C, 0x06, 0x66, 0x3C, 0x00 };
            // '4' = 52
            fontData[52] = new byte[] { 0x0C, 0x1C, 0x2C, 0x4C, 0x7E, 0x0C, 0x0C, 0x00 };
            // '5' = 53
            fontData[53] = new byte[] { 0x7E, 0x60, 0x7C, 0x06, 0x06, 0x66, 0x3C, 0x00 };
            // '6' = 54
            fontData[54] = new byte[] { 0x3C, 0x60, 0x60, 0x7C, 0x66, 0x66, 0x3C, 0x00 };
            // '7' = 55
            fontData[55] = new byte[] { 0x7E, 0x06, 0x0C, 0x18, 0x30, 0x30, 0x30, 0x00 };
            // '8' = 56
            fontData[56] = new byte[] { 0x3C, 0x66, 0x66, 0x3C, 0x66, 0x66, 0x3C, 0x00 };
            // '9' = 57
            fontData[57] = new byte[] { 0x3C, 0x66, 0x66, 0x3E, 0x06, 0x06, 0x3C, 0x00 };

            // A-Z (uppercase)
            fontData[65] = new byte[] { 0x18, 0x3C, 0x66, 0x66, 0x7E, 0x66, 0x66, 0x00 }; // A
            fontData[66] = new byte[] { 0x7C, 0x66, 0x66, 0x7C, 0x66, 0x66, 0x7C, 0x00 }; // B
            fontData[67] = new byte[] { 0x3C, 0x66, 0x60, 0x60, 0x60, 0x66, 0x3C, 0x00 }; // C
            fontData[68] = new byte[] { 0x78, 0x6C, 0x66, 0x66, 0x66, 0x6C, 0x78, 0x00 }; // D
            fontData[69] = new byte[] { 0x7E, 0x60, 0x60, 0x7C, 0x60, 0x60, 0x7E, 0x00 }; // E
            fontData[70] = new byte[] { 0x7E, 0x60, 0x60, 0x7C, 0x60, 0x60, 0x60, 0x00 }; // F
            fontData[71] = new byte[] { 0x3C, 0x66, 0x60, 0x6E, 0x66, 0x66, 0x3C, 0x00 }; // G
            fontData[72] = new byte[] { 0x66, 0x66, 0x66, 0x7E, 0x66, 0x66, 0x66, 0x00 }; // H
            fontData[73] = new byte[] { 0x3C, 0x18, 0x18, 0x18, 0x18, 0x18, 0x3C, 0x00 }; // I
            fontData[74] = new byte[] { 0x1E, 0x0C, 0x0C, 0x0C, 0x0C, 0x6C, 0x38, 0x00 }; // J
            fontData[75] = new byte[] { 0x66, 0x6C, 0x78, 0x70, 0x78, 0x6C, 0x66, 0x00 }; // K
            fontData[76] = new byte[] { 0x60, 0x60, 0x60, 0x60, 0x60, 0x60, 0x7E, 0x00 }; // L
            fontData[77] = new byte[] { 0x63, 0x77, 0x7F, 0x6B, 0x63, 0x63, 0x63, 0x00 }; // M
            fontData[78] = new byte[] { 0x66, 0x76, 0x7E, 0x7E, 0x6E, 0x66, 0x66, 0x00 }; // N
            fontData[79] = new byte[] { 0x3C, 0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x00 }; // O
            fontData[80] = new byte[] { 0x7C, 0x66, 0x66, 0x7C, 0x60, 0x60, 0x60, 0x00 }; // P
            fontData[81] = new byte[] { 0x3C, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x0E, 0x00 }; // Q
            fontData[82] = new byte[] { 0x7C, 0x66, 0x66, 0x7C, 0x78, 0x6C, 0x66, 0x00 }; // R
            fontData[83] = new byte[] { 0x3C, 0x66, 0x60, 0x3C, 0x06, 0x66, 0x3C, 0x00 }; // S
            fontData[84] = new byte[] { 0x7E, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x00 }; // T
            fontData[85] = new byte[] { 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x00 }; // U
            fontData[86] = new byte[] { 0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x18, 0x00 }; // V
            fontData[87] = new byte[] { 0x63, 0x63, 0x63, 0x6B, 0x7F, 0x77, 0x63, 0x00 }; // W
            fontData[88] = new byte[] { 0x66, 0x66, 0x3C, 0x18, 0x3C, 0x66, 0x66, 0x00 }; // X
            fontData[89] = new byte[] { 0x66, 0x66, 0x66, 0x3C, 0x18, 0x18, 0x18, 0x00 }; // Y
            fontData[90] = new byte[] { 0x7E, 0x06, 0x0C, 0x18, 0x30, 0x60, 0x7E, 0x00 }; // Z

            // Common symbols
            fontData[32] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // Space
            fontData[33] = new byte[] { 0x18, 0x18, 0x18, 0x18, 0x00, 0x00, 0x18, 0x00 }; // !
            fontData[46] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x18, 0x00 }; // .
            fontData[44] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x18, 0x30 }; // ,
            fontData[58] = new byte[] { 0x00, 0x00, 0x18, 0x00, 0x00, 0x18, 0x00, 0x00 }; // :
            fontData[45] = new byte[] { 0x00, 0x00, 0x00, 0x7E, 0x00, 0x00, 0x00, 0x00 }; // -
            fontData[43] = new byte[] { 0x00, 0x18, 0x18, 0x7E, 0x18, 0x18, 0x00, 0x00 }; // +
            fontData[47] = new byte[] { 0x00, 0x06, 0x0C, 0x18, 0x30, 0x60, 0x00, 0x00 }; // /

            // Render each character into the texture
            for (int charCode = 0; charCode < 128; charCode++)
            {
                int charX = (charCode % charsPerRow) * charWidth;
                int charY = (charCode / charsPerRow) * charHeight;

                byte[] charPattern = fontData[charCode];

                for (int row = 0; row < 8; row++)
                {
                    byte rowData = charPattern[row];
                    for (int col = 0; col < 8; col++)
                    {
                        // Check if bit is set (character pixel is visible)
                        bool pixelOn = (rowData & (1 << (7 - col))) != 0;

                        int pixelX = charX + col;
                        int pixelY = charY + row;
                        int pixelIndex = pixelY * texWidth + pixelX;

                        pixels[pixelIndex] = pixelOn ? (byte)255 : (byte)0;
                    }
                }
            }

            // Create OpenGL texture
            fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fontTexture);

            // Upload pixel data to GPU (single-channel red, we'll use as alpha in shader)
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.R8,
                texWidth,
                texHeight,
                0,
                PixelFormat.Red,
                PixelType.UnsignedByte,
                pixels);

            // Set texture filtering (Nearest = sharp pixels, no blurring)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// Clean up OpenGL resources. Call this when shutting down.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            GL.DeleteTexture(fontTexture);
        }
    }
}
