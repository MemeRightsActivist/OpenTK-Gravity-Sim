using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using System;
using System.IO;

public class Texture
{
	public int Handle;
	public Texture(string path)
	{
        Handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        StbImage.stbi_set_flip_vertically_on_load(1);


        // Texture parameters (required!)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // Load image (example with StbImageSharp)
        StbImageSharp.ImageResult image;
        using (var fs = File.OpenRead(path))
        {
            image = StbImageSharp.ImageResult.FromStream(fs, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
        }

        // Upload pixels
        GL.TexImage2D(TextureTarget.Texture2D, level: 0,
            internalformat: PixelInternalFormat.Rgba,
            width: image.Width, height: image.Height, border: 0,
            format: PixelFormat.Rgba, type: PixelType.UnsignedByte,
            pixels: image.Data);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

    }
}
